using AuxiliaryLibraries.GameFormat;
using AuxiliaryLibraries.GameFormat.Text;
using AuxiliaryLibraries.WPF.Wrapper;
using System.IO;
using System.Xml.Linq;

namespace PersonaEditorCMD
{
    public static class PersonaEditorTools
    {
        public static void OpenImageFile(ObjectContainer objectFile, string path)
        {
            if (objectFile.Object is IImage image)
            {
                try
                {
                    var temp = AuxiliaryLibraries.WPF.Tools.ImageTools.OpenPNG(path).GetBitmap();
                    image.SetBitmap(temp);
                }
                catch { }
            }
        }

        public static void SaveImageFile(ObjectContainer objectFile, string path)
        {
            if (objectFile?.Object is IImage image)
            {
                var temp = image.GetBitmap().GetBitmapSource();
                if (temp != null)
                    AuxiliaryLibraries.WPF.Tools.ImageTools.SaveToPNG(temp, path);
            }
        }

        public static void SavePTPFile(ObjectContainer objectFile, string path, PersonaEncoding oldEncoding = null)
        {
            if (objectFile.Object is BMD bmd)
            {
                PTP PTP = new PTP(bmd);
                if (oldEncoding != null)
                    PTP.CopyOld2New(oldEncoding);
                File.WriteAllBytes(path, PTP.GetData());
            }
        }

        public static void OpenPTPFile(ObjectContainer objectFile, string path, PersonaEncoding newEncoding)
        {
            if (newEncoding == null)
                return;
            if (objectFile.Object is BMD bmd)
                if (File.Exists(path))
                {
                    PTP PTP = new PTP(File.ReadAllBytes(path));
                    bmd.Open(PTP, newEncoding);
                }
        }

        public static void SaveTableFile(ObjectContainer objectFile, string path)
        {
            if (objectFile?.Object is ITable table)
            {
                var temp = table.GetTable();
                if (temp != null)
                    temp.Save(path);
            }
        }

        public static void OpenTableFile(ObjectContainer objectFile, string path)
        {
            if (objectFile?.Object is ITable table)
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