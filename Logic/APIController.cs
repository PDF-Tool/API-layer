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
            PDFGenerator pdfGenerator = null;
            string generatedFileName = "Unnamed.pdf";

            try
            {
                // 1. Create and Generate PDF using PDFGenerator instance
                int targetPageSizeBytes = ConvertToBytes(sizePerPage, byteUnit);

                pdfGenerator = new PDFGenerator(pages, targetPageSizeBytes);
                generatedFileName = pdfGenerator.GeneratedFileName;

                Console.WriteLine($"Generating PDF for: {generatedFileName}");
                
                // Create a memory stream to hold the PDF data
                using (var memoryStream = new MemoryStream())
                {
                    // Generate PDF directly to the memory stream
                    await pdfGenerator.GenerateAndWriteStreamAsync(memoryStream);
                    byte[] pdfBytes = memoryStream.ToArray();

                    if (pdfBytes == null || pdfBytes.Length == 0)
                    {
                        throw new Exception("PDF generation resulted in null or empty data.");
                    }
                    Console.WriteLine($"PDF generated successfully ({pdfBytes.Length} bytes).");

                    // 2. Send to LPR Server
                    var lprClient = new LprClient(lprHost, lprQueue, lprPort);
                    Console.WriteLine($"Attempting to send '{generatedFileName}' to LPR server {lprHost}:{lprPort} queue '{lprQueue}'...");

                    await lprClient.SendPrintJobAsync(pdfBytes, generatedFileName);
                    Console.WriteLine($"Successfully sent '{generatedFileName}' to LPR server.");
                    return (true, $"Print job '{generatedFileName}' sent successfully.", generatedFileName);
                }
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine($"Configuration Error: {argEx.Message}");
                return (false, $"Configuration Error: {argEx.Message}", pdfGenerator?.GeneratedFileName ?? generatedFileName);
            }
            catch (InvalidOperationException opEx)
            {
                Console.WriteLine($"PDF Generation Error: {opEx.Message}");
                return (false, $"PDF Generation Error: {opEx.Message}", pdfGenerator?.GeneratedFileName ?? generatedFileName);
            }
            catch (LprCommunicationException lprEx)
            {
                Console.WriteLine($"LPR Error sending '{generatedFileName}': {lprEx.Message}");
                return (false, $"LPR Error: {lprEx.Message}", generatedFileName);
            }
            catch (Exception ex)
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

            int targetPageSizeBytes = ConvertToBytes(sizePerPage, byteUnit);
            var lprClient = new LprClient(lprHost, lprQueue, lprPort);

            for (int i = 0; i < numberOfFiles; i++)
            {
                PDFGenerator pdfGenerator = null;
                string currentFileName = $"BatchFile_{i + 1}_Generating.pdf";
                Console.WriteLine($"--- Processing file {i + 1}/{numberOfFiles} ---");

                try
                {
                    // 1. Create and Generate PDF
                    pdfGenerator = new PDFGenerator(pagesPerFile, targetPageSizeBytes);
                    currentFileName = pdfGenerator.GeneratedFileName;

                    using (var memoryStream = new MemoryStream())
                    {
                        await pdfGenerator.GenerateAndWriteStreamAsync(memoryStream);
                        byte[] pdfBytes = memoryStream.ToArray();

                        if (pdfBytes == null || pdfBytes.Length == 0)
                        {
                            throw new Exception("PDF generation resulted in null or empty data.");
                        }
                        Console.WriteLine($"Generated '{currentFileName}' ({pdfBytes.Length} bytes).");

                        // 2. Send PDF
                        Console.WriteLine($"Sending '{currentFileName}' to LPR...");
                        await lprClient.SendPrintJobAsync(pdfBytes, currentFileName);
                        successCount++;
                        Console.WriteLine($"Successfully sent '{currentFileName}'.");
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Failed file {i + 1} ('{currentFileName}'): {ex.Message}";
                    Console.WriteLine($"ERROR: {errorMsg}");
                    errorMessages.AppendLine(errorMsg);
                }
            }

            Console.WriteLine($"\nBatch print job finished. Successful: {successCount}/{numberOfFiles}.");

            bool overallSuccess = successCount == numberOfFiles;
            string finalMessage = overallSuccess
                ? "Batch print job completed successfully."
                : $"Batch print job completed with {numberOfFiles - successCount} failure(s). See details.";

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
                int targetPageSizeBytes = ConvertToBytes(sizePerPage, byteUnit);
                if (pages <= 0) return 0;

                // Create a temporary generator instance for estimation
                var tempPdfGenerator = new PDFGenerator(pages, targetPageSizeBytes);
                return tempPdfGenerator.EstimatedTotalSizeBytes;
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine($"Error during size estimation (Input: {pages}p, {sizePerPage}{byteUnit}): {argEx.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error during size estimation: {ex.ToString()}");
                return -1;
            }
        }
    }
}