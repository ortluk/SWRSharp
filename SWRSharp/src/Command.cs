using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;

namespace SWRSharp
{
    public class Command
    {
        private static Dictionary<string, Action<string, Character>> DictionaryofFunctions;
            
        private string _name;
        private string _function_name;
        private int _level;
        
        public string get_name()
        {
            return _name;
        }

        public void set_name(string name)
        {
            _name = name;
        }

        public string get_function_name()
        {
            return _function_name;
        }

        public void set_function_name(string function_name)
        {
            _function_name = function_name;
        }
        
        public int get_Level()
        {
            return _level;
        }

        public void set_Level(int lvl)
        {
            _level = lvl;
        }

        public void set_Invoke()
        {
             DictionaryofFunctions.TryGetValue(_function_name, out Invoke);
            
        }

        public static void Initialize()
        {
            DictionaryofFunctions = new Dictionary<string, Action<string, Character>>
            {
                {"do_move", do_move},
                {"do_look", do_look},
                {"do_quit", do_quit}, 
                {"do_shutdown", do_shutdown}
            };
        }
        public Action<string, Character> Invoke;

        private static void do_move(string commandline, Character ch)
        {
            string[] args = commandline.Split();
            Exit xit;
            Room inroom = ch.get_room();
            switch (args[0].ToLower())
            {
                case "north":
                case "n":
                    xit = inroom.Get_Exit("north");
                    break;
                
                case "south":
                case "s":
                    xit = inroom.Get_Exit("south");
                    break;
                case "east":
                case "e":
                    xit = inroom.Get_Exit("east");
                    break;
                case "west":
                case "w":
                    xit = inroom.Get_Exit("west");
                    break;
                case "northeast":
                case "ne":
                    xit = inroom.Get_Exit("northeast");
                    break;
                case "northwest":
                case "nw":
                    xit = inroom.Get_Exit("northwest");
                    break;
                case "southeast":
                case "se":
                    xit = inroom.Get_Exit("southeast");
                    break;
                case "southwest":
                case "sw":
                    xit = inroom.Get_Exit("southwest");
                    break;
                case "up":
                case "u":
                    xit = inroom.Get_Exit("up");
                    break;
                case "down":
                case "d":
                    xit = inroom.Get_Exit("down");
                    break;
                default:
                    xit = inroom.Get_Exit(args[0]);
                    if (xit == null)
                    {
                        ch.Send("Huh?!?\r\n");
                        return;
                    }
                    break;
            }

            if (xit == null)
            {
                ch.Send("Alas, you cannot go that way. \r\n");
                return;
            }
            ch.to_room(xit.Get_to_room());
        }
        private static void do_look(string commandline, Character ch)
        {
            Room inroom;
            string buffer;
            inroom = ch.get_room();
            buffer = inroom.Get_Name() + "\r\n";
            buffer += inroom.Get_Description() + "\r\n";
            buffer += inroom.List_Exits() + "\r\n";
            buffer += inroom.List_Occupants() + "\r\n";
            ch.Send(buffer);
        }
        private static void do_shutdown(string commandline, Character ch)
        {
            string[] args = commandline.Split();
            if (args[0].ToLower() != "shutdown")
            {
                ch.Send("&RUse shutdown to shutdown mud\r\n&n");
                return;
            }
            Globals.server.Stop();
            for (var x = Globals.clientList.Count - 1; x > -1; x--)
            {
                Console.WriteLine("Client Disconnected...");
                Globals.clientList[x].Close();
                Globals.clientList.RemoveAt(x);
            }

            Globals.MudDown = true;
            Console.WriteLine("Server Stopped");
        }
        private static void do_quit(string commandline, Character ch)
        {
            Console.WriteLine("Client Disconnected...");
            for (var x = Globals.clientList.Count - 1; x > -1; x--)
            {
                if (ch.get_client() != Globals.clientList[x]) 
                    continue;
                
                ch.get_client().Close();
                Globals.clientList.RemoveAt(x);
            }
        }
    }

}