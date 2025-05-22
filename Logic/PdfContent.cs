using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Logic
{
    public class PDFContent
    {
        private readonly int _pageCount;
        private readonly int _targetSizePerPage;
        private readonly string _generatedFileName;

        public PDFContent(int pages, int targetSizePerPageBytes)
        {
            _pageCount = pages;
            _targetSizePerPage = targetSizePerPageBytes;
            _generatedFileName = $"GeneratedPDF_{DateTime.Now:yyyyMMdd_HHmmss}_{_pageCount}pages_{_targetSizePerPage}bytes.pdf";
        }

        public string GeneratedFileName => _generatedFileName;

        public async Task GenerateAndWriteStreamAsync(Stream outputStream)
        {
            using var document = new PdfSharpCore.Pdf.PdfDocument();
            for (int i = 0; i < _pageCount; i++)
            {
                var page = new PDFPage(_targetSizePerPage);
                var pdfPage = document.AddPage();
                pdfPage.Size = PdfSharpCore.PageSize.A4;

                using var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(pdfPage);
                var pngBytes = page.GetUncompressedPngBytes();
                using var ms = new MemoryStream(pngBytes);
                using var image = PdfSharpCore.Drawing.XImage.FromStream(() => ms);

                double scale = Math.Min(pdfPage.Width / image.PixelWidth, pdfPage.Height / image.PixelHeight);
                double width = image.PixelWidth * scale;
                double height = image.PixelHeight * scale;
                double x = (pdfPage.Width - width) / 2;
                double y = (pdfPage.Height - height) / 2;

                gfx.DrawImage(image, x, y, width, height);

                await Task.Yield();
            }
            document.Save(outputStream, false);
        }
    }
}