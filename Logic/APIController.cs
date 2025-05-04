// Logic/APIController.cs
using System;
using System.Diagnostics;
using System.IO; // Add if needed for filenames
using System.Linq;
using System.Text;
using System.Threading.Tasks; // Add for async

namespace Logic
{
    public class APIController
    {
        // --- Method for Single PDF Print Job ---
        // --- Method for Single PDF Print Job ---
        // This method is now fully self-contained for a single print request.
        public async Task<(bool Success, string Message, string GeneratedFileName)> HandlePrintRequestAsync(
            int pages, int sizePerPage, string byteUnit,
            string lprHost, string lprQueue, int lprPort)
        {
            PDFGenerator pdfGenerator = null; // Declare here to access filename in catch blocks
            string generatedFileName = "Unnamed.pdf";

            try
            {
                // 1. Create, Configure, and Generate PDF Bytes using PDFGenerator instance
                int targetPageSizeBytes = ConvertToBytes(sizePerPage, byteUnit); // Use static helper

                pdfGenerator = new PDFGenerator(); // Create new instance FOR THIS REQUEST
                pdfGenerator.Configure(pages, targetPageSizeBytes); // Configure it
                generatedFileName = pdfGenerator.GeneratedFileName; // Get the filename FROM THE INSTANCE

                Console.WriteLine($"Generating PDF bytes for: {generatedFileName}");
                byte[] pdfBytes = pdfGenerator.GeneratePdfBytes(); // Generate bytes FROM THE INSTANCE

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    // Handle generation failure from GeneratePdfBytes (which returns null)
                    throw new Exception("PDF generation resulted in null or empty data.");
                }
                Console.WriteLine($"PDF bytes generated successfully ({pdfBytes.Length} bytes).");

                // 2. Send to LPR Server
                var lprClient = new LprClient(lprHost, lprQueue, lprPort);
                Console.WriteLine($"Attempting to send '{generatedFileName}' to LPR server {lprHost}:{lprPort} queue '{lprQueue}'...");

                // SendPrintJobAsync now throws exceptions on failure, so we rely on catch blocks
                await lprClient.SendPrintJobAsync(pdfBytes, generatedFileName);

                // If SendPrintJobAsync completes without throwing, it succeeded
                Console.WriteLine($"Successfully sent '{generatedFileName}' to LPR server.");
                return (true, $"Print job '{generatedFileName}' sent successfully.", generatedFileName);

            }
            catch (ArgumentException argEx) // Catch config/validation errors (e.g., from ConvertToBytes)
            {
                Console.WriteLine($"Configuration Error: {argEx.Message}");
                // Use generatedFileName if available (set after Configure), otherwise default
                return (false, $"Configuration Error: {argEx.Message}", pdfGenerator?.GeneratedFileName ?? generatedFileName);
            }
            catch (InvalidOperationException opEx) // Catch errors from PDFGenerator (e.g., not configured before GenerateBytes)
            {
                Console.WriteLine($"PDF Generation Error: {opEx.Message}");
                return (false, $"PDF Generation Error: {opEx.Message}", pdfGenerator?.GeneratedFileName ?? generatedFileName);
            }
            catch (LprCommunicationException lprEx) // Catch specific LPR errors
            {
                Console.WriteLine($"LPR Error sending '{generatedFileName}': {lprEx.Message}");
                return (false, $"LPR Error: {lprEx.Message}", generatedFileName); // Filename should be set by this point
            }
            catch (Exception ex) // Catch general generation or unexpected LPR errors
            {
                Console.WriteLine($"Error during print request for '{generatedFileName}': {ex.ToString()}");
                return (false, $"Failed to process print job '{generatedFileName}': {ex.Message}", generatedFileName);
            }
        }

