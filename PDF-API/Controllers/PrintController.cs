using Logic.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http.Features;
using System.ComponentModel.DataAnnotations;

namespace PDF_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintController : ControllerBase
    {
        private readonly ILpdPrintService _lpdService;
        private readonly ILogger<PrintController> _logger;

        public PrintController(ILpdPrintService lpdService, ILogger<PrintController> logger)
        {
            _lpdService = lpdService;
            _logger = logger;
        }

        [HttpPost("TestConnection")]
        public async Task<IActionResult> TestConnection([FromBody] ConnectionTestRequest request)
        {
            if (string.IsNullOrEmpty(request.Host))
            {
                return BadRequest(new { success = false, message = "Host is required" });
            }

            int port = request.Port > 0 ? request.Port : 515;
            
            _logger.LogInformation("Testing connection to LPD server at {Host}:{Port}", request.Host, port);
            
            try
            {
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(request.Host, port);
                    var timeoutTask = Task.Delay(3000); // 3 second timeout
                    
                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        _logger.LogWarning("Connection to {Host}:{Port} timed out", request.Host, port);
                        return Ok(new { success = false, message = "Connection timed out" });
                    }
                    
                    if (client.Connected)
                    {
                        // If connection was successful, update the LpdPrintService configuration
                        _lpdService.UpdateConfiguration(request.Host, port);
                        
                        _logger.LogInformation("Successfully connected to LPD server at {Host}:{Port}", request.Host, port);
                        return Ok(new { success = true, message = "Connected successfully to LPD server" });
                    }
                    else
                    {
                        _logger.LogWarning("Failed to connect to LPD server at {Host}:{Port}", request.Host, port);
                        return Ok(new { success = false, message = "Failed to connect" });
                    }
                }
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "Socket error connecting to {Host}:{Port}: {Message}", request.Host, port, ex.Message);
                return Ok(new { success = false, message = $"Connection error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to {Host}:{Port}: {Message}", request.Host, port, ex.Message);
                return Ok(new { success = false, message = "Unexpected error during connection test" });
            }
        }

        // Simple GET endpoint for testing
        [HttpGet("GenerateSimple")]
        public async Task<IActionResult> GenerateSimple(
            [FromQuery] int pages = 10, 
            [FromQuery] int sizePerPage = 10, 
            [FromQuery] string sizeUnit = "MB",
            [FromQuery] string printerName = "default")
        {
            _logger.LogInformation("Received simple GET print request: PrinterName={PrinterName}, Pages={Pages}, SizePerPage={SizePerPage}, SizeUnit={SizeUnit}",
                printerName, pages, sizePerPage, sizeUnit);

            try
            {
                // Generate a PDF of the specified size
                _logger.LogInformation("Generating PDF with {Pages} pages, {Size} {Unit} per page", 
                    pages, sizePerPage, sizeUnit);
                
                long bytesPerPage = ConvertToBytes(sizePerPage, sizeUnit);
                var fileBytes = GeneratePdf(pages, bytesPerPage);
                
                _logger.LogInformation("Generated PDF of {Size} bytes", fileBytes.Length);

                _logger.LogInformation("Sending print job to LPD server, size: {Size} bytes", fileBytes.Length);
                var success = await _lpdService.SendPrintJob(printerName, fileBytes);
                
                if (success)
                {
                    _logger.LogInformation("Print job sent successfully");
                    return Ok(new { message = "Print job sent successfully", size = fileBytes.Length });
                }
                else
                {
                    _logger.LogError("Failed to send print job");
                    return StatusCode(500, new { message = "Failed to send print job" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing print job: {Message}", ex.Message);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Add a new endpoint specifically for size-based printing
        [HttpPost("GenerateAndPrint")]
        [RequestSizeLimit(1073741824)] // 1GB
        public async Task<IActionResult> GenerateAndPrint([FromForm] PrintSizeRequest request)
        {
            _logger.LogInformation("Received size-based print request: PrinterName={PrinterName}, Pages={Pages}, SizePerPage={SizePerPage}, SizeUnit={SizeUnit}, ServerHost={ServerHost}",
                request.PrinterName, request.Pages, request.SizePerPage, request.SizeUnit, request.ServerHost ?? "default");

            if (string.IsNullOrEmpty(request.PrinterName))
            {
                return BadRequest(new { message = "PrinterName is required" });
            }

            if (request.Pages <= 0)
            {
                return BadRequest(new { message = "Pages must be greater than 0" });
            }

            if (request.SizePerPage <= 0)
            {
                return BadRequest(new { message = "SizePerPage must be greater than 0" });
            }

            try
            {
                // Update the LPD service configuration if server host is provided
                if (!string.IsNullOrEmpty(request.ServerHost))
                {
                    int port = request.ServerPort > 0 ? request.ServerPort : 515;
                    _lpdService.UpdateConfiguration(request.ServerHost, port);
                    _logger.LogInformation("Using custom LPD server: {Host}:{Port}", request.ServerHost, port);
                }
                else
                {
                    // Use default server configuration (localhost:515)
                    _logger.LogInformation("Using default LPD server configuration");
                }

                // Generate a PDF of the specified size
                _logger.LogInformation("Generating PDF with {Pages} pages, {Size} {Unit} per page", 
                    request.Pages, request.SizePerPage, request.SizeUnit);
                
                long bytesPerPage = ConvertToBytes(request.SizePerPage, request.SizeUnit);
                var fileBytes = GeneratePdf(request.Pages, bytesPerPage);
                
                _logger.LogInformation("Generated PDF of {Size} bytes", fileBytes.Length);

                _logger.LogInformation("Sending print job to LPD server, size: {Size} bytes", fileBytes.Length);
                var success = await _lpdService.SendPrintJob(request.PrinterName, fileBytes);
                
                if (success)
                {
                    _logger.LogInformation("Print job sent successfully");
                    return Ok(new { message = "Print job sent successfully", size = fileBytes.Length });
                }
                else
                {
                    _logger.LogError("Failed to send print job");
                    return StatusCode(500, new { message = "Failed to send print job" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing print job: {Message}", ex.Message);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Original Print endpoint (kept for backward compatibility)
        [HttpPost]
        [RequestSizeLimit(1073741824)] // 1GB
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        public async Task<IActionResult> Print([FromForm] PrintRequest request)
        {
            _logger.LogInformation("Received print request for printer: {PrinterName}", request.PrinterName);

            if (string.IsNullOrEmpty(request.PrinterName))
            {
                _logger.LogWarning("Printer name is missing");
                return BadRequest(new { message = "Printer name is required" });
            }

            try
            {
                // Update the LPD service configuration if server host is provided
                if (!string.IsNullOrEmpty(request.ServerHost))
                {
                    int port = request.ServerPort > 0 ? request.ServerPort : 515;
                    _lpdService.UpdateConfiguration(request.ServerHost, port);
                    _logger.LogInformation("Using custom LPD server: {Host}:{Port}", request.ServerHost, port);
                }

                byte[] fileBytes;
                
                // Use uploaded file if provided, otherwise generate PDF based on size parameters
                if (request.File != null && request.File.Length > 0)
                {
                    _logger.LogInformation("Using uploaded file: {FileName}, Size: {FileSize} bytes", 
                        request.File.FileName, request.File.Length);

                    using var memoryStream = new MemoryStream();
                    await request.File.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
                else if (request.Pages > 0 && request.SizePerPage > 0)
                {
                    // Generate a PDF of the specified size
                    _logger.LogInformation("Generating PDF with {Pages} pages, {Size} {Unit} per page", 
                        request.Pages, request.SizePerPage, request.SizeUnit);
                    
                    long bytesPerPage = ConvertToBytes(request.SizePerPage, request.SizeUnit);
                    fileBytes = GeneratePdf(request.Pages, bytesPerPage);
                    
                    _logger.LogInformation("Generated PDF of {Size} bytes", fileBytes.Length);
                }
                else
                {
                    _logger.LogWarning("No file was uploaded and no size parameters provided");
                    return BadRequest(new { message = "Either a file must be uploaded or size parameters provided" });
                }

                _logger.LogInformation("Sending print job to LPD server, size: {Size} bytes", fileBytes.Length);
                var success = await _lpdService.SendPrintJob(request.PrinterName, fileBytes);
                
                if (success)
                {
                    _logger.LogInformation("Print job sent successfully");
                    return Ok(new { message = "Print job sent successfully", size = fileBytes.Length });
                }
                else
                {
                    _logger.LogError("Failed to send print job");
                    return StatusCode(500, new { message = "Failed to send print job" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing print job: {Message}", ex.Message);
                return StatusCode(500, new { message = ex.Message });
            }
        }
        
        private long ConvertToBytes(double size, string unit)
        {
            return unit?.ToLower() switch
            {
                "kb" => (long)(size * 1024),
                "mb" => (long)(size * 1024 * 1024),
                "gb" => (long)(size * 1024 * 1024 * 1024),
                _ => (long)size // Default to bytes
            };
        }
        
        private byte[] GeneratePdf(int pages, long bytesPerPage)
        {
            // Create a simple PDF with the specified number of pages
            using var memoryStream = new MemoryStream();
            
            // PDF Header - Simple PDF format
            string header = "%PDF-1.4\n";
            byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(header);
            memoryStream.Write(headerBytes, 0, headerBytes.Length);
            
            // Simplified content for each page
            for (int i = 0; i < pages; i++)
            {
                string pageContent = $"{i + 1} 0 obj\n<< /Type /Page /Contents {i + 1} 0 R >>\nendobj\n";
                byte[] pageBytes = System.Text.Encoding.ASCII.GetBytes(pageContent);
                memoryStream.Write(pageBytes, 0, pageBytes.Length);
            }
            
            // Add padding to reach the target size
            long currentSize = memoryStream.Length;
            long totalTargetSize = pages * bytesPerPage;
            long paddingSize = totalTargetSize - currentSize;
            
            if (paddingSize > 0)
            {
                // Add padding as random bytes
                var random = new Random();
                byte[] padding = new byte[paddingSize];
                random.NextBytes(padding);
                memoryStream.Write(padding, 0, padding.Length);
            }
            
            // PDF Footer
            string footer = "%%EOF";
            byte[] footerBytes = System.Text.Encoding.ASCII.GetBytes(footer);
            memoryStream.Write(footerBytes, 0, footerBytes.Length);
            
            return memoryStream.ToArray();
        }
    }

    // DTO classes
    public class PrintRequest
    {
        public string? PrinterName { get; set; }
        public string? ServerHost { get; set; }
        public int ServerPort { get; set; }
        public IFormFile? File { get; set; }
        public int Pages { get; set; }
        public double SizePerPage { get; set; }
        public string SizeUnit { get; set; } = "MB";
    }

    // New request class specifically for size-based printing
    public class PrintSizeRequest
    {
        public string? PrinterName { get; set; }
        public string? ServerHost { get; set; } = "localhost";
        public int ServerPort { get; set; } = 515;
        public int Pages { get; set; }
        public double SizePerPage { get; set; }
        public string SizeUnit { get; set; } = "MB";
    }

    public class ConnectionTestRequest
    {
        public string? Host { get; set; }
        public int Port { get; set; }
    }
} 