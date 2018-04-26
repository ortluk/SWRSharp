using System;
using System.IO;

namespace SWRSharp
{
    public class Character
    {
        private string _name;
        private string _password;
        private Client _client;

        public void set_client(Client sock)
        {
            _client = sock;
        }

        public Client get_client()
        {
            return _client;
        }
        public void set_name(string Name)
        {
            _name = Name;
        }

        public string get_name()
        {
            return _name;
        }

        public void set_password(string pass)
        {
            _password = pass;
        }

        public string get_password()
        {
            return _password;
        }

        public bool load_Character(string Name)
        {
            var fname = "./player/" + Name.ToLower();
            try
            {
                var fstream = File.OpenText(fname);
                set_name(fstream.ReadLine());
                set_password(fstream.ReadLine());
                
                fstream.Close();
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            return true;
        }

        public void save_Character()
        {
            var fname = "./player/" + _name.ToLower();

            var fstream = File.CreateText(fname);
            fstream.WriteLine(_name);
            fstream.WriteLine(_password);
            fstream.Close();
        }
    }
}