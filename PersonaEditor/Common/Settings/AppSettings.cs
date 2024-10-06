using System;
using System.Windows.Media;

namespace PersonaEditor.Common.Settings
{
    public sealed class AppSettings : ICloneable
    {
        public string BMDFontDefault { get; set; } = "P5R_ENG";

        public string BMDFontDestDefault { get; set; } = "P5R_ENG";

        public string DefaultLocalization { get; set; } = "Default";

        public Color PreviewSelectedColor { get; set; } = Color.FromArgb(0, 0, 0, 0);

        public bool SaveAsPTP_CO2N { get; set; } = false;

        public string SaveAsPTP_Font { get; set; } = "Empty";

        public string OpenPTP_Font { get; set; } = "Empty";

        public bool SingleInstanceApplication { get; set; } = false;

        public string FTDEncoding { get; set; } = "Empty";

        public Color SPDEditorBackgroundColor { get; set; } = Color.FromArgb(0, 0, 0, 0);

        public Color SPDObjectBorderColor { get; set; } = Colors.Black;

        public Color SPDObjectSelectionColor { get; set; } = Color.FromArgb(0x80, 0xFF, 0x66, 0x00);

        public int SPDObjectBorderThickness { get; set; } = 1;

        public AppSettings Clone()
        {
            return (AppSettings)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
    }
}