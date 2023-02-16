using System.Windows;
using System.Threading;
using PersonaEditor.Views;
using PersonaEditor.Classes;

namespace PersonaEditor
{
    public partial class App : Application
    {
        private const string CURRENT_NAMED_PIPE = "PersonaEditor";

        Mutex Mutex;
        NamedPipeManager NamedPipeManager;
        MainWindowVM MainWindowVM;

        public App()
        {
            Startup += Application_Startup;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            Exit += Application_Exit;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (ApplicationSettings.AppSetting.Default.SingleInstanceApplication)
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

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            NamedPipeManager?.Stop();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NamedPipeManager?.Stop();
        }
    }
}