using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks; // Keep this for GenerateAndWriteStreamAsync

namespace Logic
{
    public class PDFGenerator
    {
        private List<PDFPage> _pdfPages;
        private int _pageCount;
        private int _targetSizePerPage;
        private long _totalActualContentBytes;
        private long _estimatedTotalSizeBytes;

        private const long BytesInKB = 1024;
        private const long BytesInMB = 1048576;


        public string GeneratedFileName { get; private set; }
        public long EstimatedTotalSizeBytes => _estimatedTotalSizeBytes;

        public void Configure(int pages, int targetSizePerPageBytes)
        {
            if (pages <= 0) throw new ArgumentOutOfRangeException(nameof(pages), "Number of pages must be positive.");
            if (targetSizePerPageBytes <= 0) throw new ArgumentOutOfRangeException(nameof(targetSizePerPageBytes), "Target size per page must be positive.");

            _pageCount = pages;
            _targetSizePerPage = targetSizePerPageBytes;
            GeneratedFileName = BuildOutputFileName(); // Set filename here

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
            Console.WriteLine($"Total actual generated content data size: {_totalActualContentBytes / BytesInKB:F2} KB ({_totalActualContentBytes} bytes)");

            _estimatedTotalSizeBytes = PDFContent.CalculateExpectedTotalSize(_pageCount, _totalActualContentBytes);

            Console.WriteLine($"Estimated total PDF size: {_estimatedTotalSizeBytes / BytesInKB:F2} KB ({_estimatedTotalSizeBytes} bytes)\n");
        }

