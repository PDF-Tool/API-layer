using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace Logic
{
    public class PDFGenerator
    {
        private List<PDFPage> _pdfPages;
        private int _pageCount;
        private int _targetSizePerPage; // User's requested target per page
        private long _totalActualContentBytes; // Actual sizes of generated images
        private long _estimatedTotalSizeBytes; // Final estimate based on actual content


        private const long BytesInKB = 1024;
        private const long BytesInMB = 1048576;
        private const long BytesInGB = 1073741824;

        public void Configure(int pages, int targetSizePerPageBytes)
        {
            if (pages <= 0) throw new ArgumentOutOfRangeException(nameof(pages), "Number of pages must be positive.");
            if (targetSizePerPageBytes <= 0) throw new ArgumentOutOfRangeException(nameof(targetSizePerPageBytes), "Target size per page must be positive.");

            _pageCount = pages;
            _targetSizePerPage = targetSizePerPageBytes;

            Console.WriteLine($"Configuring for {_pageCount} pages, Target content size per page: {_targetSizePerPage} bytes.\n");
            Console.WriteLine("Generating content for all pages...");

            var stopwatch = Stopwatch.StartNew();
            _pdfPages = new List<PDFPage>(_pageCount);
            _totalActualContentBytes = 0;

            for (int i = 0; i < _pageCount; i++)
            {
                var page = new PDFPage(_targetSizePerPage);
                _pdfPages.Add(page);
                _totalActualContentBytes += page.GetActualContentImageDataSize();
            }
            stopwatch.Stop();
            Console.WriteLine($"Content generation complete ({stopwatch.ElapsedMilliseconds} ms).");
            Console.WriteLine($"Total actual generated content data size: {_totalActualContentBytes / 1024.0:F2} KB ({_totalActualContentBytes} bytes)");

            // --- Calculate the final estimate USING the actual content size ---
            _estimatedTotalSizeBytes = PDFContent.CalculateExpectedTotalSize(_pageCount, _totalActualContentBytes);
            // -----------------------------------------------------------------

            Console.WriteLine($"Estimated total PDF size: {_estimatedTotalSizeBytes / 1024.0:F2} KB ({_estimatedTotalSizeBytes} bytes)\n");
            //Console.WriteLine($" (Based on {PDFContent.ESTIMATED_PDF_FIXED_OVERHEAD_BYTES} fixed + {_pageCount} * {PDFContent.ESTIMATED_PDF_PAGE_OVERHEAD_BYTES} page + {_totalActualContentBytes} * {PDFContent.CONTENT_EMBEDDING_FACTOR:F3} content)");
        }


        public long GenerateAndSavePDF()
        {
            if (_pdfPages == null || !_pdfPages.Any())
            {
                Console.WriteLine("Error: PDF Generator not configured or has no pages.");
                return -1;
            }

            string outputDirectory = Path.Combine(Environment.CurrentDirectory, "GeneratedPDFs");
            Directory.CreateDirectory(outputDirectory);

            string uuid = Guid.NewGuid().ToString().Substring(0, 8);
            string timestamp = DateTime.Now.ToString("mm-ss_ffff");
            string outputFileName = $"PDF_{uuid}_{_pageCount}pg_{_targetSizePerPage}bpp_{timestamp}.pdf";
            string outputPath = Path.Combine(outputDirectory, outputFileName);

            Console.WriteLine($"Attempting to save PDF to: {outputPath}");
            var stopwatch = Stopwatch.StartNew();
            long actualFileSize = -1;

            try
            {
                using (var document = new PdfDocument())
                {
                    
                    document.Info.Title = $"Generated PDF ({_pageCount} pages, ~{_targetSizePerPage} bytes/page content target)";
                    document.Info.Author = "PDFGenerator";
                    document.Options.CompressContentStreams = true;
                    document.Options.NoCompression = false;        


                    for (int i = 0; i < _pdfPages.Count; i++)
                    {
                        PdfPage pdfSharpPage = document.AddPage();
                        pdfSharpPage.Size = PdfSharpCore.PageSize.A4;

                        
                        using (XGraphics gfx = XGraphics.FromPdfPage(pdfSharpPage))
                        {
                            byte[] imageData = _pdfPages[i].GetContentImageData();

                            if (imageData != null && imageData.Length > 0)
                            {
                                try
                                {
                                    using (var ms = new MemoryStream(imageData))
                                    {
                                        XImage image = XImage.FromStream(() => ms);

                                        // Draw image scaled to fit page width (adjust drawing logic if needed)
                                        double scaleRatio = pdfSharpPage.Width.Point / image.PointWidth;
                                        double drawWidth = pdfSharpPage.Width.Point;
                                        double drawHeight = image.PointHeight * scaleRatio;
                                        double yPos = (pdfSharpPage.Height.Point - drawHeight) / 2.0; // Center vertically
                                        if (yPos < 0) yPos = 0; // Prevent drawing off-page if image is very tall

                                        gfx.DrawImage(image, 0, yPos, drawWidth, drawHeight);
                                    }
                                }
                                catch (Exception imgEx)
                                {
                                    Console.WriteLine($"Error drawing image for page {i + 1}: {imgEx.Message}. Drawing placeholder.");
                                    DrawPlaceholderText(gfx, pdfSharpPage, "Error drawing page content");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Warning: No image data for page {i + 1}. Drawing placeholder.");
                                DrawPlaceholderText(gfx, pdfSharpPage, "No content generated for this page");
                            }
                        }
                    } // End page loop

                    document.Save(outputPath);
                } // PdfDocument disposed

                stopwatch.Stop();
                actualFileSize = new FileInfo(outputPath).Length;
                Console.WriteLine($"PDF saved successfully ({stopwatch.ElapsedMilliseconds} ms).\n");

                //Final comparison
                double difference = Math.Abs(_estimatedTotalSizeBytes - actualFileSize);
                double percentageDiff = (_estimatedTotalSizeBytes == 0) ? 100.0 : (difference / _estimatedTotalSizeBytes) * 100.0;

                //Console.WriteLine($" -> Final Size Comparison:");
                Console.WriteLine($"    Estimated: {_estimatedTotalSizeBytes / BytesInMB:F2} MB ({_estimatedTotalSizeBytes} bytes)");
                Console.WriteLine($"    Actual:    {actualFileSize / BytesInMB:F2} MB ({actualFileSize} bytes)");
                //Console.WriteLine($"    Difference: {difference / BytesInMB:F2} MB ({difference} bytes)");
                Console.WriteLine($"    Accuracy:   {percentageDiff:F2}% deviation");

                if (percentageDiff <= 5.0)
                {
                    Console.WriteLine("    Result: Achieved target accuracy (< 5%).");
                }
                else if (percentageDiff <= 10.0)
                {
                    Console.WriteLine("    Result: Within acceptable margin (5-10%).");
                }
                else
                {
                    Console.WriteLine("    Result: Outside target accuracy (> 10%). <-- TUNING REQUIRED.");
                }
            }

            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"An unexpected error occurred during PDF save: {ex.ToString()}");
                actualFileSize = -1;
            }
            return actualFileSize;
        }

        private void DrawPlaceholderText(XGraphics gfx, PdfPage page, string text)
        {
            XFont font = new XFont("Arial", 12, XFontStyle.Bold); // Use a common font
            XSolidBrush brush = XBrushes.DarkRed;
            XRect rect = new XRect(40, 40, page.Width.Point - 80, page.Height.Point - 80); // Center area
            gfx.DrawString(text, font, brush, rect, XStringFormats.Center);
        }

        public long EstimatedTotalSizeBytes => _estimatedTotalSizeBytes;
    }
}