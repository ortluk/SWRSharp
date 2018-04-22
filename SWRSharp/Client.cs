using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SWRSharp
{
    public class Client
    {
        private NetworkStream clientStream;
        private TcpClient tcpClient;
        private StringBuilder inBuffer;
        private String Command;
        private bool commandPending;

        public Client(TcpClient inClient)
        {
            tcpClient = inClient;
            clientStream = tcpClient.GetStream();
            inBuffer = new StringBuilder();
            commandPending = false;

        }

        public void Send(String msg)
        {
            if (clientStream.CanWrite)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(msg);
                clientStream.Write(buffer,0,buffer.Length);
            }
        }

        public String GetCommand()
        {
            String ReturnValue = String.Copy(Command);
            Command = "";
            commandPending = false;
            return ReturnValue;
        }

        public bool isCommandPending()
        {
            return commandPending;
        }
        
        public bool AttemptRead()
        {
            
            if (clientStream.CanRead && tcpClient.Client.Poll(1,SelectMode.SelectRead))
            {
                byte[] ReadBuffer = new byte[1024];
                int bytesread = 0;

                do
                {
                    try
                    {
                        if ((bytesread = clientStream.Read(ReadBuffer, 0, ReadBuffer.Length)) != 0)
                        {
                            inBuffer.AppendFormat("{0}", Encoding.ASCII.GetString(ReadBuffer, 0, bytesread));
                            if (ReadBuffer[bytesread- 1] == '\r' || ReadBuffer[bytesread - 1] == '\n')
                            {
                                if (!commandPending)
                                {
                                    Command = inBuffer.ToString();
                                    inBuffer.Clear();
                                    commandPending = true;
                                    return true;
                                }
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
                } while (clientStream.DataAvailable);
            }
            return true;
        }
        public void Close()
        {
            tcpClient.Close();
            clientStream.Close();
        }
        
    }
}