        // Method to save the generated PDF to a local file
        public long GenerateAndSavePDF()
        {
            if (_pdfPages == null || !_pdfPages.Any())
            {
                Console.WriteLine("Error: PDF Generator not configured or has no pages.");
                return -1;
            }

            string outputDirectory = Path.Combine(Environment.CurrentDirectory, "GeneratedPDFs");
            Directory.CreateDirectory(outputDirectory);
            string outputPath = Path.Combine(outputDirectory, GeneratedFileName); // Use pre-generated name

            Console.WriteLine($"Attempting to save PDF to: {outputPath}");
            var stopwatch = Stopwatch.StartNew();
            long actualFileSize = -1;

            try
            {
                // Use a FileStream and call the internal writing method
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    // WriteToStreamInternal will handle the PDF document creation and saving
                    // For FileStream, leaveOpen: false is okay as we own the stream here.
                    WriteToStreamInternal(fileStream, leaveOpen: false);
                }

                stopwatch.Stop();
                actualFileSize = new FileInfo(outputPath).Length;
                Console.WriteLine($"PDF saved successfully ({stopwatch.ElapsedMilliseconds} ms).\n");

                // Comparison logic remains the same
                double difference = Math.Abs(_estimatedTotalSizeBytes - actualFileSize);
                double percentageDiff = (_estimatedTotalSizeBytes == 0) ? 100.0 : (difference / _estimatedTotalSizeBytes) * 100.0;

                Console.WriteLine($"    Estimated: {_estimatedTotalSizeBytes / BytesInMB:F2} MB ({_estimatedTotalSizeBytes} bytes)");
                Console.WriteLine($"    Actual:    {actualFileSize / BytesInMB:F2} MB ({actualFileSize} bytes)");
                Console.WriteLine($"    Accuracy:   {percentageDiff:F2}% deviation");
                // ... (Accuracy reporting logic) ...

            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"An unexpected error occurred during PDF save: {ex.ToString()}");
                actualFileSize = -1; // Indicate failure
            }
            return actualFileSize;
        }

        // Method to generate the PDF and return its content as a byte array
        public byte[] GeneratePdfBytes()
        {
            if (_pdfPages == null || !_pdfPages.Any())
            {
                Console.WriteLine("Error: PDF Generator not configured or has no pages.");
                return null; // Return null to indicate failure
            }

            Console.WriteLine($"Generating PDF bytes in memory for: {GeneratedFileName}");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Write to the MemoryStream. leaveOpen: false is fine here too,
                    // as we'll read from it before the using block disposes it.
                    WriteToStreamInternal(memoryStream, leaveOpen: false);

                    stopwatch.Stop();
                    byte[] pdfBytes = memoryStream.ToArray(); // Get bytes AFTER Save completes
                    Console.WriteLine($"PDF bytes generated successfully ({stopwatch.ElapsedMilliseconds} ms, {pdfBytes.Length / BytesInKB:F2} KB).");
                    return pdfBytes;
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"An unexpected error occurred during PDF byte generation: {ex.ToString()}");
                return null; // Return null on error
            }
        }


        // Method to generate the PDF and write it asynchronously to an output stream
        // Typically used for streaming responses in web APIs.
        public async Task GenerateAndWriteStreamAsync(Stream outputStream)
        {
            if (_pdfPages == null || !_pdfPages.Any())
            {
                Console.WriteLine("Error: PDF Generator not configured or has no pages. Cannot write to stream.");
                throw new InvalidOperationException("PDF Generator not configured or has no pages."); // Throw for caller
            }
            if (outputStream == null || !outputStream.CanWrite)
            {
                Console.WriteLine("Error: Output stream is null or not writable.");
                throw new ArgumentException("Output stream must be valid and writable.", nameof(outputStream));
            }


            Console.WriteLine($"Attempting to write PDF stream for: {GeneratedFileName}");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // IMPORTANT: For streams owned by the caller (like Response.Body),
                // set leaveOpen to true so PdfDocument.Save doesn't close it.
                // The WriteToStreamInternal method itself is synchronous, but we wrap
                // the call in Task.Run if it becomes CPU-bound or needs true async.
                // For now, direct call is okay as Save isn't truly async.
                WriteToStreamInternal(outputStream, leaveOpen: true);

                // Optional: Flush the output stream if needed (usually handled by caller/framework)
                // await outputStream.FlushAsync();

                stopwatch.Stop();
                Console.WriteLine($"PDF stream written successfully ({stopwatch.ElapsedMilliseconds} ms).");
                Console.WriteLine($"   Estimated size was: {_estimatedTotalSizeBytes / BytesInKB:F2} KB ({_estimatedTotalSizeBytes} bytes)");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"An unexpected error occurred during PDF stream writing: {ex.ToString()}");
                throw; // Rethrow the exception for the caller to handle
            }
            // Do NOT close outputStream here - the caller owns it.
        }

        // *** CORRECTED Internal Helper for Core PDF Writing Logic ***
        private void WriteToStreamInternal(Stream stream, bool leaveOpen)
        {
            // This method now takes the leaveOpen parameter

            if (_pdfPages == null || !_pdfPages.Any())
            {
                throw new InvalidOperationException("Attempted to write PDF, but generator is not configured or has no pages.");
            }

            using (var document = new PdfDocument()) // Create the document instance HERE
            {
                document.Info.Title = $"Generated PDF ({_pageCount} pages, ~{_targetSizePerPage} bytes/page content target)";
                document.Info.Author = "PDFGenerator";
                document.Info.Subject = GeneratedFileName; // Add filename to metadata
                document.Options.CompressContentStreams = true;
                document.Options.NoCompression = false;

                // --- Page Generation Loop ---
                for (int i = 0; i < _pdfPages.Count; i++)
                {
                    PdfPage pdfSharpPage = document.AddPage();
                    pdfSharpPage.Size = PdfSharpCore.PageSize.A4; // TODO: Make this configurable if needed

                    using (XGraphics gfx = XGraphics.FromPdfPage(pdfSharpPage))
                    {
                        byte[] imageData = _pdfPages[i].GetContentImageData();

                        if (imageData != null && imageData.Length > 0)
                        {
                            try
                            {
                                using (var ms = new MemoryStream(imageData)) // Use MemoryStream for image data
                                {
                                    XImage image = XImage.FromStream(() => ms); // Lambda ensures MemoryStream isn't closed prematurely by XImage

                                    // Basic scaling logic (adjust as needed)
                                    double scaleRatio = pdfSharpPage.Width.Point / image.PointWidth;
                                    double drawWidth = pdfSharpPage.Width.Point;
                                    double drawHeight = image.PointHeight * scaleRatio;
                                    double yPos = (pdfSharpPage.Height.Point - drawHeight) / 2.0;
                                    if (yPos < 0) yPos = 0;

                                    gfx.DrawImage(image, 0, yPos, drawWidth, drawHeight);
                                } // MemoryStream for image disposed here
                            }
                            catch (Exception imgEx)
                            {
                                Console.WriteLine($"Error drawing image for page {i + 1}: {imgEx.Message}. Drawing placeholder.");
                                DrawPlaceholderText(gfx, pdfSharpPage, $"Error drawing content P.{i + 1}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Warning: No image data for page {i + 1}. Drawing placeholder.");
                            DrawPlaceholderText(gfx, pdfSharpPage, $"No content P.{i + 1}");
                        }
                    } // XGraphics disposed here
                } // End page loop

                // --- Save the document to the provided stream ---
                // Pass the leaveOpen parameter correctly
                document.Save(stream, leaveOpen);

            } // PdfDocument disposed here (which also finalizes writing to the stream)
        }

        // Helper method to draw placeholder text on a page
        private void DrawPlaceholderText(XGraphics gfx, PdfPage page, string text)
        {
            // Use a less intrusive style for placeholders
            XFont font = new XFont("Arial", 10, XFontStyle.Regular);
            XSolidBrush brush = XBrushes.LightGray;
            // Position top-left with some margin
            XRect rect = new XRect(20, 20, page.Width.Point - 40, page.Height.Point - 40);
            gfx.DrawString(text, font, brush, rect, XStringFormats.TopLeft);
        }

        // Helper method to build a unique filename
        private string BuildOutputFileName()
        {
            string uuid = Guid.NewGuid().ToString().Substring(0, 8);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            // Format: PDF_Pages_SizePerPage_Timestamp_UUID.pdf
            return $"PDF_{_pageCount}p_{_targetSizePerPage}bpp_{timestamp}_{uuid}.pdf";
        }
    } // End of PDFGenerator class
} // End of Logic namespace