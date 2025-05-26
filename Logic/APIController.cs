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
                var memoryStream = new MemoryStream();
                try
                {
                    // Generate PDF directly to the memory stream
                    await pdfGenerator.GenerateAndWriteStreamAsync(memoryStream);
                    
                    // Get the bytes after generation but before disposal
                    byte[] pdfBytes = memoryStream.ToArray();

                    if (pdfBytes == null || pdfBytes.Length == 0)
                    {
                        throw new Exception("PDF generation resulted in null or empty data.");
                    }
                    Console.WriteLine($"PDF generated successfully ({pdfBytes.Length} bytes).");

                    // Send to printer using LprClient
                    var lprClient = new LprClient(lprHost, lprQueue, lprPort);
                    bool printSuccess = await lprClient.SendPrintJobAsync(pdfBytes, generatedFileName);

                    if (!printSuccess)
                    {
                        throw new Exception("Failed to send print job to printer.");
                    }

                    Console.WriteLine($"PDF sent to printer successfully.");
                }
                finally
                {
                    memoryStream.Dispose();
                }

                return (true, "PDF generated and sent to printer successfully", generatedFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandlePrintRequestAsync: {ex.Message}");
                return (false, $"Error: {ex.Message}", generatedFileName);
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

                    var memoryStream = new MemoryStream();
                    try
                    {
                        await pdfGenerator.GenerateAndWriteStreamAsync(memoryStream);
                        byte[] pdfBytes = memoryStream.ToArray();

                        if (pdfBytes == null || pdfBytes.Length == 0)
                        {
                            throw new Exception("PDF generation resulted in null or empty data.");
                        }
                        Console.WriteLine($"PDF generated successfully ({pdfBytes.Length} bytes).");

                        // Send to printer using LprClient
                        bool printSuccess = await lprClient.SendPrintJobAsync(pdfBytes, currentFileName);
                        if (!printSuccess)
                        {
                            throw new Exception("Failed to send print job to printer.");
                        }
                        Console.WriteLine($"PDF sent to printer successfully.");

                        successCount++;
                    }
                    finally
                    {
                        memoryStream.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Error processing file {i + 1}: {ex.Message}";
                    Console.WriteLine(errorMessage);
                    errorMessages.AppendLine(errorMessage);
                }
            }

            string finalMessage = successCount == numberOfFiles
                ? "All files processed successfully."
                : $"Processed {successCount} of {numberOfFiles} files. Errors: {errorMessages}";

            return (successCount == numberOfFiles, successCount, numberOfFiles, finalMessage);
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
    }
}