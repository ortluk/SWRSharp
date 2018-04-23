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
            var ClientList = new List<Client>();

            Console.WriteLine("Listening...");
            var server = new TcpListener(IPAddress.Any, 4000);
            server.Start();
            while (true)
            {
                if (server.Pending())
                {
                    var connection = new Client(server.AcceptTcpClient());
                    connection.Send("Welcome Message.\r\n");
                    Console.WriteLine("connection accepted.");
                    ClientList.Add(connection);
                    connection.Send("What name do you wish to be known by? ");
                    connection.State = Client.ConnectionState.CON_GET_NAME;
                }

                Client client;
                for (var i = ClientList.Count - 1; i > -1; i--)
                {
                    client = ClientList[i];
                    if (!client.AttemptRead())
                    {
                        Console.WriteLine("Client Disconnected...");
                        ClientList.RemoveAt(i);
                        client.Close();
                    }

                    if (client.IsCommandPending())
                    {
                        var Command = client.GetCommand();
                        switch (client.State)
                        {
                            case Client.ConnectionState.CON_GET_NAME:
                                client.Send("Hello " + Command);
                                client.Send("Please enter Password: ");
                                client.State = Client.ConnectionState.CON_GET_PASSWORD;
                                break;
                            case Client.ConnectionState.CON_GET_PASSWORD:
                                client.Send("Password Accepted\r\n");
                                client.Send("Enter Command > ");
                                client.State = Client.ConnectionState.CON_PLAYING;
                                break;
                            case Client.ConnectionState.CON_PLAYING:
                                switch (Command)
                                {
                                    case "Quit\r\n":
                                        Console.WriteLine("Client Disconnected...");
                                        ClientList.RemoveAt(i);
                                        client.Close();
                                        break;
                                    case "Shutdown\r\n":
                                        server.Stop();
                                        for (var x = ClientList.Count - 1; x > -1; x--)
                                        {
                                            Console.WriteLine("Client Disconnected...");
                                            ClientList.RemoveAt(x);
                                            client.Close();
                                        }

                                        Console.WriteLine("Server Stopped");
                                        return;
                                    default:
                                        client.Send(Command);
                                        client.Send("Enter Command > ");
                                        client.State = Client.ConnectionState.CON_PLAYING;
                                        break;
                                }
                                break;
                            default:
                                Console.WriteLine("Invalid connection state!");
                                client.State = Client.ConnectionState.CON_PLAYING;
                                break;
                        }
                    }
                }
            }
        }
    }
}