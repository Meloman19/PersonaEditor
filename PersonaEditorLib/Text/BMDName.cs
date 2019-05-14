using System;
using System.Collections.Generic;
using System.Text;

namespace PersonaEditorLib.Text
{
    public class BMDName
    {
        public BMDName(int Index, byte[] NameBytes)
        {
            this.Index = Index;
            this.NameBytes = NameBytes;
        }

        public int Index { get; set; }
        public byte[] NameBytes { get; set; }
    }
}