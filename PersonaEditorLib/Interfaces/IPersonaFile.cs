using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        FTD,
        DDS,
        SPD,
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
        List<ObjectFile> SubFiles { get; }
        ReadOnlyObservableCollection<PropertyClass> GetProperties { get; }
    }
}