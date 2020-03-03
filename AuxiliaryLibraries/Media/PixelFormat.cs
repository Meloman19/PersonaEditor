namespace AuxiliaryLibraries.Media
{
    public struct PixelFormat
    {
        public int BitsPerPixel { get; }
        public bool IsIndexed { get; }
        internal PixelFormatEnum Format { get; }

        internal PixelFormat(PixelFormatEnum pixelFormatEnum)
        {
            BitsPerPixel = PixelFormatHelper.GetPixelFormatBPP(pixelFormatEnum);
            IsIndexed = PixelFormatHelper.GetPixelFormatIndexed(pixelFormatEnum);
            Format = pixelFormatEnum;
        }
        
        public override bool Equals(object obj)
        {
            PixelFormat pixelFormat = (PixelFormat)obj;
            return Format == pixelFormat.Format;
            //return (format == pixelFormat.format) && (BitsPerPixel == pixelFormat.BitsPerPixel) && (IsIndexed == pixelFormat.IsIndexed);
        }

        public override int GetHashCode()
        {
            return Format.GetHashCode();
        }

        public override string ToString()
        {
            return Format.ToString();
        }

        public static bool operator ==(PixelFormat a, PixelFormat b) => a.Equals(b);
        public static bool operator !=(PixelFormat a, PixelFormat b) => !(a == b);
    }
}