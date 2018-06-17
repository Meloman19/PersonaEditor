using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib
{
    public struct StreamPart
    {
        public Stream Stream { get; }
        public long Size { get; }
        public long Position { get; }

        public bool CanRead => Size >= 0 ? true : false;

        public StreamPart(Stream stream, long size, long offset)
        {
            Stream = stream;
            Size = size;
            Position = offset;
        }
    }
}
