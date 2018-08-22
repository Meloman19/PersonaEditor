using AuxiliaryLibraries.GameFormat;
using AuxiliaryLibraries.GameFormat.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace PersonaEditor
{
    public static class PersonaEditorTools
    {
        public static byte[] GetData(this BitmapSource image)
        {
            var width = image.PixelWidth;
            var height = image.PixelHeight;
            var bitPerPixel = image.Format.BitsPerPixel;
            var stride = GetStride(image.Format, width);

            var LengthData = (height * width * bitPerPixel) / 8;

            var returned = new byte[LengthData];
            image.CopyPixels(returned, stride, 0);

            return returned;
        }

        public static int GetStride(PixelFormat pixelFormat, int width)
        {
            return (pixelFormat.BitsPerPixel * width + 7) / 8;
        }

        public static BitmapSource OpenPNG(string path)
        {
            return new BitmapImage(new Uri(Path.GetFullPath(path)));

            //using (FileStream FS = new FileStream(path, FileMode.Open))
            //{
            //    var returned = new BitmapImage(new Uri(Path.GetFullPath(path)));
            //    returned.BeginInit();
            //    returned.StreamSource = FS;
            //    returned.EndInit();

            //    return returned;
            //}
        }

        public static void OpenImageFile(ObjectContainer objectFile, string path)
        {
            if (objectFile.Object is IImage image)
            {
                try
                {
                    var temp = OpenPNG(path).GetBitmap();
                    image.SetBitmap(temp);
                }
                catch { }
            }
        }

        public static void SaveToPNG(BitmapSource image, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            using (FileStream FS = new FileStream(path, FileMode.Create))
            {
                PngBitmapEncoder PNGencoder = new PngBitmapEncoder();

                PNGencoder.Frames.Add(BitmapFrame.Create(image));
                PNGencoder.Save(FS);
            }
        }

        public static void SaveImageFile(ObjectContainer objectFile, string path)
        {
            if (objectFile?.Object is IImage image)
            {
                var temp = image.GetBitmap().GetBitmapSource();
                if (temp != null)
                    SaveToPNG(temp, path);
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

        public static void SaveTableFile(ObjectContainer objectFile, string path)
        {
            if (objectFile?.Object is ITable table)
            {
                var temp = table.GetTable();
                if (temp != null)
                    temp.Save(path);
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