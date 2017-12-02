using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.Interfaces
{
    public interface IPersonaFileElement
    {
        string Name { get; set; }
        IPersonaFile File { get;}
        int Size { get; }
    }
}