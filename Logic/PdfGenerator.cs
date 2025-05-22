using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Logic
{
    public class PDFGenerator
    {
        private readonly int _pageCount;
        private readonly int _targetSizePerPage;
        private readonly string _generatedFileName;
        private long _totalActualContentBytes;
        private long _estimatedTotalSizeBytes;

        private const long BytesInKB = 1024;
        private const long BytesInMB = 1048576;

        public string GeneratedFileName => _generatedFileName;
        public long EstimatedTotalSizeBytes => _estimatedTotalSizeBytes;

        public PDFGenerator(int pages, int targetSizePerPageBytes)
        {
            if (pages <= 0) throw new ArgumentOutOfRangeException(nameof(pages), "Number of pages must be positive.");
            if (targetSizePerPageBytes <= 0) throw new ArgumentOutOfRangeException(nameof(targetSizePerPageBytes), "Target size per page must be positive.");

            _pageCount = pages;
            _targetSizePerPage = targetSizePerPageBytes;
            _generatedFileName = BuildOutputFileName();
        }

        public async Task GenerateAndWriteStreamAsync(Stream outputStream)
        {
            if (outputStream == null || !outputStream.CanWrite)
            {
                throw new ArgumentException("Output stream must be valid and writable.", nameof(outputStream));
            }

            Console.WriteLine($"Generating PDF stream for: {_generatedFileName}");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var document = new PdfDocument();
                document.Info.Title = $"Generated PDF ({_pageCount} pages, ~{_targetSizePerPage} bytes/page content target)";
                document.Info.Author = "PDFGenerator";
                document.Info.Subject = _generatedFileName;
                document.Options.CompressContentStreams = true;
                document.Options.NoCompression = false;

                _totalActualContentBytes = 0;

                // Generate and write pages one at a time
                for (int i = 0; i < _pageCount; i++)
                {
                    var page = new PDFPage(_targetSizePerPage);
                    _totalActualContentBytes += page.GetActualContentSize();

                    var pdfSharpPage = document.AddPage();
                    pdfSharpPage.Size = PdfSharpCore.PageSize.A4;

                    using (var gfx = XGraphics.FromPdfPage(pdfSharpPage))
                    {
                        byte[] bitmapData = page.GetBitmapData();
                        
                        // Create XImage from raw bitmap data
                        using (var ms = new MemoryStream(bitmapData))
                        {
                            // Create XImage with raw bitmap data
                            var image = XImage.FromStream(() => ms);

                            // Calculate scaling to fit page width
                            double scaleRatio = pdfSharpPage.Width.Point / page.Width;
                            double drawWidth = pdfSharpPage.Width.Point;
                            double drawHeight = page.Height * scaleRatio;
                            double yPos = (pdfSharpPage.Height.Point - drawHeight) / 2.0;
                            if (yPos < 0) yPos = 0;

                            gfx.DrawImage(image, 0, yPos, drawWidth, drawHeight);
                        }
                    }

                    // Write the current state to the output stream
                    if (i == 0)
                    {
                        // First page - write document header
                        document.Save(outputStream, true);
                    }
                    else
                    {
                        // Subsequent pages - append to stream
                        document.Save(outputStream, true);
                    }

                    // Allow other tasks to run
                    await Task.Yield();
                }

                stopwatch.Stop();
                _estimatedTotalSizeBytes = PDFContent.CalculateExpectedTotalSize(_pageCount, _totalActualContentBytes);

                Console.WriteLine($"PDF generation complete ({stopwatch.ElapsedMilliseconds} ms)");
                Console.WriteLine($"Total content size: {_totalActualContentBytes / BytesInKB:F2} KB");
                Console.WriteLine($"Estimated total PDF size: {_estimatedTotalSizeBytes / BytesInKB:F2} KB");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"Error during PDF generation: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GenerateAndSaveLocally()
        {
            string outputDirectory = Path.Combine(Environment.CurrentDirectory, "GeneratedPDFs");
            Directory.CreateDirectory(outputDirectory);
            string outputPath = Path.Combine(outputDirectory, _generatedFileName);

            Console.WriteLine($"Saving PDF locally to: {outputPath}");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await GenerateAndWriteStreamAsync(fileStream);
                }

                stopwatch.Stop();
                var fileInfo = new FileInfo(outputPath);
                
                Console.WriteLine($"PDF saved successfully ({stopwatch.ElapsedMilliseconds} ms)");
                Console.WriteLine($"File size: {fileInfo.Length / BytesInKB:F2} KB");
                Console.WriteLine($"Location: {outputPath}");

                return outputPath;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"Error saving PDF locally: {ex.Message}");
                throw;
            }
        }

        private string BuildOutputFileName()
        {
            return $"GeneratedPDF_{DateTime.Now:yyyyMMdd_HHmmss}_{_pageCount}pages_{_targetSizePerPage}bytes.pdf";
        }
    }
}