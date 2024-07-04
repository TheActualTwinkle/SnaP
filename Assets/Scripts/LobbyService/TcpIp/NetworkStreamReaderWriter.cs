using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LobbyService.TcpIp.Interfaces;

namespace LobbyService.TcpIp
{
    public class NetworkStreamReaderWriter
    {
        public static async Task<string> ReadAsync(NetworkStream stream)
        {
            // Start with a reasonable initial buffer size
            const int initialBufferSize = 1024;
    
            // Initialize a MemoryStream to store the incoming data
            using MemoryStream memoryStream = new();
        
            // Create a temporary buffer for reading data
            var buffer = new byte[initialBufferSize];
            int bytesRead;
        
            // Loop until the end of the message is reached
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                // Write the read bytes to the memory stream
                memoryStream.Write(buffer, 0, bytesRead);
            
                // If there's more data available, resize the buffer and continue reading
                if (stream.DataAvailable)
                {
                    Array.Resize(ref buffer, buffer.Length * 2);
                }
                else
                {
                    // No more data available, break out of the loop
                    break;
                }
            }
        
            // Convert the accumulated bytes to a string using ASCII encoding
            return Encoding.ASCII.GetString(memoryStream.ToArray());
        }
        
        public static async Task WriteAsync(NetworkStream stream, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            
            await stream.WriteAsync(data, 0, data.Length);
            Logger.Log($"Send {message}.", Logger.LogSource.LobbyService);
        }
    }
}