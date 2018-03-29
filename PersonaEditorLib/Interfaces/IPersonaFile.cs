using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace PersonaEditorLib.Interfaces
{


    public enum FileType
    {
        Unknown,
        BIN,
        SPR,
        TMX,
        BF,
        PM1,
        BMD,
        PTP,
        FNT,
        BVP,
        TBL,
        DAT,
        StringList
    }

    public enum ContextMenuItems
    {
        Edit,
        SaveAs,
        SaveAll,
        Replace,
        Separator
    }

    public interface IPersonaFile : IFile
    {
        FileType Type { get; }
        List<ObjectFile> GetSubFiles();
        Dictionary<string, object> GetProperties { get; }
    }
}