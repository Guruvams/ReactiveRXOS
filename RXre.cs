using Fleck;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace PubSubRxServer
{
    public class WithOutChannel
    {
        static List<IWebSocketConnection> connectedClients = new List<IWebSocketConnection>();
        static void Main(string[] args)
        {
            Console.WriteLine("Starting WebSocket Server...");

            var server = new WebSocketServer("ws://0.0.0.0:8181");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Client connected");
                    connectedClients.Add(socket);
                };

                socket.OnClose = () =>
                {
                    Console.WriteLine("Client disconnected");
                    connectedClients.Remove(socket);
                };

                socket.OnMessage = (msg) =>
                {
                    Console.WriteLine("Client says: " + msg);
                };
            });

            var random = new Random();

            // ReactiveX Observable → Publish Stock Price Every 1 Second
            var priceStream = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(_ => Math.Round(100 + random.NextDouble() * 20, 2));


            // Broadcast to ALL connected clients
            priceStream.Subscribe(price =>
            {
                Console.WriteLine("Publishing Price: " + price);

                foreach (var client in connectedClients)
                {
                    try
                    {
                        client.Send(price.ToString());
                    }
                    catch { }
                }
            });

            Console.WriteLine("Server started on ws://localhost:8181");
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
        }
    }
}
