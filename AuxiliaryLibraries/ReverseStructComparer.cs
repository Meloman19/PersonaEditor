using System.Collections.Generic;

namespace AuxiliaryLibraries
{
    public class ReverseStructComparer<T> : IEqualityComparer<T> where T : struct
    {
        public bool Equals(T x, T y)
        {
            if (x.Equals(y))
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