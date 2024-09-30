using System.Text;

namespace PersonaEditorLib
{
    internal static class Static
    {
        static Static()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ShiftJIS = Encoding.GetEncoding("shift-jis");
        }

        public static Encoding ShiftJIS { get; }
    }
}