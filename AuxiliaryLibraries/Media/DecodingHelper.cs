namespace AuxiliaryLibraries.Media
{
    public static class DecodingHelper
    {
        public static Pixel[] FromArgb32(byte[] data)
        {
            int size = data.Length / 4;
            Pixel[] returned = new Pixel[size];

            for (int i = 0, k = 0; i < returned.Length; i++, k += 4)
                returned[i] = Pixel.FromArgb(
                    data[k],
                    data[k + 1],
                    data[k + 2],
                    data[k + 3]);

            return returned;
        }

        public static Pixel[] FromBgra32(byte[] data)
        {
            int size = data.Length / 4;
            Pixel[] returned = new Pixel[size];

            for (int i = 0, k = 0; i < returned.Length; i++, k += 4)
                returned[i] = Pixel.FromArgb(
                    data[k + 3],
                    data[k + 2],
                    data[k + 1],
                    data[k]);

            return returned;
        }

        public static Pixel[] FromRgba32(byte[] data)
        {
            int size = data.Length / 4;
            Pixel[] returned = new Pixel[size];

            for (int i = 0, k = 0; i < returned.Length; i++, k += 4)
                returned[i] = Pixel.FromArgb(
                    data[k + 3],
                    data[k],
                    data[k + 1],
                    data[k + 2]);

            return returned;
        }

        public static Pixel[] FromRgba32PS2(byte[] data)
        {
            int size = data.Length / 4;
            Pixel[] returned = new Pixel[size];

            for (int i = 0, k = 0; i < returned.Length; i++, k += 4)
                returned[i] = Pixel.FromArgb(
                    BitHelper.AlphaPS2ToPC[data[k + 3]],
                    data[k],
                    data[k + 1],
                    data[k + 2]);

            return returned;
        }

        public static Pixel[] FromIndexed4(byte[] data, Pixel[] palette, int width)
        {
            Pixel[] returned;
            if (width % 2 == 0)
            {
                int size = data.Length * 2;
                returned = new Pixel[size];

                for (int i = 0, k = 0; i < size; i += 2, k++)
                {
                    int ind1 = data[k] >> 4;
                    int ind2 = data[k] & 0x0F;

                    returned[i] = palette[ind1];
                    returned[i + 1] = palette[ind2];
                }
            }
            else
            {
                int height = data.Length / (width + 1);
                int size = height * width;
                returned = new Pixel[size];

                int tempWidth = width - 1;
                for (int x = 0, y = 0, i = 0; y < height; y++)
                {
                    for (; x < tempWidth; x += 2, i++)
                    {
                        int ind1 = data[i] >> 4;
                        int ind2 = data[i] & 0x0F;

                        returned[x] = palette[ind1];
                        returned[x + 1] = palette[ind2];
                    }

                    returned[x] = palette[data[i] >> 4];
                    i++;
                    x++;
                    tempWidth += width;
                }
            }

            return returned;
        }

        public static Pixel[] FromIndexed4Reverse(byte[] data, Pixel[] palette, int width)
        {
            Pixel[] returned;
            if (width % 2 == 0)
            {
                int size = data.Length * 2;
                returned = new Pixel[size];

                for (int i = 0, k = 0; i < size; i += 2, k++)
                {
                    int ind1 = data[k] & 0x0F;
                    int ind2 = data[k] >> 4;

                    returned[i] = palette[ind1];
                    returned[i + 1] = palette[ind2];
                }
            }
            else
            {
                int height = data.Length / (width + 1);
                int size = height * width;
                returned = new Pixel[size];

                int tempWidth = width - 1;
                for (int x = 0, y = 0, i = 0; y < height; y++)
                {
                    for (; x < tempWidth; x += 2, i++)
                    {
                        int ind1 = data[i] & 0x0F;
                        int ind2 = data[i] >> 4;

                        returned[x] = palette[ind1];
                        returned[x + 1] = palette[ind2];
                    }

                    returned[x] = palette[data[i] >> 4];
                    i++;
                    x++;
                    tempWidth += width;
                }
            }

            return returned;
        }

        public static Pixel[] FromIndexed8(byte[] data, Pixel[] palette)
        {
            int size = data.Length;
            Pixel[] returned = new Pixel[size];

            for (int i = 0; i < size; i++)
                returned[i] = palette[data[i]];

            return returned;
        }
    }
}