using System.Drawing;
using Microsoft.AspNetCore.Mvc;

namespace PDF_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PDFGeneratorController : ControllerBase
    {

        private PDFGenerator PdfController = new PDFGenerator();

        private enum ByteFormats
        {
            Gigabyte,
            Megabyte,
        };

        

        private string? UsedByteFormat;

        private int? AmountOfBytes;

        [Route(template: "response")]
        [HttpGet]
        public IActionResult ResponseDocument(
            [FromQuery] int? Pages,
            [FromQuery] int? MB,
            [FromQuery] int? GB,
            [FromQuery] int? Width,
            [FromQuery] int? Height,
            [FromQuery] int? Square,
            [FromQuery] string? Format
        )

        {
            try
            {
                if (!Pages.HasValue)
                {
                    Pages = 1;
                }

                if (!DetermineUsedByteFormat(GB, MB, out AmountOfBytes, out UsedByteFormat))
                {
                    Console.WriteLine("No byte format specified");
                    return BadRequest("No byte format specified");
                }

                if(Width.HasValue || Height.HasValue)
                {
                    return CheckWidthAndHeight(Pages, Width, Height, AmountOfBytes, UsedByteFormat);
                }

                if (Square.HasValue)
                {
                    PdfController.GeneratePDF(Pages.Value, AmountOfBytes.Value, Square.Value, Square.Value);
                    Console.WriteLine($"{Pages} pages of {Square}mm2 and {AmountOfBytes} {UsedByteFormat} each");
                    return Content($"{Pages} pages of {Square}mm2 and {AmountOfBytes} {UsedByteFormat} each");
                }

                if (Format != null)
                {
                    PdfController.GeneratePDF(Pages.Value, AmountOfBytes.Value, Format);
                    return Content("Pages");
                }

                
                PdfController.GeneratePDF(Pages.Value, AmountOfBytes.Value);
                Console.WriteLine($"{Pages} pages of A4 format and {AmountOfBytes} {UsedByteFormat} each");
                return Content($"{Pages} pages of A4 format and {AmountOfBytes} {UsedByteFormat} each");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        private bool DetermineUsedByteFormat(int? GB, int? MB, out int? AmountOfBytes, out string? UsedByteFormat)
        {
            if (GB.HasValue)
            {
                AmountOfBytes = GB;
                UsedByteFormat = ByteFormats.Gigabyte.ToString();
                return true;
            }
            else if (MB.HasValue)
            {
                AmountOfBytes = MB;
                UsedByteFormat = ByteFormats.Megabyte.ToString();
                return true;
            }
            else
            {
                AmountOfBytes = null;
                UsedByteFormat = null;
                return false;
            }
        }

        private IActionResult CheckWidthAndHeight(int? Pages, int? Width, int? Height, int? AmountOfBytes, string? UsedByteFormat)
        {
            if (Width.HasValue && !Height.HasValue)
            {
                Console.WriteLine("Width given but no height");
                return BadRequest("Width given but no height");
            }
            else if (Height.HasValue && !Width.HasValue)
            {
                Console.WriteLine("Heigt given but no width");
                return BadRequest("Heigt given but no width");
            }
            else
            {
                PdfController.GeneratePDF(Pages.Value, AmountOfBytes.Value, Width.Value, Height.Value);
                Console.WriteLine($"{Pages} pages of {Width}x{Height}mm and {AmountOfBytes} {UsedByteFormat} each");
                return Content($"{Pages} pages of {Width}x{Height}mm and {AmountOfBytes} {UsedByteFormat} each");
            }
        }

        
    }
}
