using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryLibraries.WPF.Interactivity
{
    public interface ICommandArgConverter
    {
        object GetArguments(object[] args);        
    }
}
