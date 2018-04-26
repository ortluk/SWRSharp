using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SWRSharp
{
    public class Client
    {
        private readonly NetworkStream _clientStream;
        private string _command;
        private bool _commandPending;
        private readonly StringBuilder _inBuffer;
        private readonly TcpClient _tcpClient;
        public ConnectionState State { get; set; }
        private static Dictionary<string, string> _colorTable;

        public static void InitializeColors()
        {
            _colorTable = new Dictionary<string, string>
            {
                {"&n", "\u001b[0m"},
                {"&r", "\u001b[31m"},
                {"&g", "\u001b[32m"},
                {"&y", "\u001b[33m"},
                {"&b", "\u001b[34m"},
                {"&m", "\u001b[35m"},
                {"&c", "\u001b[36m"},
                {"&w", "\u001b[37m"},
                {"&R", "\u001b[31;1m"},
                {"&G", "\u001b[32;1m"},
                {"&Y", "\u001b[33;1m"},
                {"&B", "\u001b[34;1m"},
                {"&M", "\u001b[35;1m"},
                {"&C", "\u001b[36;1m"},
                {"&W", "\u001b[37;1m"}
            };
        }
        
        public enum  ConnectionState
        {
            ConGetName,
            ConGetPassword,
            ConConfirmPassword,
            ConPlaying
        }
        
        public Client(TcpClient inClient)
        {
            _tcpClient = inClient;
            _clientStream = _tcpClient.GetStream();
            _inBuffer = new StringBuilder();
            _commandPending = false;
        }

        public void Send(string msg)
        {
            foreach (var colorPair in _colorTable)
            {
                msg = msg.Replace(colorPair.Key, colorPair.Value);
            }

            if (!_clientStream.CanWrite) return;
            var buffer = Encoding.UTF8.GetBytes(msg);
            _clientStream.Write(buffer, 0, buffer.Length);
        }

        public string GetCommand()
        {
            var returnValue = string.Copy(_command);
            _command = "";
            _commandPending = false;
            return returnValue;
        }

        public bool IsCommandPending()
        {
            return _commandPending;
        }

        public bool AttemptRead()
        {
            if (!_clientStream.CanRead || !_tcpClient.Client.Poll(1, SelectMode.SelectRead)) return true;
            var readBuffer = new byte[1024];
            do
            {
                try
                {
                    int bytesread;
                    if ((bytesread = _clientStream.Read(readBuffer, 0, readBuffer.Length)) != 0)
                    {
                        _inBuffer.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, bytesread));
                        if (readBuffer[bytesread - 1] != '\r' && readBuffer[bytesread - 1] != '\n') continue;
                        if (_commandPending) continue;
                        _command = _inBuffer.ToString();
                        _inBuffer.Clear();
                        _commandPending = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (IOException)
                {
                    return false;
                }
            } while (_clientStream.DataAvailable);

            return true;
        }

        public void Close()
        {
            _tcpClient.Close();
            _clientStream.Close();
        }
    }
}