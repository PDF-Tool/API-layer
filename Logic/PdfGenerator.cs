using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Logic
{
    public class PDFGenerator
    {
        private readonly int _pageCount;
        private readonly int _targetSizePerPage;
        private readonly string _generatedFileName;

        public PDFGenerator(int pages, int targetSizePerPageBytes)
        {
            if (pages <= 0) throw new ArgumentOutOfRangeException(nameof(pages), "Number of pages must be positive.");
            if (targetSizePerPageBytes <= 0) throw new ArgumentOutOfRangeException(nameof(targetSizePerPageBytes), "Target size per page must be positive.");

            _pageCount = pages;
            _targetSizePerPage = targetSizePerPageBytes;
            _generatedFileName = $"GeneratedPDF_{DateTime.Now:yyyyMMdd_HHmmss}_{_pageCount}pages_{_targetSizePerPage}bytes.pdf";
        }

        public string GeneratedFileName => _generatedFileName;

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

                // Reuse a single MemoryStream for all pages
                using var ms = new MemoryStream();

                // Generate and write pages one at a time
                for (int i = 0; i < _pageCount; i++)
                {
                    var page = new PDFPage(_targetSizePerPage);
                    var pdfPage = document.AddPage();
                    pdfPage.Size = PdfSharpCore.PageSize.A4;

                    using var gfx = XGraphics.FromPdfPage(pdfPage);
                    var pngBytes = page.GetUncompressedPngBytes();
                    
                    // Reset and reuse the MemoryStream
                    ms.SetLength(0);
                    ms.Write(pngBytes, 0, pngBytes.Length);
                    ms.Position = 0;

                    using var image = XImage.FromStream(() => ms);

                    double scale = Math.Min(pdfPage.Width / image.PixelWidth, pdfPage.Height / image.PixelHeight);
                    double width = image.PixelWidth * scale;
                    double height = image.PixelHeight * scale;
                    double x = (pdfPage.Width - width) / 2;
                    double y = (pdfPage.Height - height) / 2;

                    gfx.DrawImage(image, x, y, width, height);

                    await Task.Yield();
                }

                document.Save(outputStream, false);
                stopwatch.Stop();
                Console.WriteLine($"PDF generation complete ({stopwatch.ElapsedMilliseconds} ms)");
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
                Console.WriteLine($"File size: {fileInfo.Length / 1024:F2} KB");
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
    }
}