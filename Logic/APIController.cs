using System;

namespace Logic
{
    public class APIController
    {
        private PDFGenerator _pdfGenerator;

        public APIController()
        {
            _pdfGenerator = new PDFGenerator();
        }

        public long HandleRequest(int pages, int size, string byteUnit)
        {
            try
            {
                int sizeInBytes = ConvertToBytes(size, byteUnit);
                _pdfGenerator.Configure(pages, sizeInBytes);
                long actualSize = _pdfGenerator.GenerateAndSavePDF();
                return actualSize;
            }
            catch (ArgumentOutOfRangeException argEx)
            {
                Console.WriteLine($"Configuration Error: {argEx.ParamName} - {argEx.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during PDF generation request: {ex.Message}");
                return -1;
            }
        }

        public long[] HandleBatchRequest(int numberOfFiles, int pagesPerFile, int size, string byteUnit = "MB")
        {
            if (numberOfFiles <= 0 || pagesPerFile <= 0 || size <= 0)
            {
                Console.WriteLine("Error: All input values must be greater than zero.");
                return Array.Empty<long>();
            }

            int sizeInBytes = ConvertToBytes(size, byteUnit);
 
            // Calculate target size per page
            int targetPageSizeBytes = sizeInBytes / pagesPerFile;
 
            Console.WriteLine($"\nStarting batch generation:");
            Console.WriteLine($" -> Files: {numberOfFiles}");
            Console.WriteLine($" -> Pages per file: {pagesPerFile}");
            Console.WriteLine($" -> Target size per file: {sizeInBytes} bytes");
            Console.WriteLine($" -> Target size per page: {targetPageSizeBytes} bytes\n");
 
            long[] actualSizes = new long[numberOfFiles];
 
            for (int i = 0; i < numberOfFiles; i++)
            {
                Console.WriteLine($"\n--- Generating PDF {i + 1}/{numberOfFiles} ---");
                _pdfGenerator = new PDFGenerator(); // New instance per file
                _pdfGenerator.Configure(pagesPerFile, targetPageSizeBytes);
                actualSizes[i] = _pdfGenerator.GenerateAndSavePDF();
            }
 
            return actualSizes;
        }

        private int ConvertToBytes(int size, string byteUnit)
        {
            byteUnit = byteUnit.ToUpper();
            return byteUnit switch
            {
                "MB" => size * 1024 * 1024,
                "GB" => size * 1024 * 1024 * 1024,
                _ => throw new ArgumentException("Invalid byte unit. Only MB and GB are supported.")
            };
        }

        public long EstimateSize(int pages, int size, string byteUnit)
                {
                    try
                    {
                        int sizeInBytes = ConvertToBytes(size, byteUnit);
                        int targetPageSizeBytes = (pages > 0) ? sizeInBytes / pages : sizeInBytes;
                        if (targetPageSizeBytes <= 0) targetPageSizeBytes = 1;

                        var tempPdfGenerator = new PDFGenerator();
                        tempPdfGenerator.Configure(pages, targetPageSizeBytes); // Configure calculates the estimate
                        return tempPdfGenerator.EstimatedTotalSizeBytes;
                    }
                    catch(Exception ex)
                    {
                         Console.WriteLine($"Error during size estimation: {ex.Message}");
                         return -1;
                    }
                }
    }
}