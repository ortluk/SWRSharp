using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;

namespace SWRSharp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var clientList = new List<Client>();
            var greeting = System.IO.File.ReadAllText("./config/greeting.txt");
            Client.InitializeColors();
            
            Console.WriteLine("Listening...");
            var server = new TcpListener(IPAddress.Any, 4000);
            server.Start();
            while (true)
            {
                if (server.Pending())
                {
                    var connection = new Client(server.AcceptTcpClient());
                    connection.Send(greeting);
                    Console.WriteLine("connection accepted.");
                    clientList.Add(connection);
                    connection.Send("\r\n\r\nWhat name do you wish to be known by? ");
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

                    if (!client.IsCommandPending()) 
                        continue;

                    if (!nanny(client, client.GetCommand(), i))
                        return;
                }
            }

            bool nanny(Client sock, string line, int position)
            {
                    var command = line;
                    switch (sock.State)
                    {
                        case Client.ConnectionState.ConGetName:
                            sock.Send("Hello " + command);
                            sock.Send("Please enter Password: ");
                            sock.State = Client.ConnectionState.ConGetPassword;
                            break;
                        case Client.ConnectionState.ConConfirmPassword:
                            break;
                        case Client.ConnectionState.ConGetPassword:
                            sock.Send("Password Accepted\r\n");
                            sock.Send("Enter Command > ");
                            sock.State = Client.ConnectionState.ConPlaying;
                            break;
                        case Client.ConnectionState.ConPlaying:
                            switch (command)
                            {
                                case "Quit\r\n":
                                    Console.WriteLine("Client Disconnected...");
                                    clientList.RemoveAt(position);
                                    sock.Close();
                                    break;
                                case "Shutdown\r\n":
                                    server.Stop();
                                    for (var x = clientList.Count - 1; x > -1; x--)
                                    {
                                        Console.WriteLine("Client Disconnected...");
                                        clientList.RemoveAt(x);
                                        sock.Close();
                                    }

                                    Console.WriteLine("Server Stopped");
                                    return false;
                                default:
                                    sock.Send(command);
                                    sock.Send("Enter Command > ");
                                    sock.State = Client.ConnectionState.ConPlaying;
                                    break;
                            }
                            break;
                        default:
                            Console.WriteLine("Invalid connection state!");
                            sock.State = Client.ConnectionState.ConPlaying;
                            break;
                    }

                return true;
            }
        }
    }
}