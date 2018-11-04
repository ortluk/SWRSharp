using System;
using System.Collections.Generic;
using System.IO;

namespace SWRSharp
{
    public class Command_Interpreter
    {
        private static Dictionary<string, Command > CommandList;

        public static void Initialize()
        {
            var fname = "./config/command.dat";
          
            string[] commands = File.ReadAllLines(fname);
            CommandList = new Dictionary<string, Command>();
            Command.Initialize();
            foreach (string s in commands)
            {
                string[] values = s.Split();
                Command com = new Command();
                int lvl;
                com.set_name(values[0].ToLower());
                com.set_function_name(values[1]);
                Int32.TryParse(values[2], out lvl);
                com.set_Level(lvl);
                com.set_Invoke();
                CommandList.Add(com.get_name(), com);
            }

            
        }

        public static bool Interpret(string commandline, Character ch)
        {
            bool found;
            found = false;
            string[] args = commandline.Split();
            foreach (KeyValuePair<string, Command> kvp in CommandList)
            {
                found = kvp.Key.StartsWith(args[0].ToLower());
                if (found)
                {
                    kvp.Value.Invoke(commandline, ch);
                    return found;
                }
            }
            
            return found;
            
        }
    }
}