using System.Drawing;
using Microsoft.AspNetCore.Mvc;

namespace PDF_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PDFGeneratorController : ControllerBase
    {

        //private PDFGenerator PdfController = new PDFGenerator();

        private enum ByteFormats
        {
            Gigabyte,
            Megabyte,
            Kilobyte
        };

        private enum DocumentSizeFormats
        {
            A0,
            A1,
            A2,
            A3,
            A4,
            A5,
            A6,
            B0,
            B1,
            B2,
            B3,
            B4,
            B5,
            B6,
            C0,
            C1,
            C2,
            C3,
            C4,
            C5,
            C6,
            Letter,
            Legal,
            Tabloid,
            Ledger,
            Executive,
            Folio
        }

        private string? UsedByteFormat;

        private int? AmountOfBytes;

        [Route(template: "response")]
        [HttpGet]
        public IActionResult ResponseDocument(
            [FromQuery] int? Pages,
            [FromQuery] int? KB,
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

                if (!DetermineUsedByteFormat(GB, MB, KB, out AmountOfBytes, out UsedByteFormat))
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
                    Console.WriteLine($"{Pages} pages of {Square}mm2 and {AmountOfBytes} {UsedByteFormat} each");
                    return Content($"{Pages} pages of {Square}mm2 and {AmountOfBytes} {UsedByteFormat} each");
                }

                if (Format != null)
                {
                    return CheckFormat(Format, Pages, AmountOfBytes.Value, UsedByteFormat);
                }

                
                //PdfController.GeneratePDF(Pages.Value, AmountOfBytes.Value);
                Console.WriteLine($"{Pages} pages of {DocumentSizeFormats.A4} format and {AmountOfBytes} {UsedByteFormat} each");
                return Content($"{Pages} pages of {DocumentSizeFormats.A4} format and {AmountOfBytes} {UsedByteFormat} each");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        private bool DetermineUsedByteFormat(int? GB, int? MB, int? KB, out int? AmountOfBytes, out string? UsedByteFormat)
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
            else if (KB.HasValue)
            {
                AmountOfBytes = KB;
                UsedByteFormat = ByteFormats.Kilobyte.ToString();
                return true;
            }
            else
            {
                AmountOfBytes = null;
                UsedByteFormat = null;
                return false;
            }
        }

        private string ToCapitalCase(string Input)
        {
            return char.ToUpper(Input[0]) + Input.Substring(1).ToLower();
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
                Console.WriteLine($"{Pages} pages of {Width}x{Height}mm and {AmountOfBytes} {UsedByteFormat} each");
                return Content($"{Pages} pages of {Width}x{Height}mm and {AmountOfBytes} {UsedByteFormat} each");
            }
        }

        private IActionResult CheckFormat(string? Format, int? Pages, int AmountOfBytes, string? UsedByteFormat)
        {
            Format = ToCapitalCase(Format);

            var FormatDimension = GetDocumentSizeFormat(Format);

            if (FormatDimension != null)
            {
                Console.WriteLine($"{Pages} pages of {Format} format and {AmountOfBytes} {UsedByteFormat} each");
                return Content($"{Pages} pages of {FormatDimension.Value.Width}x{FormatDimension.Value.Height} and {AmountOfBytes} {UsedByteFormat} each");
            }

            Console.WriteLine("Invalid format");
            return BadRequest("Invalid format");
        }


        private (DocumentSizeFormats Format, int Width, int Height)? GetDocumentSizeFormat(string format)
        {
            if (Enum.TryParse(typeof(DocumentSizeFormats), format, true, out var formatEnum) &&
                formatEnum is DocumentSizeFormats validFormat)
            {
                if (!Enum.IsDefined(typeof(DocumentSizeFormats), validFormat) || int.TryParse(format, out _))
                {
                    return null;
                }

                var documentSizes = new Dictionary<DocumentSizeFormats, (int Width, int Height)>
                {
                    { DocumentSizeFormats.A0, (841, 1189) },
                    { DocumentSizeFormats.A1, (594, 841) },
                    { DocumentSizeFormats.A2, (420, 594) },
                    { DocumentSizeFormats.A3, (297, 420) },
                    { DocumentSizeFormats.A4, (210, 297) },
                    { DocumentSizeFormats.A5, (148, 210) },
                    { DocumentSizeFormats.A6, (105, 148) },
                    { DocumentSizeFormats.B0, (1000, 1414) },
                    { DocumentSizeFormats.B1, (707, 1000) },
                    { DocumentSizeFormats.B2, (500, 707) },
                    { DocumentSizeFormats.B3, (353, 500) },
                    { DocumentSizeFormats.B4, (250, 353) },
                    { DocumentSizeFormats.B5, (176, 250) },
                    { DocumentSizeFormats.B6, (125, 176) },
                    { DocumentSizeFormats.C0, (917, 1297) },
                    { DocumentSizeFormats.C1, (648, 917) },
                    { DocumentSizeFormats.C2, (458, 648) },
                    { DocumentSizeFormats.C3, (324, 458) },
                    { DocumentSizeFormats.C4, (229, 324) },
                    { DocumentSizeFormats.C5, (162, 229) },
                    { DocumentSizeFormats.C6, (114, 162) },
                    { DocumentSizeFormats.Letter, (216, 279) },
                    { DocumentSizeFormats.Legal, (216, 356) },
                    { DocumentSizeFormats.Tabloid, (279, 432) },
                    { DocumentSizeFormats.Ledger, (432, 279) },
                    { DocumentSizeFormats.Executive, (184, 267) },
                    { DocumentSizeFormats.Folio, (216, 330) }
                };

                var size = documentSizes[validFormat];
                return (validFormat, size.Width, size.Height);
            }

            return null;
        }
    }
}
