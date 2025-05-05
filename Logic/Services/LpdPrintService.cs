using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Logic.Services
{
    public interface ILpdPrintService
    {
        Task<bool> SendPrintJob(string printerName, byte[] jobData);
        void UpdateConfiguration(string serverHost, int serverPort);
    }

    public class LpdPrintService : ILpdPrintService
    {
        private readonly ILogger<LpdPrintService> _logger;
        private LpdPrintServiceOptions _options;

        public LpdPrintService(ILogger<LpdPrintService> logger, LpdPrintServiceOptions options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void UpdateConfiguration(string serverHost, int serverPort)
        {
            if (!string.IsNullOrEmpty(serverHost))
            {
                _options.ServerHost = serverHost;
                _logger.LogInformation("Updated LPD server host to: {ServerHost}", serverHost);
            }

            if (serverPort > 0)
            {
                _options.ServerPort = serverPort;
                _logger.LogInformation("Updated LPD server port to: {ServerPort}", serverPort);
            }
        }

        public async Task<bool> SendPrintJob(string printerName, byte[] jobData)
        {
            if (string.IsNullOrEmpty(printerName))
                throw new ArgumentException("Printer name cannot be null or empty", nameof(printerName));
            
            if (jobData == null || jobData.Length == 0)
                throw new ArgumentException("Job data cannot be null or empty", nameof(jobData));

            int retryCount = 0;
            while (retryCount < _options.MaxRetries)
            {
                try
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync(_options.ServerHost, _options.ServerPort);
                    
                    _logger.LogInformation("Connected to LPD server at {Host}:{Port}", 
                        _options.ServerHost, _options.ServerPort);

                    using var stream = client.GetStream();
                    
                    // Create a unique job ID
                    string jobId = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    // 1. Send receive-job command (1 byte + queue name + newline)
                    byte[] receiveJobCmd = new byte[printerName.Length + 2];
                    receiveJobCmd[0] = 0x02; // 0x02 is the receive-job command
                    Encoding.ASCII.GetBytes(printerName).CopyTo(receiveJobCmd, 1);
                    receiveJobCmd[receiveJobCmd.Length - 1] = 0x0A; // newline
                    
                    await stream.WriteAsync(receiveJobCmd, 0, receiveJobCmd.Length);
                    await stream.FlushAsync();
                    
                    // 2. Wait for acknowledgment (0 byte)
                    int response = stream.ReadByte();
                    if (response != 0)
                    {
                        _logger.LogError("Invalid response from LPD server: {Response}", response);
                        throw new LpdException("LPD server did not acknowledge receive-job command");
                    }
                    
                    // 3. Send control file
                    string controlFileName = $"cfA{jobId}{_options.HostName}";
                    string controlFileContent = 
                        $"H{_options.HostName}\n" +
                        $"P{_options.UserName}\n" +
                        $"J{jobId}\n" +
                        $"C{_options.UserName}\n" +
                        $"L{_options.UserName}\n" +
                        $"UdfA{jobId}{_options.HostName}\n" +
                        $"N{jobId}.pdf\n";

                    byte[] controlFileCmd = Encoding.ASCII.GetBytes(
                        $"\x02{controlFileContent.Length} {controlFileName}\n");
                    await stream.WriteAsync(controlFileCmd, 0, controlFileCmd.Length);
                    await stream.FlushAsync();
                    
                    // Wait for acknowledgment for receive-control-file
                    response = stream.ReadByte();
                    if (response != 0)
                    {
                        _logger.LogError("LPD server did not acknowledge control file command: {Response}", response);
                        throw new LpdException("LPD server did not acknowledge control file command");
                    }
                    
                    // Send control file content + null terminator
                    byte[] controlFileBytes = Encoding.ASCII.GetBytes(controlFileContent);
                    await stream.WriteAsync(controlFileBytes, 0, controlFileBytes.Length);
                    stream.WriteByte(0); // Null terminator
                    await stream.FlushAsync();
                    
                    // Wait for acknowledgment for control-file
                    response = stream.ReadByte();
                    if (response != 0)
                    {
                        _logger.LogError("LPD server did not accept control file: {Response}", response);
                        throw new LpdException("LPD server did not accept control file");
                    }
                    
                    // 4. Send data file command
                    string dataFileName = $"dfA{jobId}{_options.HostName}";
                    byte[] dataFileCmd = Encoding.ASCII.GetBytes($"\x03{jobData.Length} {dataFileName}\n");
                    await stream.WriteAsync(dataFileCmd, 0, dataFileCmd.Length);
                    await stream.FlushAsync();
                    
                    // Wait for acknowledgment for receive-data-file
                    response = stream.ReadByte();
                    if (response != 0)
                    {
                        _logger.LogError("LPD server did not acknowledge data file command: {Response}", response);
                        throw new LpdException("LPD server did not acknowledge data file command");
                    }
                    
                    // Send data file content + null terminator
                    await stream.WriteAsync(jobData, 0, jobData.Length);
                    stream.WriteByte(0); // Null terminator
                    await stream.FlushAsync();
                    
                    // Wait for acknowledgment for data-file
                    response = stream.ReadByte();
                    if (response != 0)
                    {
                        _logger.LogError("LPD server did not accept data file: {Response}", response);
                        throw new LpdException("LPD server did not accept data file");
                    }

                    _logger.LogInformation("Successfully sent print job to printer {PrinterName}", printerName);
                    return true;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogError(ex, "Failed to send print job to {PrinterName} (attempt {RetryCount}/{MaxRetries})", 
                        printerName, retryCount, _options.MaxRetries);

                    if (retryCount >= _options.MaxRetries)
                    {
                        throw new LpdException($"Failed to send print job after {_options.MaxRetries} attempts", ex);
                    }

                    await Task.Delay(_options.RetryDelay);
                }
            }

            return false;
        }
    }

    public class LpdPrintServiceOptions
    {
        public string ServerHost { get; set; } = "printer-host";
        public int ServerPort { get; set; } = 515;
        public string HostName { get; set; } = "localhost";
        public string UserName { get; set; } = "user";
        public int MaxRetries { get; set; } = 3;
        public int RetryDelay { get; set; } = 1000; // milliseconds
    }

    public class LpdException : Exception
    {
        public LpdException(string message) : base(message) { }
        public LpdException(string message, Exception innerException) : base(message, innerException) { }
    }
}