        // --- Method for Batch PDF Print Job ---
        // This method is now fully self-contained for a batch print request.
        public async Task<(bool Success, int SuccessCount, int TotalFiles, string Message)> HandleBatchPrintRequestAsync(
            int numberOfFiles, int pagesPerFile, int sizePerPage, string byteUnit,
            string lprHost, string lprQueue, int lprPort)
        {
            if (numberOfFiles <= 0 || pagesPerFile <= 0 || sizePerPage <= 0)
            {
                // Return failure immediately for invalid batch parameters
                return (false, 0, numberOfFiles, "Error: Number of files, pages per file, and size per page must all be greater than zero.");
            }

            int successCount = 0;
            var errorMessages = new StringBuilder();

            Console.WriteLine($"\nStarting batch print job:");
            Console.WriteLine($" -> Files: {numberOfFiles}");
            Console.WriteLine($" -> LPR Target: {lprHost}:{lprPort} Queue: {lprQueue}\n");

            int targetPageSizeBytes = ConvertToBytes(sizePerPage, byteUnit); // Use static helper
            var lprClient = new LprClient(lprHost, lprQueue, lprPort); // Create LPR client once for the batch

            for (int i = 0; i < numberOfFiles; i++)
            {
                PDFGenerator pdfGenerator = null; // Instance per file
                string currentFileName = $"BatchFile_{i + 1}_Generating.pdf"; // Default name during processing
                Console.WriteLine($"--- Processing file {i + 1}/{numberOfFiles} ---");

                try
                {
                    // 1. Create, Configure, Generate PDF FOR THIS FILE
                    pdfGenerator = new PDFGenerator(); // New instance for this file
                    pdfGenerator.Configure(pagesPerFile, targetPageSizeBytes);
                    currentFileName = pdfGenerator.GeneratedFileName; // Get actual generated name
                    byte[] pdfBytes = pdfGenerator.GeneratePdfBytes(); // Generate bytes

                    if (pdfBytes == null || pdfBytes.Length == 0)
                    {
                        throw new Exception("PDF generation resulted in null or empty data.");
                    }
                    Console.WriteLine($"Generated '{currentFileName}' ({pdfBytes.Length} bytes).");

                    // 2. Send PDF FOR THIS FILE
                    Console.WriteLine($"Sending '{currentFileName}' to LPR...");
                    // SendPrintJobAsync throws on error
                    await lprClient.SendPrintJobAsync(pdfBytes, currentFileName);

                    // If we reach here, it was successful for this file
                    successCount++;
                    Console.WriteLine($"Successfully sent '{currentFileName}'.");

                    // Optional: Add a small delay between jobs if the printer needs it
                    // await Task.Delay(100);
                }
                catch (Exception ex) // Catch ANY exception during this file's processing
                {
                    // Log the specific error for this file
                    string errorMsg = $"Failed file {i + 1} ('{currentFileName}'): {ex.Message}";
                    Console.WriteLine($"ERROR: {errorMsg}");
                    // Append to the summary of errors
                    errorMessages.AppendLine(errorMsg);
                    // NOTE: The loop continues to the next file unless you uncomment 'break;'
                    // break; // Uncomment to stop batch on first error
                }
            } // End loop

            Console.WriteLine($"\nBatch print job finished. Successful: {successCount}/{numberOfFiles}.");

            // Determine overall success based on counts
            bool overallSuccess = successCount == numberOfFiles;
            string finalMessage = overallSuccess
                ? "Batch print job completed successfully."
                : $"Batch print job completed with {numberOfFiles - successCount} failure(s). See details."; // Keep message concise

            // Include collected errors in the message if any occurred
            string detailedMessage = errorMessages.Length > 0
               ? $"{finalMessage} Errors: {errorMessages.ToString()}"
               : finalMessage;

            return (overallSuccess, successCount, numberOfFiles, detailedMessage);
        }


        // --- Keep Utility Methods ---
        private static int ConvertToBytes(int size, string byteUnit)
        {
            byteUnit = byteUnit.ToUpper();
            return byteUnit switch
            {
                "MB" => size * 1024 * 1024,
                "GB" => size * 1024 * 1024 * 1024,
                _ => throw new ArgumentException("Invalid byte unit. Only MB and GB are supported.")
            };
        }
        public static double ConvertBytesToUnit(long bytes, string byteUnit)
        {
            return byteUnit.ToUpper() switch
            {
                "MB" => bytes / (1024.0 * 1024.0),
                "GB" => bytes / (1024.0 * 1024.0 * 1024.0),
                "KB" => bytes / 1024.0,
                _ => bytes
            };
        }

        public static long EstimateSize(int pages, int sizePerPage, string byteUnit)
        {
            try
            {
                int targetPageSizeBytes = ConvertToBytes(sizePerPage, byteUnit); // Use static helper
                if (pages <= 0) return 0; // Handle invalid page count

                // Create a temporary generator instance JUST for estimation
                var tempPdfGenerator = new PDFGenerator();
                // Configure calculates the estimate and stores it in the instance
                tempPdfGenerator.Configure(pages, targetPageSizeBytes);
                // Return the estimate from the temporary instance
                return tempPdfGenerator.EstimatedTotalSizeBytes;
            }
            catch (ArgumentException argEx) // Catch errors from ConvertToBytes or Configure
            {
                Console.WriteLine($"Error during size estimation (Input: {pages}p, {sizePerPage}{byteUnit}): {argEx.Message}");
                return -1; // Indicate estimation failure
            }
            catch (Exception ex) // Catch unexpected errors
            {
                Console.WriteLine($"Unexpected error during size estimation: {ex.ToString()}");
                return -1; // Indicate estimation failure
            }
        }
    }
}