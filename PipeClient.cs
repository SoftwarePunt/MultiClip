using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;

namespace MultiClip
{
    public class PipeClient
    {
        private NamedPipeClientStream pipeClient;

        public void Open()
        {
            pipeClient = new NamedPipeClientStream(".", Program.PROCESS_PIPE_NAME, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            Debug.WriteLine("Connecting to server via named pipe (" + Program.PROCESS_PIPE_NAME + ")...\n");
            pipeClient.Connect(3000);
        }

        public void Send(string data)
        {
            var ss = new StreamString(pipeClient);
            ss.WriteString(data);
        }

        public string Read()
        {
            var ss = new StreamString(pipeClient);
            return ss.ReadString();
        }

        public string SendAndRead(string data)
        {
            Send(data);
            return Read();
        }

        public void Close()
        {
            try
            {
                pipeClient.Close();
            }
            catch (Exception) { }

            pipeClient = null;
        }
    }
}