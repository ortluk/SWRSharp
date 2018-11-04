using System;
using System.IO;

namespace SWRSharp
{
    public class Exit
    {
        private string names;
        private string tarea;
        private int troom;
        private Area to_area;
        private Room to_room;

        public string Get_Names()
        {
            return names;
        }

        public Area Get_to_area()
        {
            return to_area;
        }

        public Room Get_to_room()
        {
            return to_room;
        }
        public void Load_Exit(StreamReader sr)
        {
            string line = sr.ReadLine();
            names = line;
            line = sr.ReadLine();
            tarea = line;
            line = sr.ReadLine();
            troom = Int32.Parse(line);
        }

        public void Write_Exit(StreamWriter sw)
        {
            sw.WriteLine(names);
            sw.WriteLine(tarea);
            sw.WriteLine(troom);
        }
        public void Connect_Exit()
        {
            Globals.AreaList.TryGetValue(tarea, out to_area);
            to_room = to_area.Get_Room(troom);
        }
    }
}