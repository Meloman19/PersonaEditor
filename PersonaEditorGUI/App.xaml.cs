using System.Windows;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using System;
using PersonaEditorLib;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Collections;

namespace PersonaEditorGUI
{
    public partial class App : Application
    {
        public Mutex Mutex;
        NamedPipeManager NamedPipeManager;
        MainWindowVM MainWindowVM;

        public App()
        {
            InitializeComponent();
            LoadLocalization();
            //SaveLocaliztion();
        }

        private void LoadLocalization()
        {
            if (Settings.App.Default.DefaultLocalization != "Default")
            {
                ResourceDictionary resourceDictionary = Resources.MergedDictionaries[0];

                XDocument xDocument;

                try
                {
                    xDocument = XDocument.Load(Path.Combine(Static.Paths.DirLang, Settings.App.Default.DefaultLocalization + ".xml"));
                }
                catch
                {
                    return;
                }

                var loc = xDocument.Element("Localization");

                foreach (var a in loc?.Elements())
                {
                    string key = a.Attribute("Key").Value;
                    string value = a.Value.Replace("\\n", "\n");

                    if (resourceDictionary.Contains(key))
                        resourceDictionary[key] = value;
                }
            }
        }

        private void SaveLocaliztion()
        {

            ResourceDictionary resourceDictionary = Resources.MergedDictionaries[0];

            XDocument xDocument = new XDocument();

            XElement loc = new XElement("Localization");
            xDocument.Add(loc);

            foreach (DictionaryEntry a in resourceDictionary)
            {
                XElement xElement = a.Value is string str ? new XElement("String", str.Replace("\n", "\\n")) : new XElement("String", a.Value);
                xElement.Add(new XAttribute("Key", a.Key));
                loc.Add(xElement);
            }

            xDocument.Save("Localization.xml");
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Mutex = new Mutex(true, "PersonaEditor", out bool Is);
            if (!Is)
            {
                NamedPipeManager.Write(e.Args);
                Shutdown(0);
                return;
            }

            NamedPipeManager = new NamedPipeManager();
            NamedPipeManager.ReceiveString += NamedPipeManager_ReceiveString;
            NamedPipeManager.Start();

            MainWindowVM = new MainWindowVM();
            if (e.Args.Length > 0)
                MainWindowVM.OpenFile(e.Args[0]);

            MainWindow window = new MainWindow() { DataContext = MainWindowVM };
            MainWindow = window;
            window.Show();
        }

        private void NamedPipeManager_ReceiveString(string obj)
        {
            MainWindow.Activate();
            string[] objlist = obj.Split('\n');
            if (objlist.Length > 0)
                MainWindowVM.OpenFile(objlist[0]);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logging.Write("", e.Exception);
            NamedPipeManager?.Stop();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NamedPipeManager?.Stop();
        }
    }

    public class NamedPipeManager
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