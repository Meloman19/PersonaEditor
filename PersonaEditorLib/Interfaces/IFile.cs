using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PersonaEditorLib.Interfaces
{   
    public interface IFile
    {
        int Size { get; }
        byte[] Get();
    }
}