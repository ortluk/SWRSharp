using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Net;
using System.IO;


namespace SWRSharp
{
    class Program
    {
        
        static void Main(string[] args)
        {
            List<Client> ClientList = new List<Client>();
            
            Console.WriteLine("Listening...");
            TcpListener server = new TcpListener(IPAddress.Any, 4000);
            server.Start();
            while (true)
            {
                if (server.Pending())
                {
                    Client connection = new Client( server.AcceptTcpClient());
                    connection.Send("Welcome Message.\r\n");
                    Console.WriteLine("connection accepted.");
                    ClientList.Add(connection);
                }

                Client client;
                for (int i = ClientList.Count - 1; i > -1; i--)
                {
                    client = ClientList[i];
                    if (!client.AttemptRead())
                    {
                        Console.WriteLine("Client Disconnected...");
                        ClientList.RemoveAt(i);
                        client.Close();
                    }

                    if (client.isCommandPending())
                    {
                        String Command = client.GetCommand();

                        switch (Command)
                        {
                            case "Quit\r\n":
                                Console.WriteLine("Client Disconnected...");
                                ClientList.RemoveAt(i);
                                client.Close();
                                break;
                            case "Shutdown\r\n":
                                server.Stop();
                                for (int x = ClientList.Count - 1; x > -1; x--)
                                {
                                    Console.WriteLine("Client Disconnected...");
                                    ClientList.RemoveAt(x);
                                    client.Close();
                                }
                                Console.WriteLine("Server Stopped");
                                return;
                            default:
                                client.Send(Command);
                                break;
                        }
                    }
                }
            }
        }
    }
}
