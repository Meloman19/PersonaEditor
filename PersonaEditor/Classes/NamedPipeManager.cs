using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;

namespace PersonaEditor.Classes
{
    class NamedPipeManager
    {
        #region Private Field

        private BackgroundWorker backgroundWorker;

        private string name;

        private bool disposed = false;

        #endregion

        #region Events

        public event Action<string> ReceiveString;

        #endregion

        public NamedPipeManager(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));

            this.name = name;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        #region Public Methods
        
        public void Start()
        {
            backgroundWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            disposed = false;
            Write(name, "");
        }

        public static bool Write(string name, string[] text, int connectTimeout = 300)
        {
            using (var client = new NamedPipeClientStream(name))
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

        public static bool Write(string name, string text, int connectTimeout = 300)
        {
            using (var client = new NamedPipeClientStream(name))
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

        #endregion Public Methods

        #region Private Methods

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
            if (disposed)
                return;

            while (true)
            {
                string result;
                using (var server = new NamedPipeServerStream(name))
                {
                    server.WaitForConnection();

                    using (StreamReader reader = new StreamReader(server))
                        result = reader.ReadToEnd();
                }

                if (disposed)
                    break;

                backgroundWorker.ReportProgress(0, result);
            }
        }

        #endregion
    }
}