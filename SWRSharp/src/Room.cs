using System;
using System.Collections.Generic;
using System.IO;

namespace SWRSharp
{
    public class Room
    {
        private int vnum;
        private string name;
        private string description;
        private Dictionary<string, Character> Occupants;
        private Dictionary<string, Exit> Exits;

        public Dictionary<string, Exit> Get_Exits()
        {
            return Exits;
        }

        public Exit Get_Exit(string exname)
        {
            Exit xit;
            if (!Exits.TryGetValue(exname, out xit))
            {
                return null;
            }
            else
            {
                return xit;
            }

        }
        public void char_to_room(Character ch)
        {
            Occupants.Add(ch.get_name(), ch);
        }

        public void char_from_room(Character ch)
        {
            if (Occupants.ContainsKey(ch.get_name()))
                Occupants.Remove(ch.get_name());
        }
        
        public int Get_Vnum()
        {
            return vnum;
        }

        public void Set_Vnum(int num)
        {
            vnum = num;
        }

        public string Get_Name()
        {
            return name;
        }

        public string Get_Description()
        {
            return description;
        }

        public string List_Occupants()
        {
            string buffer = "Occupants:\r\n";
            foreach (var ch in Occupants.Keys)
            {
                buffer += ch + "\r\n";
            }

            return buffer;
        }

        public string List_Exits()
        {
            string buffer = "Exits:\r\n";
            foreach (var xit in Exits.Values)
            {
                buffer += xit.Get_Names() + "-" + xit.Get_to_room().Get_Name() + "\r\n";
            }

            return buffer;
        }
        public void Load_Room(StreamReader sr)
        {
            string line = sr.ReadLine();
            vnum = Int32.Parse(line);
            name = sr.ReadLine();
            bool endofblock = false;
            while (!endofblock)
            {
                line = sr.ReadLine();
                switch (line)
                {
                    case "~":
                        endofblock = true;
                        break;
                    default:
                        description += line + "\r\n";
                        break;
                }                
            }

            line = sr.ReadLine();
            switch (line)
            {
                    case "EXIT":
                        if (Exits == null)
                          Exits = new Dictionary<string, Exit>();    
                        Exit newexit = new Exit();
                        newexit.Load_Exit(sr);
                        string[] names = newexit.Get_Names().Split();
                        foreach (string s in names)
                        {
                            Exits.Add(s.ToLower(), newexit);
                        }

                        break;
                    
                    case "End":
                        break;
            }
            Occupants = new Dictionary<string, Character>();
        }

        public void Write_Room(StreamWriter sw)
        {
            sw.WriteLine(vnum);
            sw.WriteLine(name);
            sw.Write(description.ToCharArray(),0,description.Length);
            sw.WriteLine("~");
            foreach (var xit in Exits)
            {
                sw.WriteLine("EXIT");
                xit.Value.Write_Exit(sw);
            }
            sw.WriteLine("End");
        }
    }
}
