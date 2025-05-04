// Logic/LprClient.cs
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class LprClient
    {
        private const int DefaultLprPort = 515;

        private readonly string _serverHost;
        private readonly int _serverPort;
        private readonly string _queueName;

        // Constructor accepting host and queue, using default port
        public LprClient(string serverHost, string queueName)
            : this(serverHost, queueName, DefaultLprPort)
        {
        }

        // Constructor accepting host, queue, and port
        public LprClient(string serverHost, string queueName, int serverPort)
        {
            _serverHost = serverHost ?? throw new ArgumentNullException(nameof(serverHost));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            _serverPort = serverPort;

             if (string.IsNullOrWhiteSpace(_serverHost))
                throw new ArgumentException("Server host cannot be empty.", nameof(serverHost));
             if (string.IsNullOrWhiteSpace(_queueName))
                throw new ArgumentException("Queue name cannot be empty.", nameof(queueName));
             if (_serverPort <= 0 || _serverPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(serverPort), "Port must be between 1 and 65535.");

        }

        // Method to send PDF from a file path (Kept for potential other uses)
        // Returns true on success, false on failure.
        public async Task<bool> SendPrintJobFromFileAsync(string pdfFilePath)
        {
            if (!File.Exists(pdfFilePath))
            {
                Console.WriteLine($"Error: PDF file not found at '{pdfFilePath}'");
                return false;
            }
            string fileName = Path.GetFileName(pdfFilePath);
            byte[] pdfData = await File.ReadAllBytesAsync(pdfFilePath);
            return await SendPrintJobInternalAsync(pdfData, fileName);
        }

        // Method to send PDF from byte array
        // Returns true on success, false on failure. Throws exceptions on connection issues.
        public async Task<bool> SendPrintJobAsync(byte[] pdfData, string sourceFileName)
        {
            if (pdfData == null || pdfData.Length == 0)
            {
                Console.WriteLine($"Error: PDF data is null or empty for file '{sourceFileName}'");
                return false;
            }
             if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                 Console.WriteLine($"Error: Source file name is required.");
                 return false;
            }

            // Delegate to the internal implementation
            // Exceptions during connection or protocol steps will propagate up
            return await SendPrintJobInternalAsync(pdfData, sourceFileName);
        }


        // Internal method containing the core LPR logic
        private async Task<bool> SendPrintJobInternalAsync(byte[] pdfDataBytes, string sourceFileName)
        {
            Console.WriteLine($"Connecting to LPR server at {_serverHost}:{_serverPort} for queue '{_queueName}'...");
            using TcpClient client = new TcpClient();
            try
            {
                // Use a timeout for connection attempts
                var connectTask = client.ConnectAsync(_serverHost, _serverPort);
                if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask) // 5-second timeout
                {
                     client.Close(); // Ensure client is closed on timeout
                     throw new SocketException((int)SocketError.TimedOut);
                }
                // If connectTask completed, check for exceptions
                await connectTask; // Propagate potential exception from ConnectAsync
            }
            catch (SocketException sockEx)
            {
                Console.WriteLine($"LPR Connection failed to {_serverHost}:{_serverPort}: {sockEx.Message} (SocketError: {sockEx.SocketErrorCode})");
                // Rethrow a more specific exception or return false, based on desired handling
                // Rethrowing allows the caller (background task) to catch and report specific failure
                throw new LprCommunicationException($"Failed to connect to LPR server {_serverHost}:{_serverPort}. Error: {sockEx.Message}", sockEx);
            }
            catch (Exception ex) // Catch other potential non-socket connection errors
            {
                Console.WriteLine($"LPR Connection failed: {ex.Message}");
                 throw new LprCommunicationException($"Failed to connect to LPR server {_serverHost}:{_serverPort}. Error: {ex.Message}", ex);
            }

            Console.WriteLine("LPR Connected successfully.");
            using NetworkStream stream = client.GetStream();
            // Set timeouts for read/write operations
            stream.ReadTimeout = 10000; // 10 seconds
            stream.WriteTimeout = 10000; // 10 seconds

            try
            {
                string hostName = Dns.GetHostName();
                string userName = Environment.UserName;
                int jobId = new Random().Next(1, 1000);
                string controlFileName = $"cfA{jobId:D3}{hostName}";
                string dataFileName = $"dfA{jobId:D3}{hostName}";

                Console.WriteLine($"Preparing LPR job: ID={jobId}, SrcFile={sourceFileName}, CtrlFile={controlFileName}, DataFile={dataFileName}");

                // Step 2: Receive Printer Job command
                await SendCommandAsync(stream, $"\x02{_queueName}\n");
                await WaitForAckAsync(stream, "Receive Job command");

                // Step 4a: Control File Content
                string controlFileContent = $"H{hostName}\n" +
                                            $"P{userName}\n" +
                                            $"N{sourceFileName}\n" + // Use the provided source file name
                                            $"l{dataFileName}\n"; // Print data file raw
                byte[] controlFileBytes = Encoding.ASCII.GetBytes(controlFileContent);

                // Step 4b: Receive Control File subcommand
                await SendCommandAsync(stream, $"\x02{controlFileBytes.Length} {controlFileName}\n");
                await WaitForAckAsync(stream, "Receive Control File subcommand");

                // Step 6: Send Control File data
                await stream.WriteAsync(controlFileBytes, 0, controlFileBytes.Length);
                // Step 7: Send Control File Completion Byte (0x00)
                await stream.WriteAsync(new byte[] { 0x00 }, 0, 1);
                await stream.FlushAsync();
                await WaitForAckAsync(stream, "Control File data");

                // Step 9b: Receive Data File subcommand
                await SendCommandAsync(stream, $"\x03{pdfDataBytes.Length} {dataFileName}\n");
                await WaitForAckAsync(stream, "Receive Data File subcommand");

                // Step 11: Send PDF data (Consider chunking for very large files if needed)
                Console.WriteLine($"Sending PDF data ({pdfDataBytes.Length} bytes)...");
                await stream.WriteAsync(pdfDataBytes, 0, pdfDataBytes.Length);
                // Step 12: Send Data File Completion Byte (0x00)
                await stream.WriteAsync(new byte[] { 0x00 }, 0, 1);
                await stream.FlushAsync();
                await WaitForAckAsync(stream, "PDF data");

                Console.WriteLine($"\nLPR Print job '{sourceFileName}' sent successfully to {_serverHost}:{_serverPort} queue '{_queueName}'.");
                return true; // Indicate success
            }
            catch (IOException ioEx) // Catches potential stream read/write errors/timeouts
            {
                Console.WriteLine($"\n--- LPR IO Error during communication ---");
                Console.WriteLine(ioEx.Message);
                // Rethrow custom exception
                throw new LprCommunicationException($"IO error during LPR communication with {_serverHost}:{_serverPort}. Error: {ioEx.Message}", ioEx);
            }
            catch (LprAckException ackEx) // Catch specific ACK errors
            {
                 Console.WriteLine($"\n--- LPR Protocol Error ---");
                 Console.WriteLine(ackEx.Message);
                 // Rethrow
                 throw;
            }
            catch (Exception ex) // Catch other unexpected errors during protocol steps
            {
                Console.WriteLine($"\n--- An unexpected error occurred during LPR communication ---");
                Console.WriteLine(ex.Message);
                 // Rethrow custom exception
                 throw new LprCommunicationException($"Unexpected error during LPR communication with {_serverHost}:{_serverPort}. Error: {ex.Message}", ex);
            }
            // finally // No finally block needed as using statements handle disposal
            // {
            //     Console.WriteLine("LPR Connection closed."); // Logged implicitly by using TcpClient/NetworkStream
            // }
        }

        // *** NEW Connection Check Method ***
        public static async Task<(bool Success, string ErrorMessage)> CheckConnectionAsync(string host, int port = DefaultLprPort)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return (false, "Host name cannot be empty.");
            }
             if (port <= 0 || port > 65535)
            {
                 return (false, "Port must be between 1 and 65535.");
            }

            Console.WriteLine($"Checking LPR connection to {host}:{port}...");
            using var client = new TcpClient();
            try
            {
                // Use a short timeout for connection check
                var connectTask = client.ConnectAsync(host, port);
                if (await Task.WhenAny(connectTask, Task.Delay(3000)) == connectTask) // 3-second timeout
                {
                    await connectTask; // Await to observe potential exceptions
                    Console.WriteLine($"Connection to {host}:{port} successful.");
                    // We could optionally send a benign LPR command here (like queue state request)
                    // but just establishing the TCP connection is often enough for a basic check.
                    client.Close(); // Close immediately after successful connect
                    return (true, null); // Success
                }
                else
                {
                    client.Close(); // Ensure closed on timeout
                    Console.WriteLine($"Connection to {host}:{port} timed out.");
                    return (false, "Connection timed out.");
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Connection check to {host}:{port} failed: {ex.Message} (Code: {ex.SocketErrorCode})");
                return (false, $"Connection failed: {ex.Message} (Code: {ex.SocketErrorCode})");
            }
            catch (Exception ex) // Catch other unexpected errors
            {
                 Console.WriteLine($"Connection check to {host}:{port} failed: {ex.Message}");
                 return (false, $"Connection failed: {ex.Message}");
            }
        }


        // Helper to send command
        private async Task SendCommandAsync(NetworkStream stream, string command)
        {
            byte[] commandBytes = Encoding.ASCII.GetBytes(command);
            await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
            await stream.FlushAsync();
        }

        // Helper to wait for ACK, throws LprAckException on failure
        private async Task WaitForAckAsync(NetworkStream stream, string afterOperation)
        {
            byte[] ackBuffer = new byte[1];
            // Use ReadAsync with cancellation token potentially, or rely on stream.ReadTimeout
            int bytesRead = await stream.ReadAsync(ackBuffer, 0, 1);

            if (bytesRead == 0)
            {
                throw new LprCommunicationException($"Connection closed by server while waiting for ACK after: {afterOperation}");
            }

            if (ackBuffer[0] == 0x00)
            {
                Console.WriteLine($"--> OK: Received ACK (0x00) after: {afterOperation}");
            }
            else
            {
                // Failure!
                throw new LprAckException($"Error: Received NACK (0x{ackBuffer[0]:X2}) instead of ACK after: {afterOperation}", ackBuffer[0]);
            }
        }
    }

    // --- Custom Exception Classes ---
    public class LprCommunicationException : Exception
    {
        public LprCommunicationException(string message) : base(message) { }
        public LprCommunicationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class LprAckException : LprCommunicationException
    {
        public byte ReceivedByte { get; }
        public LprAckException(string message, byte receivedByte) : base(message)
        {
            ReceivedByte = receivedByte;
        }
    }
}