using Logic;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PDF_API.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestPDFController : ControllerBase
    {
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateTestPDF([FromQuery] int pages = 1, [FromQuery] int sizePerPageMB = 1)
        {
            try
            {
                // Convert MB to bytes
                int sizePerPageBytes = sizePerPageMB * 1024 * 1024;

                // Create generator and save locally
                var generator = new PDFGenerator(pages, sizePerPageBytes);
                string savedPath = await generator.GenerateAndSaveLocally();

                return Ok(new
                {
                    Message = "PDF generated successfully",
                    FilePath = savedPath,
                    Pages = pages,
                    SizePerPageMB = sizePerPageMB
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Failed to generate PDF",
                    Message = ex.Message
                });
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Status = "Test PDF Generator is running",
                Timestamp = DateTime.UtcNow
            });
        }
    }
} 