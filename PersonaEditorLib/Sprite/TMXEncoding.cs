using AuxiliaryLibraries.Media;
using System;

namespace PersonaEditorLib.Sprite
{
    public static class TMXEncoding
    {
        public static void Encode(TMX tmx, PixelMap pixelMap)
        {
            byte[] pallete;
            byte[] imageData;
            switch (tmx.Header.PixelFormat)
            {
                case TMXPixelFormatEnum.PSMT4:
                    {
                        var res = EncodingHelper.ToIndexed4Reverse(pixelMap.Pixels);
                        imageData = res.data;

                        if (tmx.Header.PaletteFormat != TMXPixelFormatEnum.PSMTC32)
                            throw new Exception();

                        pallete = EncodingHelper.ToRgba32PS2(res.pallete);
                    }
                    break;
                case TMXPixelFormatEnum.PSMT8:
                    {
                        var res = EncodingHelper.ToIndexed8(pixelMap.Pixels);
                        imageData = res.data;

                        if (tmx.Header.PaletteFormat != TMXPixelFormatEnum.PSMTC32)
                            throw new Exception();

                        pallete = TMXHelper.TilePalette(EncodingHelper.ToRgba32PS2(res.pallete));
                    }
                    break;
                case TMXPixelFormatEnum.PSMTC32:
                    {
                        imageData = EncodingHelper.ToRgba32PS2(pixelMap.Pixels);
                        pallete = null;
                    }
                    break;
                default:
                    throw new Exception();
            }

            if (pallete != null)
                tmx.Pallete = new byte[][] { pallete };
            else
                tmx.Pallete = Array.Empty<byte[]>();

            tmx.ImageData = imageData;
        }
    }
}