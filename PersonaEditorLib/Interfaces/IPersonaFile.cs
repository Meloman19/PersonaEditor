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
            new Tuple<FileType, string>(FileType.BIN, "BIN file (*.BIN)|*.BIN"),
            new Tuple<FileType, string>(FileType.SPR, "SPR file (*.SPR)|*.SPR"),
            new Tuple<FileType, string>(FileType.TMX, "TMX file (*.TMX)|*.TMX|PNG file (*.PNG)|*.PNG"),
            new Tuple<FileType, string>(FileType.BF, "BF file (*.BF)|*.BF"),
            new Tuple<FileType, string>(FileType.BMD, "BMD file (*.BMD)|*.BMD"),
            new Tuple<FileType, string>(FileType.PTP, "Persona Text Project (*.PTP)|*.PTP")
        };
    }

    public enum FileType
    {
        HEX,
        BIN,
        SPR,
        TMX,
        BF,
        BMD,
        PTP
    }

    public enum ContextMenuItems
    {
        Export,
        Import,
        Separator
    }

    public class TreeItem
    {
        public string Name { get; set; } = "";
        public byte[] Data { get; set; }
        public FileType Type { get; set; } = FileType.HEX;
    }

    public interface IPersonaFile
    {
        string Name { get; }
        FileType Type { get; }

        List<object> GetSubFiles();
        bool Replace(object a);

        List<ContextMenuItems> ContextMenuList { get; }
        Dictionary<string, object> GetProperties { get; }
    }
}