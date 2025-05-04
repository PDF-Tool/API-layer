// PDF-API/Controllers/LprController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Logic; // Your Logic namespace
using System; // For Exception

namespace PDF_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LprController : ControllerBase
    {
        // No dependencies needed if LprClient.CheckConnectionAsync is static
        // If it were an instance method, inject LprClient or a factory

        public LprController()
        {
            // Constructor
        }

        // GET api/lpr/check?host=your_printer_ip&port=515
        [HttpGet("check")]
        public async Task<IActionResult> CheckConnection(
            [FromQuery] string host,
            [FromQuery] int port = 515) // Default LPR port
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return BadRequest(new { Connected = false, Message = "Host parameter is required." });
            }

            try
            {
                var (success, errorMessage) = await LprClient.CheckConnectionAsync(host, port);

                if (success)
                {
                    return Ok(new
                    {
                        Connected = true,
                        Message = $"Successfully connected to {host}:{port}."
                    });
                }

                return Ok(new
                { 
                        Connected = false, 
                        Message = $"{errorMessage}"
                });

            }
            catch (Exception ex)
            {
                 // Log the exception details server-side
                 Console.WriteLine($"Error during LPR connection check to {host}:{port} - {ex.ToString()}");
                 // Return a 500 Internal Server Error for unexpected issues
                 return StatusCode(500, new { Connected = false, Message = $"An internal error occurred: {ex.Message}" });
            }
        }
    }
}