using System;
using System.Text;

namespace PersonaEditorLib.Text
{
    public struct TextBaseElement
    {
        public TextBaseElement(bool isText, byte[] array)
        {
            Array = array;
            IsText = isText;
        }

        public string GetText(Encoding encoding, bool linesplit = false)
        {
            if (IsText)
                return String.Concat(encoding.GetChars(Array));
            else
            {
                if (Array[0] == 0x0A)
                    if (linesplit)
                        return "\n";
                    else
                        return GetSystem();
                else
                    return GetSystem();
            }
        }

        public string GetSystem()
        {
            string returned = "";

            if (Array.Length > 0)
            {
                returned += "{" + Convert.ToString(Array[0], 16).PadLeft(2, '0').ToUpper();
                for (int i = 1; i < Array.Length; i++)
                    returned += " " + Convert.ToString(Array[i], 16).PadLeft(2, '0').ToUpper();

                returned += "}";
            }

            return returned;
        }

        public bool IsText { get; }
        public byte[] Array { get; }
    }
}