using System.IO;
using System.Linq;
using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditorLib;
using PersonaEditorLib.Other;
using PersonaEditorLib.Sprite;

namespace PersonaEditor.Samples
{
    public static class ImageProcessing
    {
        public static void ExportImages(string filePath, string outputDir)
        {
            // Try open file as known format;
            var gf = GameFormatHelper.OpenUnknownFile(Path.GetFileName(filePath), File.ReadAllBytes(filePath));

            // Collect all DDS
            var ddsGFs = gf.GetAllObjectOfType<DDS>()
                .Concat(gf.GetAllObjectOfType<DDSAtlus>())
                .ToArray();
            if (!ddsGFs.Any())
                return;

            foreach (var ddsGF in ddsGFs)
            {
                var dds = ddsGF.GameData as DDS;

                var bitmap = dds.GetBitmap().GetBitmapSource();

                var outputPath = Path.Combine(outputDir, ddsGF.Name);

                ImageTools.SaveToPNG(bitmap, outputPath);
            }
        }

        public static void ImportImage(DDS dds, string pngPath)
        {
            var bitmap = ImageTools.OpenPNG(pngPath);
            var pixelmap = bitmap.GetBitmap();

            dds.SetBitmap(pixelmap);
        }
    }
}