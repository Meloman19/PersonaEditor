using System.Text;

namespace PersonaEditorLib
{
    internal static class Static
    {
        static Static()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ShiftJIS = Encoding.GetEncoding(932);
        }

        public static Encoding ShiftJIS { get; }
    }
}