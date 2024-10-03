using AuxiliaryLibraries.Media;
using System;

namespace PersonaEditorLib.Sprite
{
    public static class TMXDecoding
    {
        public static PixelMap Decode(TMX tmx)
        {
            if (tmx.Pallete.Length != 0)
            {
                var palleteData = tmx.Pallete[tmx.CurrentPallete];
                if (tmx.Header.PixelFormat == TMXPixelFormatEnum.PSMT8)
                    palleteData = TMXHelper.TilePalette(palleteData);

                Pixel[] pallete;
                switch (tmx.Header.PaletteFormat)
                {
                    case TMXPixelFormatEnum.PSMTC32:
                        pallete = DecodingHelper.FromRgba32PS2(palleteData);
                        break;
                    default:
                        throw new Exception();
                }

                Pixel[] pixels;
                switch (tmx.Header.PixelFormat)
                {
                    case TMXPixelFormatEnum.PSMT4:
                        pixels = DecodingHelper.FromIndexed4Reverse(tmx.ImageData, pallete, tmx.Header.Width);
                        break;
                    case TMXPixelFormatEnum.PSMT8:
                        pixels = DecodingHelper.FromIndexed8(tmx.ImageData, pallete);
                        break;
                    default:
                        throw new Exception();
                }

                return new PixelMap(tmx.Header.Width, tmx.Header.Height, pixels);
            }
            else
            {
                Pixel[] pixels;
                switch (tmx.Header.PixelFormat)
                {
                    case TMXPixelFormatEnum.PSMTC32:
                        pixels = DecodingHelper.FromRgba32PS2(tmx.ImageData);
                        break;
                    default:
                        throw new Exception();
                }

                return new PixelMap(tmx.Header.Width, tmx.Header.Height, pixels);
            }
        }
    }
}