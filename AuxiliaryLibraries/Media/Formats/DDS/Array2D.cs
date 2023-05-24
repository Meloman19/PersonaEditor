using System;

namespace AuxiliaryLibraries.Media.Formats.DDS
{
    // TODO: .net 7 -> change to span2d
    public readonly ref struct Array2D<T>
    {
        private readonly T[] _data;
        private readonly int _dim0;
        private readonly int _dim1;

        public Array2D(T[] data, int dim0, int dim1)
        {
            if (dim0 * dim1 != data.Length)
                throw new ArgumentException();

            _data = data;
            _dim0 = dim0;
            _dim1 = dim1;
        }

        public int Dim0 => _dim0;

        public int Dim1 => _dim1;

        public T this[int i0, int i1]
        {
            get => _data[i0 * _dim1 + i1];
            set => _data[i0 * _dim1 + i1] = value;
        }
    }
}