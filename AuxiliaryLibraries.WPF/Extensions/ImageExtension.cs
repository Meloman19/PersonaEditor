using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AuxiliaryLibraries.WPF.Extensions
{
    public static class ImageExtension
    {
        public static byte[] GetData(this BitmapSource image)
        {
            var width = image.PixelWidth;
            var height = image.PixelHeight;
            var bitPerPixel = image.Format.BitsPerPixel;
            var stride = image.Format.GetStride(width);
            var LengthData = height * stride;

            var returned = new byte[LengthData];
            image.CopyPixels(returned, stride, 0);            

            return returned;
        }

        public static int GetStride(this PixelFormat pixelFormat, int width) => (pixelFormat.BitsPerPixel * width + 7) / 8;

        public static Color[] GetColors(this BitmapSource image)
        {
            FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
            var data = formatConvertedBitmap.GetData();
            Color[] returned = new Color[data.Length / 4];

            for (int i = 0, k = 0; i < data.Length & k < returned.Length; i += 4, k++)
            {
                returned[k] = Color.FromArgb(
                    data[i + 3],
                    data[i + 2],
                    data[i + 1],
                    data[i]);
            }


            return returned;
        }
    }
}