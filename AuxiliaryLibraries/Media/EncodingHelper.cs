using System;

namespace AuxiliaryLibraries.Media
{
    public static class EncodingHelper
    {
        public static byte[] ToArgb32(Pixel[] pixels)
        {
            int size = pixels.Length * 4;
            byte[] returned = new byte[size];

            for (int i = 0, k = 0; i < pixels.Length; i++, k += 4)
            {
                returned[k] = pixels[i].A;
                returned[k + 1] = pixels[i].R;
                returned[k + 2] = pixels[i].G;
                returned[k + 3] = pixels[i].B;
            }

            return returned;
        }

        public static byte[] ToBgra32(Pixel[] pixels)
        {
            int size = pixels.Length * 4;
            byte[] returned = new byte[size];

            for (int i = 0, k = 0; i < pixels.Length; i++, k += 4)
            {
                returned[k] = pixels[i].B;
                returned[k + 1] = pixels[i].G;
                returned[k + 2] = pixels[i].R;
                returned[k + 3] = pixels[i].A;
            }

            return returned;
        }

        public static byte[] ToRgba32(Pixel[] pixels)
        {
            int size = pixels.Length * 4;
            byte[] returned = new byte[size];

            for (int i = 0, k = 0; i < pixels.Length; i++, k += 4)
            {
                returned[k] = pixels[i].R;
                returned[k + 1] = pixels[i].G;
                returned[k + 2] = pixels[i].B;
                returned[k + 3] = pixels[i].A;
            }

            return returned;
        }

        public static byte[] ToRgba32PS2(Pixel[] pixels)
        {
            int size = pixels.Length * 4;
            byte[] returned = new byte[size];

            for (int i = 0, k = 0; i < pixels.Length; i++, k += 4)
            {
                returned[k] = pixels[i].R;
                returned[k + 1] = pixels[i].G;
                returned[k + 2] = pixels[i].B;
                returned[k + 3] = BitHelper.AlphaPCToPS2[pixels[i].A];
            }

            return returned;
        }

        public static (byte[] data, Pixel[] pallete) ToIndexed4(Pixel[] pixels)
        {
            var qu = new WuAlphaColorQuantizer();
            if (!qu.StartQuantization(pixels, 16))
                throw new Exception();

            return (qu.QuantData, qu.QuantPalette);
        }

        public static byte[] ToIndexed4(Pixel[] pixels, Pixel[] palette, int width)
        {
            var indexes = ImageHelper.GetIndexes(pixels, palette);
            return ImageHelper.IndexesToData(indexes, 4, width);
        }

        public static (byte[] data, Pixel[] pallete) ToIndexed4Reverse(Pixel[] pixels)
        {
            var res = ToIndexed4(pixels);

            for (int i = 0; i < res.data.Length; i++)
                res.data[i] = PixelFormatHelper.ReverseByte(res.data[i]);

            return res;
        }

        public static byte[] ToIndexed4Reverse(Pixel[] pixels, Pixel[] palette, int width)
        {
            var indexes = ImageHelper.GetIndexes(pixels, palette);
            var res = ImageHelper.IndexesToData(indexes, 4, width);

            for (int i = 0; i < res.Length; i++)
                res[i] = PixelFormatHelper.ReverseByte(res[i]);

            return res;
        }

        public static (byte[] data, Pixel[] pallete) ToIndexed8(Pixel[] pixels)
        {
            var qu = new WuAlphaColorQuantizer();
            if (!qu.StartQuantization(pixels, 256))
                throw new Exception();

            return (qu.QuantData, qu.QuantPalette);
        }

        public static byte[] ToIndexed8(Pixel[] pixels, Pixel[] palette, int width)
        {
            var indexes = ImageHelper.GetIndexes(pixels, palette);
            return ImageHelper.IndexesToData(indexes, 8, width);
        }

        public static byte[] ToFullRgba32PS2(Pixel pixel, bool reverseOrder)
        {
            byte[] data =
            [
                BitHelper.AlphaPCToPS2[pixel.R],
                BitHelper.AlphaPCToPS2[pixel.G],
                BitHelper.AlphaPCToPS2[pixel.B],
                BitHelper.AlphaPCToPS2[pixel.A],
            ];

            if (reverseOrder)
                Array.Reverse(data);

            return data;
        }
    }
}