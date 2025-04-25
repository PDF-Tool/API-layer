using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


public class LprClient
{
    private const int LprPort = 515;

    private readonly string _serverHost;
    private readonly string _queueName;
    private readonly string _pdfFilePath;

    public LprClient(string serverHost, string queueName, string pdfFilePath)
    {
        _serverHost = serverHost;
        _queueName = queueName;
        _pdfFilePath = pdfFilePath;
    }

    public async Task SendPrintJobAsync()
    {
        if (!File.Exists(_pdfFilePath))
        {
            Console.WriteLine($"Error: PDF file not found at '{_pdfFilePath}'");
            return;
        }

        // --- 1. Connect to the Server ---
        Console.WriteLine($"Connecting to LPR server at {_serverHost}:{LprPort}...");
        using TcpClient client = new TcpClient();
        try
        {
            await client.ConnectAsync(_serverHost, LprPort);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return;
        }

        using NetworkStream stream = client.GetStream();
        Console.WriteLine("Connected successfully.");

        try 
        {
            // --- LPR Protocol Steps ---

            // Basic job details (needed for control file)
            string hostName = Dns.GetHostName(); // Get local computer name
            string userName = Environment.UserName; // Get current user name
            int jobId = new Random().Next(1, 1000); // Simple random job ID (1-999)
            string controlFileName = $"cfA{jobId:D3}{hostName}"; // e.g., cfA001MyPC
            string dataFileName = $"dfA{jobId:D3}{hostName}";    // e.g., dfA001MyPC
            string sourceFileName = Path.GetFileName(_pdfFilePath); // Original PDF filename

            Console.WriteLine($"Preparing job: ID={jobId}, ControlFile={controlFileName}, DataFile={dataFileName}");

            // --- Step 2: Send "Receive Printer Job" command (Code 0x02) ---
            Console.WriteLine($"Sending 'Receive Job' command for queue '{_queueName}'...");
            await SendCommandAsync(stream, $"\x02{_queueName}\n"); // \x02 = command code 2

            // --- Step 3: Wait for Acknowledgement (ACK = 0x00) ---
            await WaitForAckAsync(stream, "Receive Job command");

            // --- Step 4a: Prepare Control File Content ---
            // Tells the server who sent the job (H, P) and what to do with the data file (l = print raw)
            string controlFileContent = $"H{hostName}\n" + // Host sending the job
                                        $"P{userName}\n" + // User sending the job
                                        $"N{sourceFileName}\n" + // Original file name (optional info)
                                        $"l{dataFileName}\n"; // Print data file (dfA...) raw/literally
            byte[] controlFileBytes = Encoding.ASCII.GetBytes(controlFileContent);

            // --- Step 4b: Send "Receive Control File" subcommand (Code 0x02) ---
            // Format: \x02<byte count> <control file name>\n
            Console.WriteLine($"Sending 'Receive Control File' subcommand ({controlFileBytes.Length} bytes, name: {controlFileName})...");
            await SendCommandAsync(stream, $"\x02{controlFileBytes.Length} {controlFileName}\n");

            // --- Step 5: Wait for ACK ---
            await WaitForAckAsync(stream, "Receive Control File subcommand");

            // --- Step 6: Send the actual Control File data ---
            Console.WriteLine("Sending control file data...");
            await stream.WriteAsync(controlFileBytes, 0, controlFileBytes.Length);

            // --- Step 7: Send Control File Completion Byte (0x00) ---
            Console.WriteLine("Sending control file completion byte (0x00)...");
            await stream.WriteAsync(new byte[] { 0x00 }, 0, 1);
            await stream.FlushAsync(); // Ensure data is sent

            // --- Step 8: Wait for ACK ---
            await WaitForAckAsync(stream, "Control File data");

            // --- Step 9a: Read the PDF file into memory ---
            Console.WriteLine($"Reading PDF file '{_pdfFilePath}'...");
            byte[] pdfDataBytes = await File.ReadAllBytesAsync(_pdfFilePath);

            // --- Step 9b: Send "Receive Data File" subcommand (Code 0x03) ---
            // Format: \x03<byte count> <data file name>\n
            Console.WriteLine($"Sending 'Receive Data File' subcommand ({pdfDataBytes.Length} bytes, name: {dataFileName})...");
            await SendCommandAsync(stream, $"\x03{pdfDataBytes.Length} {dataFileName}\n");

            // --- Step 10: Wait for ACK ---
            await WaitForAckAsync(stream, "Receive Data File subcommand");

            // --- Step 11: Send the actual PDF File data ---
            Console.WriteLine("Sending PDF data...");

            // For very large files, sending the actual PDF Data in chunks would be better for memory
            await stream.WriteAsync(pdfDataBytes, 0, pdfDataBytes.Length);

            // --- Step 12: Send Data File Completion Byte (0x00) ---
            Console.WriteLine("Sending PDF data completion byte (0x00)...");
            await stream.WriteAsync(new byte[] { 0x00 }, 0, 1);
            await stream.FlushAsync(); // Ensure data is sent

            // --- Step 13: Wait for final ACK ---
            await WaitForAckAsync(stream, "PDF data");

            Console.WriteLine("\nPrint job sent successfully!");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n--- An error occurred during LPR communication ---");
            Console.WriteLine(ex.Message);
        }

        // --- Step 14: Close Connection ---
        Console.WriteLine("Connection closed.");
    }

    // Method to send a command
    private async Task SendCommandAsync(NetworkStream stream, string command)
    {
        byte[] commandBytes = Encoding.ASCII.GetBytes(command);
        await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
        await stream.FlushAsync(); 
    }

    // Method to wait for and check the ACK byte from the server (0x00)
    private async Task WaitForAckAsync(NetworkStream stream, string afterOperation)
    {
        byte[] ackBuffer = new byte[1];

        int bytesRead = await stream.ReadAsync(ackBuffer, 0, 1);

        if (bytesRead == 0)
        {
            // Server closed connection unexpectedly
            throw new IOException($"Connection closed by server while waiting for ACK after: {afterOperation}");
        }

        if (ackBuffer[0] == 0x00)
        {
            // Success!
            Console.WriteLine($"--> OK: Received ACK (0x00) after: {afterOperation}");
        }
        else
        {
            // Failure! Server sent something other than ACK
            throw new Exception($"Error: Received NACK (0x{ackBuffer[0]:X2}) instead of ACK after: {afterOperation}");
        }
    }

    /* example config and lprclient object, that sends a print job
    string server = "localhost";
    string queue = "myqueue";
    string pdfPath = "test.pdf"; 

    LprClient lprClient = new LprClient(server, queue, pdfPath);
    await lprClient.SendPrintJobAsync();
    } */
}