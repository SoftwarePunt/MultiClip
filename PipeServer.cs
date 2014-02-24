using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace MultiClip
{
    public class PipeServer
    {
        private static Thread serverThread;
        private static bool run;

        public static void Start()
        {
            run = true;

            serverThread = new Thread(ExecuteThread);
            serverThread.Name = "PipeServerWatcher";
            serverThread.Start();
        }

        public static void Stop()
        {
            run = false;

            try
            {
                serverThread.Abort();
            }
            catch (Exception) { }
        }

        private static void ExecuteThread()
        {
            Debug.WriteLine("\n*** Named pipe server stream starting (" + Program.PROCESS_PIPE_NAME + ") ***\n");

            var pipeServer = new NamedPipeServerStream(Program.PROCESS_PIPE_NAME, PipeDirection.InOut, 1);
            int threadId = Thread.CurrentThread.ManagedThreadId;

            while (run)
            {
                pipeServer.WaitForConnection();

                try
                {
                    StreamString ss = new StreamString(pipeServer);
                    var incoming = ss.ReadString();

                    Debug.WriteLine("Incoming command from pipe: " + incoming);
                    ss.WriteString("OK");
                    pipeServer.Disconnect();

                    if (incoming.Length <= 0)
                    {
                        continue;
                    }

                    string[] commandParts = incoming.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    CommandProcessor.Process(commandParts);
                }
                catch (IOException e)
                {
                    Debug.WriteLine("\n*** ERROR - Named pipe server stream failed, exception details follow [will try to continue] (" + Program.PROCESS_PIPE_NAME + ") ***\n\n {0} \n\n", e.Message);

                    try
                    {
                        pipeServer.Disconnect();
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}