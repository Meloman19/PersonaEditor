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
    public static class Info
    {
        public static List<Tuple<FileType, string>> FileInfo = new List<Tuple<FileType, string>>()
        {
            new Tuple<FileType, string>(FileType.HEX, "Raw Data (*.*)|*.*"),
            new Tuple<FileType, string>(FileType.BIN, "BIN file (*.BIN)|*.BIN|PAK file (*.PAK)|*.PAK"),
            new Tuple<FileType, string>(FileType.SPR, "SPR file (*.SPR)|*.SPR"),
            new Tuple<FileType, string>(FileType.TMX, "TMX file (*.TMX)|*.TMX|PNG file (*.PNG)|*.PNG"),
            new Tuple<FileType, string>(FileType.BF, "BF file (*.BF)|*.BF"),
            new Tuple<FileType, string>(FileType.BMD, "BMD file (*.BMD)|*.BMD"),
            new Tuple<FileType, string>(FileType.PTP, "Persona Text Project (*.PTP)|*.PTP"),
            new Tuple<FileType, string>(FileType.FNT, "Persona Font (*.FNT)|*.FNT"),
            new Tuple<FileType, string>(FileType.BIN, "BVP file (*.BVP)|*.BVP")
        };
    }

    public enum FileType
    {
        HEX,
        BIN,
        SPR,
        TMX,
        BF,
        PM1,
        BMD,
        PTP,
        FNT,
        BVP,
        ObjList
    }

    public enum ContextMenuItems
    {
        SaveAs,
        SaveAll,
        Replace,
        Separator
    }

    public interface IPersonaFile : IFile
    {
        FileType Type { get; }

        List<ObjectFile> GetSubFiles();

        List<ContextMenuItems> ContextMenuList { get; }
        Dictionary<string, object> GetProperties { get; }
    }
}