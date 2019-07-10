using System.Windows;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using System;
using System.ComponentModel;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using PersonaEditor.Views;
using PersonaEditor.Classes;
using PersonaEditorLib.Text;
using PersonaEditorLib.FileContainer;

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
            //Test();
            InitializeComponent();
            LoadLocalization();
            //SaveLocalization();
        }

        private void Test()
        {
            string dir = @"";
            var enc = new PersonaEditorLib.PersonaEncoding(@"");

            string[] files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (new FileInfo(file).Length > 100000000)
                    continue;

                var temp = PersonaEditorLib.GameFormatHelper.OpenFile(Path.GetFileName(file), File.ReadAllBytes(file));

                if (temp != null)
                {
                    foreach (var a in temp.GetAllObjectFiles(PersonaEditorLib.FormatEnum.BMD))
                    {
                        var bmd = a.GameData as BMD;
                        var ptp = new PTP(bmd);

                        string newPath = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(a.Name.Replace('/', '+')) + ".txt");
                        File.WriteAllLines(newPath, ptp.ExportTXT(true, enc));
                    }
                }
            }
        }

        private void LoadLocalization()
        {
            if (ApplicationSettings.AppSetting.Default.DefaultLocalization != "Default")
            {
                XDocument xDocument;

                try
                {
                    xDocument = XDocument.Load(Path.Combine(Static.Paths.DirLang, ApplicationSettings.AppSetting.Default.DefaultLocalization + ".xml"));
                }
                catch
                {
                    return;
                }

                var loc = xDocument.Element("Localization");

                foreach (var dict in Resources.MergedDictionaries)
                {
                    var lang = loc.Element(Path.GetFileNameWithoutExtension(dict.Source.OriginalString));

                    foreach (var a in lang != null ? lang.Elements() : new List<XElement>())
                    {
                        string key = a.Attribute("Key").Value;
                        string value = a.Value.Replace("\\n", "\n");

                        if (dict.Contains(key))
                            dict[key] = value;
                    }
                }
            }
        }

        private void SaveLocalization()
        {
            XDocument xDocument = new XDocument();

            XElement loc = new XElement("Localization");
            xDocument.Add(loc);

            foreach (var dict in Resources.MergedDictionaries)
            {
                XElement dic = new XElement(Path.GetFileNameWithoutExtension(dict.Source.OriginalString));
                loc.Add(dic);
                foreach (DictionaryEntry a in dict)
                {
                    XElement xElement = a.Value is string str ? new XElement("String", str.Replace("\n", "\\n")) : new XElement("String", a.Value);
                    xElement.Add(new XAttribute("Key", a.Key));
                    dic.Add(xElement);
                }
            }

            xDocument.Save("Localization.xml");
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