using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;

namespace PersonaEditor.Classes
{
    class NamedPipeManager
    {
        public static string NamedPipeName { get; } = "PersonaEditor";
        public event Action<string> ReceiveString;

        private const string EXIT_STRING = "__EXIT__";
        private BackgroundWorker backgroundWorker;

        public void Start()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            Write(EXIT_STRING);
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorker.Dispose();
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ReceiveString?.Invoke(e.UserState as string);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                string result;
                using (var server = new NamedPipeServerStream(NamedPipeName))
                {
                    server.WaitForConnection();

                    using (StreamReader reader = new StreamReader(server))
                        result = reader.ReadToEnd();
                }

                if (result == EXIT_STRING)
                    break;

                backgroundWorker.ReportProgress(0, result);
            }
        }

        public static bool Write(string[] text, int connectTimeout = 300)
        {
            using (var client = new NamedPipeClientStream(NamedPipeName))
            {
                try
                {
                    client.Connect(connectTimeout);
                }
                catch
                {
                    return false;
                }

                if (!client.IsConnected)
                    return false;

                using (StreamWriter writer = new StreamWriter(client))
                {
                    foreach (var a in text)
                        writer.Write(a + '\n');
                    writer.Flush();
                }
            }
            return true;
        }

        public static bool Write(string text, int connectTimeout = 300)
        {
            using (var client = new NamedPipeClientStream(NamedPipeName))
            {
                try
                {
                    client.Connect(connectTimeout);
                }
                catch
                {
                    return false;
                }

                if (!client.IsConnected)
                    return false;

                using (StreamWriter writer = new StreamWriter(client))
                {
                    writer.Write(text);
                    writer.Flush();
                }
            }
            return true;
        }
    }
}