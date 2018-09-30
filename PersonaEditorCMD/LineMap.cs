using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PersonaEditorCMD
{
    public class LineMap
    {
        public enum Type
        {
            FileName,
            MSGindex,
            MSGname,
            StringIndex,
            OldText,
            NewText,
            OldName,
            NewName
        }

        Dictionary<Type, int> dic = new Dictionary<Type, int>();

        public LineMap(string map)
        {
            string[] temp = Regex.Split(map, " ");

            dic.Add(Type.FileName, Array.IndexOf(temp, "%FN"));
            dic.Add(Type.MSGindex, Array.IndexOf(temp, "%MSGIND"));
            dic.Add(Type.MSGname, Array.IndexOf(temp, "%MSGNM"));
            dic.Add(Type.StringIndex, Array.IndexOf(temp, "%STRIND"));
            dic.Add(Type.OldText, Array.IndexOf(temp, "%OLDSTR"));
            dic.Add(Type.NewText, Array.IndexOf(temp, "%NEWSTR"));
            dic.Add(Type.OldName, Array.IndexOf(temp, "%OLDNM"));
            dic.Add(Type.NewName, Array.IndexOf(temp, "%NEWNM"));

            MinLength = dic.Max(x => x.Value) + 1;
        }

        public List<Type> GetList()
        {
            return dic.Where(x => x.Value >= 0).OrderBy(x => x.Value).Select(x => x.Key).ToList();
        }

        public int this[Type type] { get { return dic[type]; } }

        public int MinLength { get; }

        public bool CanGetText => dic[Type.FileName] >= 0 & (dic[Type.MSGindex] >= 0 | dic[Type.MSGname] >= 0) & dic[Type.StringIndex] >= 0 & dic[Type.NewText] >= 0;

        public bool CanGetName => dic[Type.OldName] >= 0 & dic[Type.NewName] >= 0;

        public bool CanGetNameMSG => dic[Type.FileName] >= 0 & (dic[Type.MSGindex] >= 0 | dic[Type.MSGname] >= 0) & dic[Type.NewName] >= 0;

        public bool CanGetString => dic[Type.OldText] >= 0 & dic[Type.NewText] >= 0;
    }
}
