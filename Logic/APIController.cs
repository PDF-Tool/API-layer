using System;

public class APIController
{
    private PDFGenerator _pdfGenerator;

    public APIController()
    {
        _pdfGenerator = new PDFGenerator();
    }

    public long HandleRequest(int pages, int pageSize)
    {
        try
        {
            _pdfGenerator.Configure(pages, pageSize);
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

    public long[] HandleBatchRequest(int numberOfFiles, int pagesPerFile, int targetFileSizeBytes)
    {
        if (numberOfFiles <= 0 || pagesPerFile <= 0 || targetFileSizeBytes <= 0)
        {
            Console.WriteLine("Error: All input values must be greater than zero.");
            return Array.Empty<long>();
        }
 
        // Calculate target size per page
        int targetPageSizeBytes = targetFileSizeBytes / pagesPerFile;
 
        Console.WriteLine($"\nStarting batch generation:");
        Console.WriteLine($" -> Files: {numberOfFiles}");
        Console.WriteLine($" -> Pages per file: {pagesPerFile}");
        Console.WriteLine($" -> Target size per file: {targetFileSizeBytes} bytes");
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

    public long GetEstimatedSize()
    {
        return _pdfGenerator?.EstimatedTotalSizeBytes ?? 0;
    }
}