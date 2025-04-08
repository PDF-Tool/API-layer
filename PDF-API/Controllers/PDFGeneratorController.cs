using System.Drawing;
using Microsoft.AspNetCore.Mvc;

namespace PDF_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PDFGeneratorController : ControllerBase
    {
        private enum ByteFormats
        {
            Gigabyte,
            Megabyte,
        };

        private string? UsedByteFormat;

        private int? AmountOfBytes;

        public class RequestModel
        {
            public int? Pages { get; set; }
            public int? MB { get; set; }
            public int? GB { get; set; }
            public int? Width { get; set; }
            public int? Height { get; set; }
            public int? Square { get; set; }
            public string? Format { get; set; }
        }

        [Route("response")]
        [HttpPost]
        public IActionResult ResponseDocument([FromBody] RequestModel request)
        {
            try
            {
                int Pages;
                if(!request.Pages.HasValue || request.Pages.Value < 1){
                    Pages = 1;
                }else{
                    Pages = request.Pages.Value;
                }

                if (!DetermineUsedByteFormat(request.GB, request.MB, out AmountOfBytes, out UsedByteFormat))
                {
                    return BadRequest("No byte format specified");
                }

                if (request.Width.HasValue || request.Height.HasValue)
                {
                    return CheckWidthAndHeight(Pages, request.Width, request.Height, AmountOfBytes, UsedByteFormat);
                }

                if (request.Square.HasValue)
                {
                    return Content($"{Pages} pages of {request.Square}mm² and {AmountOfBytes} {UsedByteFormat} each");
                }

                if (request.Format != null)
                {
                    return Content("Pages");
                }

                return Content($"{Pages} pages of A4 format and {AmountOfBytes} {UsedByteFormat} each");
            }
            catch (Exception ex)
            {
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
                return BadRequest("Width given but no height");
            }
            else if (Height.HasValue && !Width.HasValue)
            {
                return BadRequest("Heigt given but no width");
            }
            else
            {
                return Content($"{Pages} pages of {Width}x{Height}mm and {AmountOfBytes} {UsedByteFormat} each");
            }
        }
    }
}
