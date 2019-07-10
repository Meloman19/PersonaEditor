using PersonaEditorLib;
using PersonaEditorLib.Text;
using AuxiliaryLibraries.WPF.Wrapper;
using System.IO;
using System.Xml.Linq;

namespace PersonaEditorCMD
{
    public static class PersonaEditorTools
    {
        public static void OpenImageFile(GameFile objectFile, string path)
        {
            if (objectFile.GameData is IImage image)
            {
                try
                {
                    var temp = AuxiliaryLibraries.WPF.Tools.ImageTools.OpenPNG(path).GetBitmap();
                    image.SetBitmap(temp);
                }
                catch { }
            }
        }

        public static void SaveImageFile(GameFile objectFile, string path)
        {
            if (objectFile == null)
                throw new System.ArgumentNullException(nameof(objectFile));
            if (path == null)
                throw new System.ArgumentNullException(nameof(path));

            if (objectFile.GameData is IImage image)
            {
                var temp = image.GetBitmap()?.GetBitmapSource();
                if (temp != null)
                    AuxiliaryLibraries.WPF.Tools.ImageTools.SaveToPNG(temp, path);
            }
        }

        public static void SavePTPFile(GameFile objectFile, string path, PersonaEncoding oldEncoding = null)
        {
            if (objectFile.GameData is BMD bmd)
            {
                PTP PTP = new PTP(bmd);
                if (oldEncoding != null)
                    PTP.CopyOld2New(oldEncoding);
                File.WriteAllBytes(path, PTP.GetData());
            }
        }

        public static void OpenPTPFile(GameFile objectFile, string path, PersonaEncoding newEncoding)
        {
            if (objectFile == null)
                throw new System.ArgumentNullException(nameof(objectFile));
            if (path == null)
                throw new System.ArgumentNullException(nameof(path));
            if (newEncoding == null)
                return;

            if (objectFile.GameData is BMD bmd)
                if (File.Exists(path))
                {
                    PTP PTP = new PTP(File.ReadAllBytes(path));
                    objectFile.GameData = new BMD(PTP, newEncoding);
                }
        }

        public static void SaveTableFile(GameFile objectFile, string path)
        {
            if (objectFile == null)
                throw new System.ArgumentNullException(nameof(objectFile));
            if (path == null)
                throw new System.ArgumentNullException(nameof(path));

            if (objectFile.GameData is ITable table)
            {
                var temp = table.GetTable();
                if (temp != null)
                    temp.Save(path);
            }
        }

        public static void OpenTableFile(GameFile objectFile, string path)
        {
            if (objectFile == null)
                throw new System.ArgumentNullException(nameof(objectFile));
            if (path == null)
                throw new System.ArgumentNullException(nameof(path));

            if (objectFile.GameData is ITable table)
            {
                try
                {
                    var temp = XDocument.Load(path);
                    table.SetTable(temp);
                }
                catch { }
            }
        }
    }
}