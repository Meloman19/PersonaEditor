using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditorLib;
using PersonaEditorLib.Sprite;
using System.IO;
using System.Linq;

namespace PersonaEditor.Samples
{
    public static class ImageProcessing
    {
        public static void ExportImages(string filePath, string outputDir)
        {
            // Try open file as known format;
            var gf = GameFormatHelper.OpenFile(Path.GetFileName(filePath), File.ReadAllBytes(filePath));
            if (gf == null)
                // Unknown format -> next;
                return;

            // Collect all DDS (or TMX)
            var ddsGFs = gf.GetAllObjectFiles(FormatEnum.DDS).ToArray();
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