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
        
        public enum  ConnectionState
        {
            CON_GET_NAME,
            CON_GET_PASSWORD,
            CON_CONFIRM_PASSWORD,
            CON_PLAYING
        };
        
        public Client(TcpClient inClient)
        {
            _tcpClient = inClient;
            _clientStream = _tcpClient.GetStream();
            _inBuffer = new StringBuilder();
            _commandPending = false;
        }

        public void Send(string msg)
        {
            if (_clientStream.CanWrite)
            {
                var buffer = Encoding.UTF8.GetBytes(msg);
                _clientStream.Write(buffer, 0, buffer.Length);
            }
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
            if (_clientStream.CanRead && _tcpClient.Client.Poll(1, SelectMode.SelectRead))
            {
                var readBuffer = new byte[1024];
                do
                {
                    try
                    {
                        var bytesread = 0;
                        if ((bytesread = _clientStream.Read(readBuffer, 0, readBuffer.Length)) != 0)
                        {
                            _inBuffer.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, bytesread));
                            if (readBuffer[bytesread - 1] == '\r' || readBuffer[bytesread - 1] == '\n')
                                if (!_commandPending)
                                {
                                    _command = _inBuffer.ToString();
                                    _inBuffer.Clear();
                                    _commandPending = true;
                                    return true;
                                }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (IOException e)
                    {
                        return false;
                    }
                } while (_clientStream.DataAvailable);
            }

            return true;
        }

        public void Close()
        {
            _tcpClient.Close();
            _clientStream.Close();
        }
    }
}