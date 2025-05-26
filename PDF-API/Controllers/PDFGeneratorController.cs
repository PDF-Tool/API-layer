// PDF-API/PDFGeneratorController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PDF_API.Services;
using PDF_API.Models.RequestModels;
using PDF_API.Models.ResponseModels;
using PDF_API.Models;
using Logic;
using System.IO;
using System.Net.Mime;
using Microsoft.Extensions.Configuration; // Required for IConfiguration
using System.Linq;
using System.Diagnostics; // Required for LINQ in batch response
using System.Text;

namespace PDF_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PDFGeneratorController : ControllerBase
    {
        private readonly APIController _logicApiController;
        private readonly MessagingService _messagingService;
        private readonly Logic.InputService _inputService;
        private readonly IConfiguration _configuration; // Inject configuration


        public PDFGeneratorController(
            APIController logicApiController,
            MessagingService messagingService,
            Logic.InputService inputService,
            IConfiguration configuration) // Add IConfiguration
        {
            _logicApiController = logicApiController ?? throw new ArgumentNullException(nameof(logicApiController));
            _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration)); // Store configuration
        }

        // --- Endpoint for Single PDF Generation and Printing ---
        [Route("GenerateAndPrint")] // Renamed endpoint
        [HttpPost]
        public async Task<IActionResult> GenerateAndPrintAsync([FromBody] RequestModel request)
        {
            try
            {
                // 1. Validate Input
                var validationResult = _inputService.CleanseInputs(request.Pages, request.SizePerPage, request.ByteUnit, request.MetricUnit);
                if (validationResult.ErrorMessage != null)
                {
                    return BadRequest(new GenerateStartResponse { Status = false, Message = validationResult.ErrorMessage });
                }

                int pages = validationResult.Pages ?? 1;
                int sizePerPage = validationResult.Size ?? 1;
                string byteUnit = validationResult.ByteUnit!;

                // 2. Get LPR Configuration
                string lprHost = request.Host;
                string lprQueue = "myque";
                if (!int.TryParse(":", out int lprPort))
                {
                    lprPort = 515; // Default LPR port
                }

                if (string.IsNullOrWhiteSpace(lprHost) || string.IsNullOrWhiteSpace(lprQueue))
                {
                    return StatusCode(500, new GenerateStartResponse { Status = false, Message = "LPR Host or Queue not configured on the server." });
                }

                // 3. Prepare Initial Response & Start Background Task
                string processId = Guid.NewGuid().ToString();
                var startResponse = new GenerateStartResponse
                {
                    Status = true,
                    ProcessId = processId,
                    Message = "Print job accepted. Processing in background."
                };

                // Notify client via SignalR that process started
                await _messagingService.StartProcess(processId, "PDF Print Job", request.User ?? "Anonymous", new Dictionary<string, object>
                {
                    { "action", "GenerateAndPrint" },
                    { "pages", pages },
                    { "sizePerPage", sizePerPage },
                    { "byteUnit", byteUnit },
                    { "lprHost", lprHost },
                    { "lprQueue", lprQueue }
                });

                // Run the actual generation and printing in the background
                _ = Task.Run(async () =>
                {
                    string resultMessage = "Print job failed.";
                    bool success = false;
                    string generatedFileName = "Unknown.pdf";
                    var resultDetails = new Dictionary<string, object>();

                    try
                    {
                        await _messagingService.UpdateProcessProgress(processId, 10, "Initializing print job...");

                        // Call the logic controller method that handles generation AND printing
                        var (printSuccess, message, fileName) = await _logicApiController.HandlePrintRequestAsync(
                            pages, sizePerPage, byteUnit, lprHost, lprQueue, lprPort);

                        success = printSuccess;
                        resultMessage = message;
                        generatedFileName = fileName ?? generatedFileName;
                        resultDetails.Add("fileName", generatedFileName);

                        if (success)
                        {
                            await _messagingService.UpdateProcessProgress(processId, 100, "Print job sent successfully.");
                            await _messagingService.CompleteProcess(processId, resultMessage, resultDetails);
                        }
                        else
                        {
                            await _messagingService.UpdateProcessProgress(processId, 100, "Print job failed.");
                            await _messagingService.FailProcess(processId, resultMessage, resultDetails);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Background Print Task Error (Process ID: {processId}): {ex.ToString()}");
                        resultMessage = $"An internal error occurred: {ex.Message}";
                        resultDetails.Add("error", ex.Message);
                        await _messagingService.FailProcess(processId, resultMessage, resultDetails);
                    }
                });

                return Ok(startResponse); // Return immediately
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Failed to start print job",
                    Message = ex.Message
                });
            }
        }


        // --- Endpoint for Batch PDF Generation and Printing ---
        [Route("GenerateBatchAndPrint")] // Renamed endpoint
        [HttpPost]
        public async Task<IActionResult> GenerateBatchAndPrintAsync([FromBody] BatchRequestModel request)
        {
            try
            {
                // 1. Validate Input
                if (request.NumberOfFiles == null || request.NumberOfFiles <= 0) return BadRequest("NumberOfFiles must be greater than zero.");
                if (request.PagesPerFile == null || request.PagesPerFile <= 0) return BadRequest("PagesPerFile must be greater than zero.");
                if (request.SizePerPage == null || request.SizePerPage <= 0) return BadRequest("SizePerPage must be greater than zero.");

                var validationResult = _inputService.CleanseInputs(request.PagesPerFile, request.SizePerPage, request.ByteUnit, request.MetricUnit);
                if (validationResult.ErrorMessage != null)
                {
                    return BadRequest(new BatchGenerateStartResponse { Status = false, Message = validationResult.ErrorMessage });
                }

                int pagesPerFile = validationResult.Pages ?? 1;
                int sizePerPage = validationResult.Size ?? 1;
                string byteUnit = validationResult.ByteUnit!;
                int numberOfFiles = request.NumberOfFiles.Value;

                // 2. Get LPR Configuration
                string lprHost = request.Host;
                string lprQueue = "myque";
                if (!int.TryParse(":", out int lprPort))
                {
                    lprPort = 515; // Default LPR port
                }

                if (string.IsNullOrWhiteSpace(lprHost) || string.IsNullOrWhiteSpace(lprQueue))
                {
                    return StatusCode(500, new BatchGenerateStartResponse { Status = false, Message = "LPR Host or Queue not configured on the server." });
                }

                // 3. Prepare Initial Response & Start Background Task
                string processId = Guid.NewGuid().ToString();
                var startResponse = new BatchGenerateStartResponse
                {
                    Status = true,
                    ProcessId = processId,
                    Data = new BatchGenerateStartData { NumberOfFiles = numberOfFiles, PagesPerFile = pagesPerFile },
                    Message = "Batch print job accepted. Processing in background."
                };

                // Notify client
                await _messagingService.StartProcess(processId, "Batch PDF Print Job", request.User ?? "Anonymous", new Dictionary<string, object>
                {
                    { "action", "GenerateBatchAndPrint" },
                    { "numberOfFiles", numberOfFiles },
                    { "pagesPerFile", pagesPerFile },
                    { "sizePerPage", sizePerPage },
                    { "byteUnit", byteUnit },
                    { "lprHost", lprHost },
                    { "lprQueue", lprQueue }
                });

                // Run batch in background
                _ = Task.Run(async () =>
                {
                    string resultMessage = "Batch print job failed.";
                    bool overallSuccess = false;
                    var resultDetails = new Dictionary<string, object>();

                    try
                    {
                        await _messagingService.UpdateProcessProgress(processId, 10, "Initializing batch print job...");

                        int successCount = 0;
                        var errorMessages = new StringBuilder();

                        for (int i = 0; i < numberOfFiles; i++)
                        {
                            try
                            {
                                // Process this single file
                                var (printSuccess, message, fileName) = await _logicApiController.HandlePrintRequestAsync(
                                    pagesPerFile, sizePerPage, byteUnit, lprHost, lprQueue, lprPort);

                                if (printSuccess)
                                {
                                    successCount++;
                                }
                                else
                                {
                                    errorMessages.AppendLine($"File {i + 1}: {message}");
                                }

                                // Update progress after each file
                                int progressPercent = (int)(((i + 1) * 100) / numberOfFiles);
                                await _messagingService.UpdateProcessProgress(
                                    processId,
                                    progressPercent,
                                    $"Processed {i + 1} of {numberOfFiles} files. Success: {successCount}, Errors: {i + 1 - successCount}"
                                );
                            }
                            catch (Exception ex)
                            {
                                errorMessages.AppendLine($"File {i + 1}: {ex.Message}");
                            }
                        }

                        overallSuccess = successCount == numberOfFiles;
                        resultMessage = overallSuccess
                            ? "All files processed successfully."
                            : $"Processed {successCount} of {numberOfFiles} files. Errors: {errorMessages}";

                        resultDetails.Add("totalFiles", numberOfFiles);
                        resultDetails.Add("successCount", successCount);
                        resultDetails.Add("failureCount", numberOfFiles - successCount);

                        if (overallSuccess)
                        {
                            await _messagingService.UpdateProcessProgress(processId, 100, "Batch print job completed successfully.");
                            await _messagingService.CompleteProcess(processId, resultMessage, resultDetails);
                        }
                        else
                        {
                            await _messagingService.UpdateProcessProgress(processId, 100, "Batch print job finished with errors.");
                            await _messagingService.FailProcess(processId, resultMessage, resultDetails);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Background Batch Print Task Error (Process ID: {processId}): {ex.ToString()}");
                        resultMessage = $"An internal error occurred during batch print: {ex.Message}";
                        resultDetails.Add("error", ex.Message);
                        await _messagingService.FailProcess(processId, resultMessage, resultDetails);
                    }
                });

                return Ok(startResponse); // Return immediately
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Failed to start batch print job",
                    Message = ex.Message
                });
            }
        }


        // --- Endpoint for Random PDF Generation and Printing ---
        [Route("GenerateRandomAndPrint")] // Renamed endpoint
        [HttpPost]
        public async Task<IActionResult> GenerateRandomAndPrintAsync([FromBody] RandomRequestModel request)
        {
            try
            {
                // 1. Basic Validation & Random Value Generation
                int sizeMin = request.SizeMin;
                int sizeMax = request.SizeMax;
                int pageMin = request.PageMin;
                int pageMax = request.PageMax;
                string mode = request.Mode ?? "single";
                int numberOfFiles = request.NumberOfFiles ?? 1;
                // Use InputService for cleansing units
                string byteUnit = _inputService.CleanseInputs(null, null, request.ByteUnit, null).ByteUnit ?? "MB";
                string metricUnit = _inputService.CleanseInputs(null, null, null, request.MetricUnit).MetricUnit ?? "mm";
                string user = request.User ?? "Anonymous";

                var randomizer = new Logic.PDFRandomizer();
                int sizePerPage, pages;

                // 2. Delegate to Single or Batch Print Endpoint Logic
                if (mode.Equals("batch", StringComparison.OrdinalIgnoreCase))
                {
                    // For batch mode, we'll generate random values for each file
                    var batchRequest = new Models.RequestModels.BatchRequestModel
                    {
                        NumberOfFiles = numberOfFiles,
                        PagesPerFile = pageMax, // We'll override this in the batch handler
                        SizePerPage = sizeMax, // We'll override this in the batch handler
                        ByteUnit = byteUnit,
                        MetricUnit = metricUnit,
                        User = user,
                        Host = request.Host
                    };

                    // Create a custom batch handler that generates new random values for each file
                    string processId = Guid.NewGuid().ToString();
                    var startResponse = new BatchGenerateStartResponse
                    {
                        Status = true,
                        ProcessId = processId,
                        Data = new BatchGenerateStartData { NumberOfFiles = numberOfFiles, PagesPerFile = pageMax },
                        Message = "Batch print job accepted. Processing in background."
                    };

                    // Notify client
                    await _messagingService.StartProcess(processId, "Batch PDF Print Job", user, new Dictionary<string, object>
                    {
                        { "action", "GenerateBatchAndPrint" },
                        { "numberOfFiles", numberOfFiles },
                        { "sizeMin", sizeMin },
                        { "sizeMax", sizeMax },
                        { "pageMin", pageMin },
                        { "pageMax", pageMax },
                        { "byteUnit", byteUnit },
                        { "lprHost", request.Host },
                        { "lprQueue", "myque" }
                    });

                    // Run batch in background
                    _ = Task.Run(async () =>
                    {
                        string resultMessage = "Batch print job failed.";
                        bool overallSuccess = false;
                        var resultDetails = new Dictionary<string, object>();
                        int successCount = 0;

                        try
                        {
                            await _messagingService.UpdateProcessProgress(processId, 10, "Initializing batch print job...");

                            for (int i = 0; i < numberOfFiles; i++)
                            {
                                try
                                {
                                    // Generate new random values for each file
                                    (sizePerPage, pages) = randomizer.GenerateRandomValues(sizeMin, sizeMax, pageMin, pageMax);

                                    // Create a single request with the new random values
                                    var singleRequest = new Models.RequestModels.RequestModel
                                    {
                                        Pages = pages,
                                        SizePerPage = sizePerPage,
                                        ByteUnit = byteUnit,
                                        MetricUnit = metricUnit,
                                        Format = "A4",
                                        User = user,
                                        Host = request.Host
                                    };

                                    // Process this single file
                                    var (printSuccess, message, fileName) = await _logicApiController.HandlePrintRequestAsync(
                                        pages, sizePerPage, byteUnit, request.Host, "myque", 515);

                                    if (printSuccess)
                                    {
                                        successCount++;
                                    }

                                    // Update progress
                                    int progress = (int)(((i + 1) * 100) / numberOfFiles);
                                    await _messagingService.UpdateProcessProgress(
                                        processId,
                                        progress,
                                        $"Processed {i + 1} of {numberOfFiles} files. Last file: {pages} pages, {sizePerPage} {byteUnit} per page."
                                    );
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error processing file {i + 1}: {ex.Message}");
                                }
                            }

                            overallSuccess = successCount == numberOfFiles;
                            resultMessage = overallSuccess
                                ? "All files processed successfully."
                                : $"Processed {successCount} of {numberOfFiles} files successfully.";

                            resultDetails.Add("totalFiles", numberOfFiles);
                            resultDetails.Add("successCount", successCount);
                            resultDetails.Add("failureCount", numberOfFiles - successCount);

                            if (overallSuccess)
                            {
                                await _messagingService.UpdateProcessProgress(processId, 100, "Batch print job completed successfully.");
                                await _messagingService.CompleteProcess(processId, resultMessage, resultDetails);
                            }
                            else
                            {
                                await _messagingService.UpdateProcessProgress(processId, 100, "Batch print job finished with errors.");
                                await _messagingService.FailProcess(processId, resultMessage, resultDetails);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Background Batch Print Task Error (Process ID: {processId}): {ex.ToString()}");
                            resultMessage = $"An internal error occurred during batch print: {ex.Message}";
                            resultDetails.Add("error", ex.Message);
                            await _messagingService.FailProcess(processId, resultMessage, resultDetails);
                        }
                    });

                    return Ok(startResponse);
                }
                else // Single mode
                {
                    try { (sizePerPage, pages) = randomizer.GenerateRandomValues(sizeMin, sizeMax, pageMin, pageMax); }
                    catch (ArgumentException ex) { return BadRequest($"Invalid random generation range: {ex.Message}"); }

                    var singleRequest = new Models.RequestModels.RequestModel
                    {
                        Pages = pages,
                        SizePerPage = sizePerPage,
                        ByteUnit = byteUnit,
                        MetricUnit = metricUnit,
                        Format = "A4",
                        User = user,
                        Host = request.Host
                    };
                    // Call the single print method directly
                    return await GenerateAndPrintAsync(singleRequest);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Failed to start random print job",
                    Message = ex.Message
                });
            }
        }

        [Route("GeneratePerformanceRun")]
        [HttpPost]
        public async Task<IActionResult> GeneratePerformanceRunAsync([FromBody] PerformanceRunRequestModel request)
        {
            try
            {
                // Validate input
                if (request.AmountOfTime <= 0)
                {
                    return BadRequest("AmountOfTime must be greater than zero.");
                }

                // Convert minutes to milliseconds
                int amountOfTimeMilliseconds = request.AmountOfTime * 60 * 1000;

                // Generate a process ID for tracking
                string processId = Guid.NewGuid().ToString();

                // Prepare initial response
                var startResponse = new GenerateStartResponse
                {
                    Status = true,
                    ProcessId = processId,
                    Message = $"Performance test started. Will run for {request.AmountOfTime} minutes."
                };

                // Notify client via SignalR that process started
                await _messagingService.StartProcess(processId, "Performance Test", request.User ?? "Anonymous", new Dictionary<string, object>
                {
                    { "action", "GeneratePerformanceRun" },
                    { "durationMinutes", request.AmountOfTime },
                    { "pagesPerFile", request.PagesPerFile },
                    { "sizePerPage", request.SizePerPage }
                });

                // Run the performance test in the background
                _ = Task.Run(async () =>
                {
                    int taskCount = 0;
                    int errorCount = 0;
                    var stopwatch = Stopwatch.StartNew();
                    bool cancelled = false;

                    try
                    {
                        using var cts = new CancellationTokenSource();
                        cts.CancelAfter(amountOfTimeMilliseconds);

                        while (stopwatch.ElapsedMilliseconds < amountOfTimeMilliseconds)
                        {
                            if (cts.Token.IsCancellationRequested)
                            {
                                cancelled = true;
                                break;
                            }

                            try
                            {
                                // Create a single request
                                var singleRequest = new Models.RequestModels.RequestModel
                                {
                                    Pages = request.PagesPerFile,
                                    SizePerPage = request.SizePerPage,
                                    ByteUnit = request.ByteUnit,
                                    MetricUnit = request.MetricUnit,
                                    User = request.User,
                                    Host = request.Host
                                };

                                // Call the logic controller directly instead of going through GenerateAndPrintAsync
                                (bool printSuccess, string message, string? fileName) = await _logicApiController.HandlePrintRequestAsync(
                                    request.PagesPerFile ?? 1,
                                    request.SizePerPage ?? 1,
                                    request.ByteUnit,
                                    request.Host,
                                    "myque",
                                    515);

                                if (printSuccess)
                                {
                                    taskCount++;
                                }
                                else
                                {
                                    errorCount++;
                                }

                                // Update progress based on time elapsed
                                int progressPercent = (int)((stopwatch.ElapsedMilliseconds * 100) / amountOfTimeMilliseconds);
                                TimeSpan remainingTime = TimeSpan.FromMilliseconds(amountOfTimeMilliseconds - stopwatch.ElapsedMilliseconds);
                                string timeRemaining = FormatTimeSpan(remainingTime);
                                await _messagingService.UpdateProcessProgress(
                                    processId,
                                    progressPercent,
                                    $"Completed {taskCount} tasks ({errorCount} errors). {progressPercent}% of time elapsed. Time remaining: {timeRemaining}"
                                );
                            }
                            catch (Exception ex)
                            {
                                errorCount++;
                                Console.Error.WriteLine($"Error in HandlePrintRequestAsync: {ex.Message}");
                                await Task.Delay(1000, cts.Token); // Add delay after error
                            }
                        }

                        // Report final results
                        var resultDetails = new Dictionary<string, object>
                        {
                            { "totalTasks", taskCount },
                            { "errorCount", errorCount },
                            { "durationMs", stopwatch.ElapsedMilliseconds },
                            { "tasksPerMinute", taskCount / (stopwatch.ElapsedMilliseconds / 60000.0) }
                        };

                        if (cancelled)
                        {
                            await _messagingService.FailProcess(
                                processId,
                                $"Performance test was cancelled. Completed {taskCount} tasks with {errorCount} errors.",
                                resultDetails
                            );
                        }
                        else
                        {
                            await _messagingService.CompleteProcess(
                                processId,
                                $"Performance test completed. Ran {taskCount} tasks with {errorCount} errors in {request.AmountOfTime} minutes.",
                                resultDetails
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Performance Test Error: {ex}");
                        await _messagingService.FailProcess(
                            processId,
                            $"Performance test failed: {ex.Message}",
                            new Dictionary<string, object>
                            {
                                { "error", ex.Message },
                                { "tasksCompleted", taskCount },
                                { "errorCount", errorCount + 1 }
                            }
                        );
                    }
                });

                return Ok(startResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Failed to start performance test",
                    Message = ex.Message
                });
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else
            {
                return $"{timeSpan.Seconds}s";
            }
        }

        // --- Remove GenerateAndStreamPdf endpoint ---
        // [Route("GenerateAndStream")] ... removed ...

        // --- Keep Original Background Task Endpoints (Optional) ---
        // You might keep GenerateStart/GenerateBatchStart if you still
        // need the behavior of saving locally without printing.
        // If not, you can remove them. For now, I'll comment them out.

        /*
        [Route("GenerateStart")]
        [HttpPost]
        public async Task<IActionResult> StartGenerationAsync([FromBody] RequestModel request)
        {
             // ... Original implementation saving locally ...
        }

        [Route("GenerateBatchStart")]
        [HttpPost]
        public async Task<IActionResult> StartBatchGenerationAsync([FromBody] BatchRequestModel request)
        {
             // ... Original implementation saving locally ...
        }

        [Route("GenerateRandomStart")]
        [HttpPost]
        public async Task<IActionResult> StartRandomGenerationAsync([FromBody] RandomRequestModel request)
        {
             // ... Original implementation calling local save endpoints ...
        }
        */
    }
}
