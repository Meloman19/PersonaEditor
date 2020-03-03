namespace AuxiliaryLibraries.Mathematic
{
    public static class GeometryOperation
    {
        public static double Distance(double[] a, double[] b)
        {
            double x = a[0] - b[0];
            double y = a[1] - b[1];
            double z = a[2] - b[2];

            return x * x + y * y + z * z;
        }

        public static double Distance(byte[] a, byte[] b)
        {

            double x = a[0] - b[0];
            double y = a[1] - b[1];
            double z = a[2] - b[2];

            return x * x + y * y + z * z;
        }

        public static double[] Projection(double[] startLine, double[] directionLine, double[] M)
        {
            double lambda = (directionLine[0] * (M[0] - startLine[0]) + directionLine[1] * (M[1] - startLine[1]) + directionLine[2] * (M[2] - startLine[2])) /
                (directionLine[0] * directionLine[0] + directionLine[1] * directionLine[1] + directionLine[2] * directionLine[2]);

            double[] projection = new double[3];

            projection[0] = startLine[0] + directionLine[0] * lambda;
            projection[1] = startLine[1] + directionLine[1] * lambda;
            projection[2] = startLine[2] + directionLine[2] * lambda;

            return projection;
        }

        public static double[] Projection(double[] startLine, double[] directionLine, double[,] M, int matrixIndex)
        {
            double lambda = (directionLine[0] * (M[matrixIndex, 0] - startLine[0]) + directionLine[1] * (M[matrixIndex, 1] - startLine[1]) + directionLine[2] * (M[matrixIndex, 2] - startLine[2])) /
                (directionLine[0] * directionLine[0] + directionLine[1] * directionLine[1] + directionLine[2] * directionLine[2]);

            double[] projection = new double[3];

            projection[0] = startLine[0] + directionLine[0] * lambda;
            projection[1] = startLine[1] + directionLine[1] * lambda;
            projection[2] = startLine[2] + directionLine[2] * lambda;

            return projection;
        }
    }
}