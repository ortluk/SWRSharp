using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SWRSharp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var clientList = new List<Client>();
            Client.InitializeColors();
            
            Console.WriteLine("Listening...");
            var server = new TcpListener(IPAddress.Any, 4000);
            server.Start();
            while (true)
            {
                if (server.Pending())
                {
                    var connection = new Client(server.AcceptTcpClient());
                    connection.Send("&rWelcome&n Message.\r\n");
                    Console.WriteLine("connection accepted.");
                    clientList.Add(connection);
                    connection.Send("What name do you wish to be known by? ");
                    connection.State = Client.ConnectionState.ConGetName;
                }

                for (var i = clientList.Count - 1; i > -1; i--)
                {
                    var client = clientList[i];
                    if (!client.AttemptRead())
                    {
                        Console.WriteLine("Client Disconnected...");
                        clientList.RemoveAt(i);
                        client.Close();
                    }

                    if (!client.IsCommandPending()) continue;
                    var command = client.GetCommand();
                    switch (client.State)
                    {
                        case Client.ConnectionState.ConGetName:
                            client.Send("Hello " + command);
                            client.Send("Please enter Password: ");
                            client.State = Client.ConnectionState.ConGetPassword;
                            break;
                        case Client.ConnectionState.ConConfirmPassword:
                            break;
                        case Client.ConnectionState.ConGetPassword:
                            client.Send("Password Accepted\r\n");
                            client.Send("Enter Command > ");
                            client.State = Client.ConnectionState.ConPlaying;
                            break;
                        case Client.ConnectionState.ConPlaying:
                            switch (command)
                            {
                                case "Quit\r\n":
                                    Console.WriteLine("Client Disconnected...");
                                    clientList.RemoveAt(i);
                                    client.Close();
                                    break;
                                case "Shutdown\r\n":
                                    server.Stop();
                                    for (var x = clientList.Count - 1; x > -1; x--)
                                    {
                                        Console.WriteLine("Client Disconnected...");
                                        clientList.RemoveAt(x);
                                        client.Close();
                                    }

                                    Console.WriteLine("Server Stopped");
                                    return;
                                default:
                                    client.Send(command);
                                    client.Send("Enter Command > ");
                                    client.State = Client.ConnectionState.ConPlaying;
                                    break;
                            }
                            break;
                        default:
                            Console.WriteLine("Invalid connection state!");
                            client.State = Client.ConnectionState.ConPlaying;
                            break;
                    }
                }
            }
        }
    }
}