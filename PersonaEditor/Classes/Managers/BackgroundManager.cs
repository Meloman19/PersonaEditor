using AuxiliaryLibraries.WPF;
using PersonaEditor.Classes.Visual;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace PersonaEditor.Classes.Managers
{
    class BackgroundManager : BindingObject
    {
        Dictionary<string, Background> backgrounds = new Dictionary<string, Background>()
        {
            { "Empty", new Background() }
        };

        string sourcedir;

        ObservableCollection<string> backgroundList = new ObservableCollection<string>() { "Empty" };

        public ReadOnlyObservableCollection<string> BackgroundList { get; }

        public BackgroundManager(string dir)
        {
            sourcedir = dir;
            BackgroundList = new ReadOnlyObservableCollection<string>(backgroundList);

            if (Directory.Exists(dir))
            {
                var filelist = Directory.EnumerateFiles(dir);
                foreach (var file in filelist)
                    if (Path.GetExtension(file).ToLower() == ".png")
                    {
                        var temp = Path.GetFileNameWithoutExtension(file);
                        if (temp != "Empty")
                            backgroundList.Add(temp);
                    }
            }

            new EventWrapperINPC(ApplicationSettings.BackgroundDefault.Default, this);
        }

        public Background GetBackground(int index)
        {
            string file = backgroundList[index];
            return GetBackground(file);
        }

        public Background GetBackground(string name)
        {
            if (backgrounds.ContainsKey(name))
                return backgrounds[name];
            else
            {
                try
                {
                    var bckg = new Background(Path.Combine(sourcedir, name + ".png"), Path.Combine(sourcedir, name + ".xml"));
                    backgrounds.Add(name, bckg);
                    return bckg;
                }
                catch
                {
                    backgrounds.Add(name, null);
                    return null;
                }
            }
        }

        public int GetBackgroundIndex(string name)
        {
            if (backgroundList.Contains(name))
                return backgroundList.IndexOf(name);
            else
                return -1;
        }

        public string GetBackgroundName(int index)
        {
            if (index >= 0 && index < backgroundList.Count)
                return backgroundList[index];
            else
                return "";
        }

        public void EmptyUpdate()
        {
            GetBackground(0).SetEmpty();
        }
    }
}