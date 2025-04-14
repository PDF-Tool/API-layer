using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Logic;
using PDF_API.Services;
using System.Text.Json.Serialization;

namespace PDF_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PDFGeneratorController : ControllerBase
    {
        private readonly APIController _logicApiController;
        private readonly MessagingService _messagingService;

        public PDFGeneratorController(APIController logicApiController, MessagingService messagingService)
        {
            _logicApiController = logicApiController ?? throw new ArgumentNullException(nameof(logicApiController));
            _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
        }

        private enum ByteUnits
        {
            GB,
            MB
        };

        private enum MetricUnits
        {
            mm,
            cm
        }

        private int Pages;
        private int Size;
        private string? ByteUnit;
        private string? MetricUnit;
        private int Square;

        //Request Model
        public class RequestModel
        {
            public int? Pages { get; set; }
            public int? Size { get; set; } // REQUIRED
            public string? ByteUnit { get; set; } // REQUIRED
            public int? Width { get; set; }
            public int? Height { get; set; }
            public string? MetricUnit { get; set; }
            public int? Square { get; set; }
            public string? Format { get; set; }
            
            public string? User { get; set; }
        }

        //Response Models
        public class GenerateStartResponse
        {
            [JsonPropertyName("status")]
            public bool Status { get; set; }

            [JsonPropertyName("data")]
            public GenerateStartData? Data { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;
        }

        public class GenerateStartData
        {
            [JsonPropertyName("estimatedSize")]
            public long EstimatedSize { get; set; }

            [JsonPropertyName("byteUnit")]
            public string ByteUnit { get; set; } = string.Empty;
        }

        public class GenerateResultResponse
        {
            [JsonPropertyName("status")]
            public bool Status { get; set; }

            [JsonPropertyName("data")]
            public GenerateResultData? Data { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;
        }

        public class GenerateResultData
        {
            [JsonPropertyName("actualSize")]
            public long ActualSize { get; set; } 

            [JsonPropertyName("byteUnit")]
            public string ByteUnit { get; set; } = string.Empty;
        }


        [Route("GenerateStart")]
        [HttpPost]
        public async Task<IActionResult> StartGenerationAsync([FromBody] RequestModel request)
        {
            /*IActionResult validationResult = CleanseInputs(request);

            if (validationResult != null)
            {
                string errorMessage = "Input validation failed.";
                if (validationResult is BadRequestObjectResult badRequestResult && badRequestResult.Value is string msg)
                {
                    errorMessage = msg;
                }
                 else if (validationResult is ContentResult contentResult) // Handle simple BadRequest("message")
                {
                     errorMessage = contentResult.Content ?? errorMessage;
                }

                return BadRequest(new GenerateStartResponse { Status = false, Message = errorMessage });
            }
            */

            long estimatedSize = -1;
            try
            {
                estimatedSize = _logicApiController.EstimateSize(this.Pages, this.Size, this.ByteUnit!);
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
    }
}

/*
            if (request.Width.HasValue || request.Height.HasValue)
            {
                return CheckWidthAndHeight(Pages, request.Width, request.Height, MetricUnit, ByteUnit, Size);
            }

            if (request.Square.HasValue)
            {
                Square = CheckIfNullOrHigherThanZero(request.Square);
                return Content($"{Pages} pages of {Square} {MetricUnit}² and {Size} {ByteUnit} each");
            }

            if (request.Format != null)
            {
                return Content("Pages"); // Placeholder? What should happen here?
            }

        private IActionResult CheckWidthAndHeight(int? Pages, int? Width, int? Height, string? MetricUnit, string? ByteUnit, int? Size)
        {
            if (Width.HasValue && !Height.HasValue)
            {
                return BadRequest("Width given but no height");
            }
            else if (Height.HasValue && !Width.HasValue)
            {
                return BadRequest("Heigt given but no width");
            }
            else
            {
                var checkedWidth = CheckIfNullOrHigherThanZero(Width);
                var checkedHeight = CheckIfNullOrHigherThanZero(Height);


                Console.WriteLine($"Debug: Width/Height specified ({checkedWidth}x{checkedHeight} {MetricUnit}). Not used in generation.");

                return BadRequest(new GenerateStartResponse{ Status=false, Message="Width/Height specific generation not implemented in this flow yet."}); // Example response
            }
        }

        private int CheckIfNullOrHigherThanZero(int? value)
        {
            if (value.HasValue && value.Value > 0)
            {
                return value.Value;
            }
            else
            {
                return 1; // Default to 1 page if not specified or invalid
            }
        }

        private string? CheckByteUnit(string? byteUnitInput) // Changed name slightly to avoid conflict
        {
            if (string.IsNullOrWhiteSpace(byteUnitInput))
            {
                 return null;
            }
            else
            {
                string upperByteUnit = byteUnitInput.Trim().ToUpper();
                if (Enum.IsDefined(typeof(ByteUnits), upperByteUnit) && !int.TryParse(upperByteUnit, out _))
                {
                    return upperByteUnit; // Valid unit
                }
                else
                {
                    return null; // Invalid unit
                }
            }
        }

        private string? CheckMetricUnit(string? metricUnitInput) // Changed name slightly
        {
            if (string.IsNullOrWhiteSpace(metricUnitInput))
            {
                return MetricUnits.mm.ToString(); // Default ok if optional
            }
            else
            {
                string lowerMetricUnit = metricUnitInput.Trim().ToLower();
                if (Enum.IsDefined(typeof(MetricUnits), lowerMetricUnit) && !int.TryParse(lowerMetricUnit, out _))
                {
                    return lowerMetricUnit; // Valid
                }
                else
                {
                    return null; // Invalid
                }
            }
        }

        private IActionResult? CleanseInputs(RequestModel request) // Return type allows returning error results
        {
            Pages = CheckIfNullOrHigherThanZero(request.Pages);

            if (!request.Size.HasValue)
            {
                return BadRequest("No size given"); // Simplified: returning string directly for now
            }

            Size = CheckIfNullOrHigherThanZero(request.Size);


            var byteUnitResult = CheckByteUnit(request.ByteUnit);
            if (byteUnitResult == null)
            {
                return BadRequest("Byte unit specified is invalid or missing, only MB and GB are allowed");
            }
            ByteUnit = byteUnitResult; 


            var metricUnitResult = CheckMetricUnit(request.MetricUnit);
            if (metricUnitResult == null)
            {
                return BadRequest("Metric unit specified is invalid, only mm and cm are allowed");
            }
            MetricUnit = metricUnitResult; 

            if (request.Square.HasValue)
            {
                 this.Square = CheckIfNullOrHigherThanZero(request.Square);
                 Console.WriteLine($"Debug: Square parameter provided ({this.Square}). Not used in generation.");
            }

             if (request.Format != null)
             {
                  Console.WriteLine($"Debug: Format parameter provided ({request.Format}). Not used in generation.");
             }


            if (request.Width.HasValue || request.Height.HasValue)
            {
                 if ((request.Width.HasValue && !request.Height.HasValue) || (!request.Width.HasValue && request.Height.HasValue))
                 {
                     return BadRequest("Both Width and Height must be provided together."); // Structured error
                 }
                  Console.WriteLine($"Debug: Width/Height parameters provided. Not used in generation.");
                  // If valid Width/Height were provided, the original code returned Content().
                  // Now, we just note them and proceed with page/size/byteunit generation.
            }

            return null;
        }
    }
}*/