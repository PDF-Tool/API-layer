using System.Drawing;
using Microsoft.AspNetCore.Mvc;

namespace PDF_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PDFGeneratorController : ControllerBase
    {
        private enum ByteUnits
        {
            GB,
            MB
        };

        private enum MetricUnits
        {
            mm,
            cm
        }

        private int Pages;

        private int Size;
        private int Square;
        private string ByteUnit;
        private string MetricUnit;

        public class RequestModel
        {
            public int? Pages { get; set; }
            public int? Size { get; set; }
            public string? ByteUnit { get; set; }
            public int? Width { get; set; }
            public int? Height { get; set; }
            public string? MetricUnit { get; set; }
            public int? Square { get; set; }
            public string? Format { get; set; }
        }

        [Route("GeneretePDF")]
        [HttpPost]
        public IActionResult ResponseDocument([FromBody] RequestModel request)
        {
            try
            {
                IActionResult validationResult = CleanseInputs(request);
                if (validationResult != null)
                {
                    return validationResult;
                }

                if (request.Width.HasValue || request.Height.HasValue)
                {
                    return CheckWidthAndHeight(Pages, request.Width, request.Height, MetricUnit, ByteUnit, Size);
                }

                if (request.Square.HasValue)
                {
                    Square = CheckIfNullOrHigherThanZero(request.Square);
                    return Content($"{Pages} pages of {Square} {MetricUnit}² and {Size} {ByteUnit} each");
                }

                if (request.Format != null)
                {
                    return Content("Pages");
                }

                return Content($"{Pages} pages of A4 format and {Size} {ByteUnit} each");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        private IActionResult CheckWidthAndHeight(int? Pages, int? Width, int? Height, string? MetricUnit, string? ByteUnit, int? Size)
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
                Width = CheckIfNullOrHigherThanZero(Width);
                Height = CheckIfNullOrHigherThanZero(Height);
                return Content($"{Pages} pages of {Width}x{Height} {MetricUnit} and {Size} {ByteUnit} each");
            }
        }

        private int CheckIfNullOrHigherThanZero(int? value)
        {
            if (value.HasValue && value.Value > 0)
            {
                return value.Value;
            }
            else
            {
                return 1;
            }
        }

        private string CheckByteUnit(string ByteUnit)
        {
            if (ByteUnit == null)
            {
                return ByteUnit = ByteUnits.MB.ToString();
            }
            else
            {
                ByteUnit = ByteUnit.ToUpper();
                if (!Enum.IsDefined(typeof(ByteUnits), ByteUnit) || int.TryParse(ByteUnit, out _))
                {
                    return null;
                }
                else
                {
                    return ByteUnit;
                }
            }
        }

        private string CheckMetricUnit(string MetricUnit)
        {
            if (MetricUnit == null)
            {
                return MetricUnit = MetricUnits.mm.ToString();
            }
            else
            {
                MetricUnit = MetricUnit.ToLower();
                if (!Enum.IsDefined(typeof(MetricUnits), MetricUnit) || int.TryParse(MetricUnit, out _))
                {
                    return null;
                }
                else
                {
                    return MetricUnit;
                }
            }
        }

        private IActionResult CleanseInputs(RequestModel request)
        {
            Pages = CheckIfNullOrHigherThanZero(request.Pages);

            if (!request.Size.HasValue)
            {
                return BadRequest("No size given");
            }

            Size = CheckIfNullOrHigherThanZero(request.Size);

            var byteUnitResult = CheckByteUnit(request.ByteUnit);
            if (byteUnitResult == null)
            {
                return BadRequest("Byte unit specified is invalid, only MB and GB are allowed");
            }
            ByteUnit = byteUnitResult;

            var metricUnitResult = CheckMetricUnit(request.MetricUnit);
            if (metricUnitResult == null)
            {
                return BadRequest("Metric unit specified is invalid, only mm and cm are allowed");
            }
            MetricUnit = metricUnitResult;

            return null;
        }
    }
}
