using System.Text;
using System.Threading;
using System.Windows;
using PersonaEditor.Common;
using PersonaEditor.Views;

namespace PersonaEditor
{
    public partial class App : Application
    {
        private const string CURRENT_NAMED_PIPE = "PersonaEditor";

        private Mutex Mutex;
        private NamedPipeManager NamedPipeManager;
        private MainWindowVM MainWindowVM;

        public App()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                Static.SettingsProvider.Load();
            }
            catch { }

            if (Static.SettingsProvider.AppSettings.SingleInstanceApplication)
            {
                Mutex = new Mutex(true, CURRENT_NAMED_PIPE, out bool Is);
                if (!Is)
                {
                    NamedPipeManager.Write(CURRENT_NAMED_PIPE, e.Args);
                    Shutdown(0);
                    return;
                }

                NamedPipeManager = new NamedPipeManager(CURRENT_NAMED_PIPE);
                NamedPipeManager.ReceiveString += NamedPipeManager_ReceiveString;
                NamedPipeManager.Start();
            }

            MainWindowVM = new MainWindowVM();
            if (e.Args.Length > 0)
                MainWindowVM.OpenFile(e.Args[0]);

            MainWindow = new MainWindow() { DataContext = MainWindowVM };
            MainWindow.Show();
        }

        private void NamedPipeManager_ReceiveString(string obj)
        {
            MainWindow.Activate();
            string[] objlist = obj.Split('\n');
            if (objlist.Length > 0)
                MainWindowVM.OpenFile(objlist[0]);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            NamedPipeManager?.Stop();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NamedPipeManager?.Stop();
        }
    }
}