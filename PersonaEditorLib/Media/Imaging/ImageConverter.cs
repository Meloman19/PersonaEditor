using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.Media.Imaging
{
    public class ImageBaseConverter : ImageBase
    {
        public IQuantization Quantizer { get; set; } = null;
        public byte AlphaThreshold { get; set; } = 1;

        public ImageBase Source { get; }

        public ImageBaseConverter(ImageBase imageBase) : base()
        {
            Source = imageBase;
        }

        public ImageBaseConverter(BitmapSource bitmapSource)
        {
            Source = new ImageBase(bitmapSource);
        }

        public bool TryConvert(PixelBaseFormat dstFormat)
        {
            byte[] DstData = null;
            Color[] DstPalette = null;
            PixelBaseFormat DstFormat = dstFormat;
            // imageData = null;
            // DstPalette = null;
            // DstFormat = PixelBaseFormat.Unknown;

            var SrcData = Source.GetImageData();
            var SrcFormat = Source.PixelFormat;
            var SrcPalette = Source.Palette?.Colors.ToArray();

            int dstPaletteLength = (int)Math.Pow(2, PixelFormatHelper.BitsPerPixel(SrcFormat));
            if (SrcData == null ||
               (SrcFormat.IsIndexed() && SrcPalette == null))
                return false;

            if (dstFormat == SrcFormat || dstFormat == PixelBaseFormat.Unknown)
            {
                DstData = SrcData;
                if (SrcFormat.IsIndexed())
                    DstPalette = Source.Palette?.Colors.ToArray();
            }
            else if (dstFormat.IsIndexed())
            {
                if (SrcFormat.IsIndexed() &&
                    PixelFormatHelper.BitsPerPixel(SrcFormat) <= PixelFormatHelper.BitsPerPixel(dstFormat))
                {
                    var dataConverter = PixelConverter.GetDataConverter(SrcFormat, dstFormat);
                    if (dataConverter == null)
                        return false;
                    else
                    {
                        DstData = dataConverter(SrcData);
                        DstPalette = new Color[dstPaletteLength];
                        Array.Copy(SrcPalette, 0, DstPalette, 0, SrcPalette.Length);
                    }
                }
                else
                {
                    if (Quantizer == null)
                        return false;

                    var ColorConverter = PixelConverter.GetDataColorConverter(SrcFormat);
                    if (ColorConverter == null)
                        return false;

                    Color[] srcDataColor = ColorConverter(SrcData, SrcPalette);
                    Quantizer.PixelFormat = PixelFormatHelper.CompatibilityFormat(DstFormat);
                    Quantizer.StartQuantization(srcDataColor);

                    var pixelConverter = PixelConverter.GetDataConverter(PixelFormatHelper.ConvertFromSystem(Quantizer.PixelFormat), DstFormat);
                    if (pixelConverter == null)
                        DstData = Quantizer.QuantData;
                    else
                        DstData = pixelConverter(Quantizer.QuantData);

                    DstPalette = Quantizer.QuantPalette;
                }
            }
            else if (dstFormat.IsCompressed())
                ImageCompress.DDSCompress(Source.Width, Source.Height, SrcData, dstFormat, AlphaThreshold, out DstData);
            else
            {
                var Data2Data = PixelConverter.GetDataConverter(SrcFormat, dstFormat);
                if (Data2Data == null)
                {
                    var byte2color = PixelConverter.GetDataColorConverter(SrcFormat);
                    var color2byte = PixelConverter.GetColorDataConverter(dstFormat);
                    if (byte2color == null || color2byte == null)
                        return false;
                    else
                        DstData = color2byte(byte2color(SrcData, SrcPalette));
                }
                else
                    DstData = Data2Data(SrcData);
            }

            width = Source.Width;
            height = Source.Height;
            imageData = DstData;
            pixelFormat = dstFormat;
            bitmapPalette = DstPalette == null ? null : new BitmapPalette(DstPalette.ToArray());

            return true;
        }
    }
}