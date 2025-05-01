using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PDF_API.Services;
using System.Text.Json.Serialization;
using PDF_API.Models.RequestModels;
using PDF_API.Models.ResponseModels;
using Logic;

namespace PDF_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PDFGeneratorController : ControllerBase
    {
        private readonly APIController _logicApiController;
        private readonly MessagingService _messagingService;

        private readonly Logic.InputService _inputService;

        public PDFGeneratorController(APIController logicApiController, MessagingService messagingService, Logic.InputService inputService)
        {
            _logicApiController = logicApiController ?? throw new ArgumentNullException(nameof(logicApiController));
            _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
            _inputService = inputService;
        }

        private int Pages;
        private int Size;
        private string? ByteUnit;
        private string? MetricUnit;
        private int Square;

        [Route("GenerateStart")]
        [HttpPost]
        public async Task<IActionResult> StartGenerationAsync([FromBody] RequestModel request)
        {
            var validationResult = _inputService.CleanseInputs(request.Pages, request.Size, request.ByteUnit, request.MetricUnit);

            if (validationResult.ErrorMessage != null)
            {
                return BadRequest(validationResult.ErrorMessage);
            }
            else
            {
                Pages = validationResult.Pages ?? 0;
                Size = validationResult.Size ?? 0;
                ByteUnit = validationResult.ByteUnit;
            }

            long estimatedSize = -1;
            try
            {
                estimatedSize = _logicApiController.EstimateSize(Pages, Size, ByteUnit);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling EstimateSize: {ex.Message}");
                return StatusCode(500, new GenerateStartResponse { Status = false, Message = "Failed to estimate PDF size." });
            }

            if (estimatedSize < 0)
            {
                return StatusCode(500, new GenerateStartResponse { Status = false, Message = "Estimation resulted in an invalid size." });
            }


            //Send Response to client
            var startResponse = new GenerateStartResponse
            {
                Status = true,
                Data = new GenerateStartData
                {
                    EstimatedSize = estimatedSize,
                    ByteUnit = this.ByteUnit!
                },
                Message = "Started PDF Generation"
            };

            //Start Generation 
            int pagesToGenerate = this.Pages;
            int sizeToGenerate = this.Size;
            string byteUnitForGeneration = this.ByteUnit!;
            string processId = Guid.NewGuid().ToString();

            // Start process notification
            await _messagingService.StartProcess(processId, "PDF Generation", request.User ?? "Anonymous", new Dictionary<string, object>
            {
                { "pages", pagesToGenerate },
                { "size", sizeToGenerate },
                { "byteUnit", byteUnitForGeneration }
            });

            _ = Task.Run(async () => // Run background task
            {
                long actualSize = -1;
                bool success = false;
                string resultMessage;

                try
                {
                    Console.WriteLine($"Background task started: {pagesToGenerate} pages, {sizeToGenerate} {byteUnitForGeneration}.");

                    // Update progress
                    await _messagingService.UpdateProcessProgress(processId, 25, "Estimating size");

                    //Call the main logic
                    actualSize = _logicApiController.HandleRequest(pagesToGenerate, sizeToGenerate, byteUnitForGeneration);

                    if (actualSize >= 0)
                    {
                        success = true;
                        resultMessage = "Successfully generated the PDF";
                        Console.WriteLine($"Background task SUCCESS. Actual size: {actualSize} bytes.");

                        // Update progress to 100% and complete
                        await _messagingService.UpdateProcessProgress(processId, 100, "Completed");
                        await _messagingService.CompleteProcess(processId, null, new Dictionary<string, object>
                        {
                            { "actualSize", actualSize },
                            { "byteUnit", byteUnitForGeneration }
                        });
                    }
                    else
                    {
                        resultMessage = "PDF generation failed in the logic layer.";
                        Console.WriteLine($"Background task FAILED: HandleRequest returned {actualSize}.");
                        await _messagingService.FailProcess(processId, resultMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during background PDF generation: {ex.ToString()}");
                    resultMessage = $"An error occurred during PDF generation: {ex.Message}";
                    actualSize = -1;
                    success = false;
                    await _messagingService.FailProcess(processId, resultMessage);
                }
            }); // End of Task.Run

            // Return the initial "Started" response immediately
            return Ok(startResponse);
        }

        [Route("GenerateBatchStart")]
        [HttpPost]
        public async Task<IActionResult> StartBatchGenerationAsync([FromBody] PDF_API.Models.RequestModels.BatchRequestModel request)
        {
            // Validate inputs
            if (request.NumberOfFiles == null || request.NumberOfFiles <= 0)
                return BadRequest("NumberOfFiles must be greater than zero.");
            if (request.PagesPerFile == null || request.PagesPerFile <= 0)
                return BadRequest("PagesPerFile must be greater than zero.");
            if (request.Size == null || request.Size <= 0)
                return BadRequest("Size must be greater than zero.");

            var validationResult = _inputService.CleanseInputs(request.PagesPerFile, request.Size, request.ByteUnit, request.MetricUnit);
            if (validationResult.ErrorMessage != null)
            {
                return BadRequest(validationResult.ErrorMessage);
            }

            int pagesPerFile = validationResult.Pages ?? 0;
            int size = validationResult.Size ?? 0;
            string byteUnit = validationResult.ByteUnit!;
            int numberOfFiles = request.NumberOfFiles.Value;

            // Estimate size per file
            long estimatedSizePerFile = -1;
            try
            {
                estimatedSizePerFile = _logicApiController.EstimateSize(pagesPerFile, size, byteUnit);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling EstimateSize: {ex.Message}");
                return StatusCode(500, new PDF_API.Models.ResponseModels.BatchGenerateStartResponse { Status = false, Message = "Failed to estimate PDF size for batch." });
            }

            if (estimatedSizePerFile < 0)
            {
                return StatusCode(500, new PDF_API.Models.ResponseModels.BatchGenerateStartResponse { Status = false, Message = "Estimation resulted in an invalid size." });
            }

            // Send response to client
            var startResponse = new PDF_API.Models.ResponseModels.BatchGenerateStartResponse
            {
                Status = true,
                Data = new PDF_API.Models.ResponseModels.BatchGenerateStartData
                {
                    EstimatedSizePerFile = estimatedSizePerFile,
                    ByteUnit = byteUnit,
                    NumberOfFiles = numberOfFiles,
                    PagesPerFile = pagesPerFile
                },
                Message = "Started Batch PDF Generation"
            };

            string processId = Guid.NewGuid().ToString();
            await _messagingService.StartProcess(processId, "Batch PDF Generation", request.User ?? "Anonymous", new Dictionary<string, object>
            {
                { "numberOfFiles", numberOfFiles },
                { "pagesPerFile", pagesPerFile },
                { "size", size },
                { "byteUnit", byteUnit }
            });

            // Fire and forget background task to return actual size
            _ = Task.Run(async () =>
            {
                long[] actualSizes = Array.Empty<long>();
                string resultMessage;
                try
                {
                    await _messagingService.UpdateProcessProgress(processId, 25, "Estimating size");
                    actualSizes = _logicApiController.HandleBatchRequest(numberOfFiles, pagesPerFile, size, byteUnit);
                    if (actualSizes.Length == numberOfFiles)
                    {
                        resultMessage = "Successfully generated all PDFs in batch.";
                        await _messagingService.UpdateProcessProgress(processId, 100, "Completed");
                        await _messagingService.CompleteProcess(processId, null, new Dictionary<string, object>
                        {
                            { "actualSizes", actualSizes },
                            { "byteUnit", byteUnit }
                        });
                    }
                    else
                    {
                        resultMessage = "Batch PDF generation failed for some files.";
                        await _messagingService.FailProcess(processId, resultMessage);
                    }
                }
                catch (Exception ex)
                {
                    resultMessage = $"An error occurred during batch PDF generation: {ex.Message}";
                    await _messagingService.FailProcess(processId, resultMessage);
                }
            });

            return Ok(startResponse);
        }
    }
}