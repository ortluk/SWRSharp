﻿using System;
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
                com.set_name(values[0]);
                com.set_function_name(values[1]);
                Int32.TryParse(values[2], out lvl);
                com.set_Level(lvl);
                com.set_Invoke();
                CommandList.Add(com.get_name(), com);
            }

            
        }

        public static bool Interpret(string commandline, Character ch)
        {
            Command command;
            bool found;
            string[] args = commandline.Split();
            found = CommandList.TryGetValue(args[0], out command);
            if (found)
            {
                command.Invoke(commandline, ch);
            }

            return found;
        }
    }
}