using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure
{
    public class HEX : IPersonaFile
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

        public List<ObjectFile> GetSubFiles()
        {
            return new List<ObjectFile>();
        }
        
        public List<ContextMenuItems> ContextMenuList
        {
            get
            {
                List<ContextMenuItems> returned = new List<ContextMenuItems>();

                returned.Add(ContextMenuItems.SaveAs);
                returned.Add(ContextMenuItems.Replace);

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

        public int Size
        {
            get { return Data.Length; }
        }

        public byte[] Get()
        {
            return Data;
        }

        #endregion IFile

        public object Tag { get; set; }
    }
}