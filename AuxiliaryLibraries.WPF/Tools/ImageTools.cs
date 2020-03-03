using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace AuxiliaryLibraries.WPF.Tools
{
    public static class ImageTools
    {
        public static BitmapSource OpenPNG(string path)
        {
            using (FileStream FS = new FileStream(path, FileMode.Open))
            {
                PngBitmapDecoder decoder = new PngBitmapDecoder(FS, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                var frame = decoder.Frames[0];
                if (frame.CanFreeze)
                    frame.Freeze();
                return frame;
            }
        }

        public static void SaveToPNG(BitmapSource image, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            using (FileStream FS = new FileStream(path, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(FS);
            }
        }
    }
}