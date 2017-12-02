using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure
{
    class HEX : IPersonaFile, IFile
    {
        public byte[] Data { get; set; } = new byte[0];

        public HEX(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }

        #region IPersonaFile

        public string Name { get; private set; } = "";

        public FileType Type => FileType.HEX;

        public List<object> GetSubFiles()
        {
            return new List<object>();
        }

        public bool Replace(object newdata)
        {
            if (newdata is IPersonaFile pfile)
                if (pfile.Type == Type)
                    if (newdata is HEX hex)
                    {
                        Data = hex.Data;
                        return true;
                    }

            return false;
        }

        public List<ContextMenuItems> ContextMenuList
        {
            get
            {
                List<ContextMenuItems> returned = new List<ContextMenuItems>();

                returned.Add(ContextMenuItems.Export);
                returned.Add(ContextMenuItems.Import);

                return returned;
            }
        }

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Size", Size);
                returned.Add("Type", Type);

                return returned;
            }
        }

        #endregion IPersonaFile

        #region IFile

        public bool IsLittleEndian { get; set; } = true;

        public int Size
        {
            get { return Data.Length; }
        }

        public byte[] Get(bool IsLittleEndian)
        {
            return Data;
        }

        #endregion IFile
    }
}