using System;
using System.Collections.Generic;
using System.IO;

namespace SWRSharp
{
    public class Area
    {
        private string name;
        private string filename;
        private Dictionary<int, Room> RoomList;

        public void set_name(string nam)
        {
            name = nam;
        }

        public void set_filename(string fname)
        {
            filename = fname;
        }

        public string get_name()
        {
            return name;
        }

        public string get_filename()
        {
            return filename;
        }

        public Room Get_Room(int vnum)
        {
            Room temproom;
            RoomList.TryGetValue(vnum, out temproom);
            return temproom;
        }
        public void Load()
        {
            RoomList = new Dictionary<int, Room>();
            
            using (StreamReader sr = File.OpenText(filename))
            {
                string line = "";
                
                while ((line = sr.ReadLine()) != null)
                {
                    string[] words = line.Split();
                    switch (words[0])
                    {
                        case "AREA":
                            name = words[1];
                            break;
                        case "ROOM":
                            Room newroom = new Room();
                            newroom.Load_Room(sr);
                            RoomList.Add(newroom.Get_Vnum(), newroom);
                            break;
                        default:
                            Console.WriteLine("Invalid section in Area File: {0}", filename);
                            break;
                    }
                }
            }
        }
    }
}