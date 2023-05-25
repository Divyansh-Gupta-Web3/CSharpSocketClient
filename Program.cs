using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socketclient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new ClientWebSocket();

            try
            {
                // Connect to the WebSocket server
                await client.ConnectAsync(new Uri("ws://localhost:3000"), CancellationToken.None);
                Console.WriteLine("Connected to server.");

                // Receive messages from the server in a separate task
                _ = ReceiveMessage(client);

                // Continuously send "hello" to the server until the socket is closed
                while (client.State == WebSocketState.Open)
                {
                    //request the GET API to get the data and the api will return the data in JSON format api = 'http://api.c-sharpcorner.com/api/Certificate'
                    using (var api = new HttpClient())
                    {
                        // Add an Accept header for JSON format.
                        api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = await api.GetAsync("http://api.c-sharpcorner.com/api/Certificate");
                        //read the response and decode the data
                        var result = await response.Content.ReadAsStringAsync();

                        byte[] messageBytes = Encoding.UTF8.GetBytes(result);

                        // Send the message to the server
                        await client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        //Console.WriteLine("Sent: SENT");

                        // Wait for a short period before sending the next message
                        await Task.Delay(1000);
                    }
                }

                // WebSocket connection has been closed
                Console.WriteLine("WebSocket connection closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // Close the WebSocket connection
                if (client.State == WebSocketState.Open || client.State == WebSocketState.CloseSent)
                    await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

                client.Dispose();
            }

            Console.ReadLine();
        }

        static async Task ReceiveMessage(ClientWebSocket client)
        {
            byte[] buffer = new byte[1024];

            while (client.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Received message from server: " + message);
                }
            }
        }
    }
}
