using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib
{
    class ReverseStructComparer<T> : IEqualityComparer<T> where T: struct
    {
        public bool Equals(T x, T y)
        {
            if(x.Equals(y))
                return false;
            else
                return true;
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
