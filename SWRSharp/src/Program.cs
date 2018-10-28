using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;

namespace SWRSharp
{
    class Globals
    {
        public static Dictionary<string, Area> AreaList;
        public static List<Client> clientList;
        public static TcpListener server;
        public static bool MudDown;
        public static void Initialize()
        {
            AreaList = new Dictionary<string, Area>();
            clientList = new List<Client>();
            server = new TcpListener(IPAddress.Any, 4000);
            MudDown = false;
        }

        public static void BootDB()
        {
            var fname = "./World/Area.lst";
            string[] areafiles = File.ReadAllLines(fname);

            foreach (string s in areafiles)
            {
                string afname = "./World/" + s;
                Area newarea = new Area();
                newarea.set_filename(afname);
                newarea.Load();
                AreaList.Add(newarea.get_name(), newarea);
            }

        }
    }
     
    internal class Program
    {
        private static void Main(string[] args)
        {
            
            var greeting = System.IO.File.ReadAllText("./config/greeting.txt");
            Client.InitializeColors();
            Command_Interpreter.Initialize();
            Globals.Initialize();
            Globals.BootDB();
            Console.WriteLine("Listening...");
            Globals.server.Start();
            while (true)
            {
                if (Globals.server.Pending())
                {
                    var connection = new Client(Globals.server.AcceptTcpClient());
                    connection.Send(greeting);
                    Console.WriteLine("connection accepted.");
                    Globals.clientList.Add(connection);
                    connection.Send("\r\n\r\nWhat name do you wish to be known by? ");
                    connection.State = Client.ConnectionState.ConGetName;
                }

                for (var i = Globals.clientList.Count - 1; i > -1; i--)
                {
                    var client = Globals.clientList[i];
                    if (!client.AttemptRead())
                    {
                        Console.WriteLine("Client Disconnected...");
                        Globals.clientList.RemoveAt(i);
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
                            sock.set_Character(new Character());
                            if (sock.get_character().load_Character(command))
                            {
                                sock.Send("Please enter Password: ");
                                sock.State = Client.ConnectionState.ConGetPassword;
                            }
                            else
                            {
                                sock.get_character().set_name(command);
                                sock.Send("What password will you use? ");
                                sock.State = Client.ConnectionState.ConGetNewPassword;
                            }
                            break;
                        case Client.ConnectionState.ConGetNewPassword:
                            sock.get_character().set_password(command);
                            sock.Send("Retype Password: ");
                            sock.State = Client.ConnectionState.ConConfirmPassword;
                            break;
                        case Client.ConnectionState.ConConfirmPassword:
                            if (command == sock.get_character().get_password())
                            {
                                sock.get_character().save_Character();
                                sock.Send("Welcome, " + sock.get_character().get_name() + "!\r\n");
                                sock.Send("Enter Command > ");
                                sock.State = Client.ConnectionState.ConPlaying;
                            }
                            else
                            {
                                sock.Send("Passwords do not match. Enter a new Password: ");
                                sock.State = Client.ConnectionState.ConGetNewPassword;
                            }
                            break;
                        case Client.ConnectionState.ConGetPassword:
                            if (command == sock.get_character().get_password())
                            {
                                Area toarea;
                                
                                Globals.AreaList.TryGetValue("Limbo", out toarea);
                                sock.Send("Password Accepted\r\n");
                                sock.get_character().to_room(toarea.Get_Room(0));
                                sock.Send("Enter Command > ");
                                sock.State = Client.ConnectionState.ConPlaying;
                            }
                            else
                            {
                                sock.Send("Login Failure\r\n");
                                Console.WriteLine("Login Failure - " + sock.get_character().get_name());
                                sock.set_Character(null);
                                sock.Send("Enter Name: ");
                                sock.State = Client.ConnectionState.ConGetName;
                            }
                            break;
                        case Client.ConnectionState.ConPlaying:
                            if (!Command_Interpreter.Interpret(command, sock.get_character()))
                                sock.Send("Huh?!?\r\n");
                            
                            sock.Send("Enter Command > ");
                            sock.State = Client.ConnectionState.ConPlaying;
                            break;
                        default:
                            Console.WriteLine("Invalid connection state!");
                            sock.State = Client.ConnectionState.ConPlaying;
                            break;
                    }

                if (Globals.MudDown)
                    return false;
                
                return true;
            }
        }
    }
}