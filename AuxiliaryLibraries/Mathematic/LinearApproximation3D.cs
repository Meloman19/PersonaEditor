// David Eberly, Geometric Tools, Redmond WA 98052
// Copyright (c) 1998-2018
// Distributed under the Boost Software License, Version 1.0.
// http://www.boost.org/LICENSE_1_0.txt
// http://www.geometrictools.com/License/Boost/LICENSE_1_0.txt
// File Version: 3.0.0 (2016/06/19)
// https://www.geometrictools.com/GTEngine/Include/Mathematics/GteApprOrthogonalLine3.h
// https://www.geometrictools.com/GTEngine/Include/Mathematics/GteSymmetricEigensolver3x3.h
// Port to C# - Meloman19

using System;

namespace AuxiliaryLibraries.Mathematic
{
    static class LinearApproximation3D
    {
        public static bool Start(double[,] data, out double[] start, out double[] direction)
        {
            int numPoints = data.GetLength(0);

            double[] mean = new double[3];
            for (int i = 0; i < numPoints; i++)
            {
                mean[0] += data[i, 0];
                mean[1] += data[i, 1];
                mean[2] += data[i, 2];
            }
            double invSize = (double)1 / (double)numPoints;
            mean[0] *= invSize;
            mean[1] *= invSize;
            mean[2] *= invSize;

            double covar00 = 0, covar01 = 0, covar02 = 0, covar11 = 0, covar12 = 0, covar22 = 0;
            for (int i = 0; i < numPoints; i++)
            {
                double[] diff = new double[3]
                {
                    data[i, 0] - mean[0],
                    data[i, 1] - mean[1],
                    data[i, 2] - mean[2]
                };

                covar00 += diff[0] * diff[0];
                covar01 += diff[0] * diff[1];
                covar02 += diff[0] * diff[2];
                covar11 += diff[1] * diff[1];
                covar12 += diff[1] * diff[2];
                covar22 += diff[2] * diff[2];
            }
            covar00 *= invSize;
            covar01 *= invSize;
            covar02 *= invSize;
            covar11 *= invSize;
            covar12 *= invSize;
            covar22 *= invSize;

            int a = Make(covar00, covar01, covar02, covar11, covar12, covar22, true, +1, out double[] eval, out double[,] evec);

            start = mean;
            direction = new double[] { evec[2, 0], evec[2, 1], evec[2, 2] };

            return eval[1] < eval[2];
        }

        static int Make(double a00, double a01, double a02, double a11, double a12, double a22, bool aggressive, int sortType, out double[] eval, out double[,] evec)
        {
            const double zero = 0;
            const double one = 1;
            const double half = 0.5;

            bool isRotation = false;

            GetCosSin(a12, -a02, out double c, out double s);

            double[,] Q = { { c, s, zero }, { s, -c, zero }, { zero, zero, one } };
            double term0 = c * a00 + s * a01;
            double term1 = c * a01 + s * a11;
            double b00 = c * term0 + s * term1;
            double b01 = s * term0 - c * term1;
            term0 = s * a00 - c * a01;
            term1 = s * a01 - c * a11;
            double b11 = s * term0 - c * term1;
            double b12 = s * a02 - c * a12;
            double b22 = a22;

            const int maxIteration = 2074;
            int iteration;
            double c2, s2;

            if (Math.Abs(b12) <= Math.Abs(b01))
            {
                double saveB00, saveB01, saveB11;
                for (iteration = 0; iteration < maxIteration; ++iteration)
                {
                    GetCosSin(half * (b00 - b11), b01, out c2, out s2);
                    s = Math.Sqrt(half * (one - c2));
                    c = half * s2 / s;

                    Update0(Q, c, s);
                    isRotation = !isRotation;

                    saveB00 = b00;
                    saveB01 = b01;
                    saveB11 = b11;
                    term0 = c * saveB00 + s * saveB01;
                    term1 = c * saveB01 + s * saveB11;
                    b00 = c * term0 + s * term1;
                    b11 = b22;
                    term0 = c * saveB01 - s * saveB00;
                    term1 = c * saveB11 - s * saveB01;
                    b22 = c * term1 - s * term0;
                    b01 = s * b12;
                    b12 = c * b12;

                    if (Converged(aggressive, b00, b11, b01))
                    {
                        GetCosSin(half * (b00 - b11), b01, out c2, out s2);
                        s = Math.Sqrt(half * (one - c2));
                        c = half * s2 / s;

                        Update2(Q, c, s);
                        isRotation = !isRotation;

                        // Update D = Q^T*B*Q.
                        saveB00 = b00;
                        saveB01 = b01;
                        saveB11 = b11;
                        term0 = c * saveB00 + s * saveB01;
                        term1 = c * saveB01 + s * saveB11;
                        b00 = c * term0 + s * term1;
                        term0 = s * saveB00 - c * saveB01;
                        term1 = s * saveB01 - c * saveB11;
                        b11 = s * term0 - c * term1;
                        break;
                    }
                }
            }
            else
            {
                double saveB11, saveB12, saveB22;
                for (iteration = 0; iteration < maxIteration; ++iteration)
                {
                    // Compute the Givens reflection.
                    GetCosSin(half * (b22 - b11), b12, out c2, out s2);
                    s = Math.Sqrt(half * (one - c2));  // >= 1/sqrt(2)
                    c = half * s2 / s;

                    // Update Q by the Givens reflection.
                    Update1(Q, c, s);
                    isRotation = !isRotation;

                    // Update B <- Q^T*B*Q, ensuring that b02 is zero and |b12| has
                    // strictly decreased.  MODIFY...
                    saveB11 = b11;
                    saveB12 = b12;
                    saveB22 = b22;
                    term0 = c * saveB22 + s * saveB12;
                    term1 = c * saveB12 + s * saveB11;
                    b22 = c * term0 + s * term1;
                    b11 = b00;
                    term0 = c * saveB12 - s * saveB22;
                    term1 = c * saveB11 - s * saveB12;
                    b00 = c * term1 - s * term0;
                    b12 = s * b01;
                    b01 = c * b01;

                    if (Converged(aggressive, b11, b22, b12))
                    {
                        // Compute the Householder reflection.
                        GetCosSin(half * (b11 - b22), b12, out c2, out s2);
                        s = Math.Sqrt(half * (one - c2));
                        c = half * s2 / s;  // >= 1/sqrt(2)

                        // Update Q by the Householder reflection.
                        Update3(Q, c, s);
                        isRotation = !isRotation;

                        // Update D = Q^T*B*Q.
                        saveB11 = b11;
                        saveB12 = b12;
                        saveB22 = b22;
                        term0 = c * saveB11 + s * saveB12;
                        term1 = c * saveB12 + s * saveB22;
                        b11 = c * term0 + s * term1;
                        term0 = s * saveB11 - c * saveB12;
                        term1 = s * saveB12 - c * saveB22;
                        b22 = s * term0 - c * term1;
                        break;
                    }
                }
            }

            double[] diagonal = { b00, b11, b22 };
            int i0, i1, i2;
            if (sortType >= 1)
            {
                bool isOdd = Sort(diagonal, out i0, out i1, out i2);
                if (!isOdd)
                    isRotation = !isRotation;
            }
            else if (sortType <= -1)
            {
                bool isOdd = Sort(diagonal, out i0, out i1, out i2);
                int temp = i0;
                i0 = i2;
                i2 = temp;
                if (isOdd)
                    isRotation = !isRotation;
            }
            else
            {
                i0 = 0;
                i1 = 1;
                i2 = 2;
            }

            eval = new double[3];
            evec = new double[3, 3];

            eval[0] = diagonal[i0];
            eval[1] = diagonal[i1];
            eval[2] = diagonal[i2];
            evec[0, 0] = Q[0, i0];
            evec[0, 1] = Q[1, i0];
            evec[0, 2] = Q[2, i0];
            evec[1, 0] = Q[0, i1];
            evec[1, 1] = Q[1, i1];
            evec[1, 2] = Q[2, i1];
            evec[2, 0] = Q[0, i2];
            evec[2, 1] = Q[1, i2];
            evec[2, 2] = Q[2, i2];

            // Ensure the columns of Q form a right-handed set.
            if (!isRotation)
            {
                for (int j = 0; j < 3; ++j)
                {
                    evec[2, j] = -evec[2, j];
                }
            }

            return iteration;
        }

        static void GetCosSin(double u, double v, out double cs, out double sn)
        {
            double maxAbsComp = Math.Max(Math.Abs(u), Math.Abs(v));
            if (maxAbsComp > 0)
            {
                u /= maxAbsComp;  // in [-1,1]
                v /= maxAbsComp;  // in [-1,1]
                double length = Math.Sqrt(u * u + v * v);

                cs = u / length;
                sn = v / length;
                if (cs > 0)
                {
                    cs = -cs;
                    sn = -sn;
                }
            }
            else
            {
                cs = -1;
                sn = 0;
            }
        }

        static void Update0(double[,] Q, double c, double s)
        {
            for (int r = 0; r < 3; ++r)
            {
                double tmp0 = c * Q[r, 0] + s * Q[r, 1];
                double tmp1 = Q[r, 2];
                double tmp2 = c * Q[r, 1] - s * Q[r, 0];
                Q[r, 0] = tmp0;
                Q[r, 1] = tmp1;
                Q[r, 2] = tmp2;
            }
        }

        static void Update1(double[,] Q, double c, double s)
        {
            for (int r = 0; r < 3; ++r)
            {
                double tmp0 = c * Q[r, 1] - s * Q[r, 2];
                double tmp1 = Q[r, 0];
                double tmp2 = c * Q[r, 2] + s * Q[r, 1];
                Q[r, 0] = tmp0;
                Q[r, 1] = tmp1;
                Q[r, 2] = tmp2;
            }
        }

        static void Update2(double[,] Q, double c, double s)
        {
            for (int r = 0; r < 3; ++r)
            {
                double tmp0 = c * Q[r, 0] + s * Q[r, 1];
                double tmp1 = s * Q[r, 0] - c * Q[r, 1];
                Q[r, 0] = tmp0;
                Q[r, 1] = tmp1;
            }
        }

        static void Update3(double[,] Q, double c, double s)
        {
            for (int r = 0; r < 3; ++r)
            {
                double tmp0 = c * Q[r, 1] + s * Q[r, 2];
                double tmp1 = s * Q[r, 1] - c * Q[r, 2];
                Q[r, 1] = tmp0;
                Q[r, 2] = tmp1;
            }
        }

        static bool Converged(bool aggressive, double bDiag0, double bDiag1, double bSuper)
        {
            if (aggressive)
            {
                return bSuper == 0;
            }
            else
            {
                double sum = Math.Abs(bDiag0) + Math.Abs(bDiag1);
                return sum + Math.Abs(bSuper) == sum;
            }
        }

        static bool Sort(double[] d, out int i0, out int i1, out int i2)
        {
            bool odd;
            if (d[0] < d[1])
            {
                if (d[2] < d[0])
                {
                    i0 = 2; i1 = 0; i2 = 1; odd = true;
                }
                else if (d[2] < d[1])
                {
                    i0 = 0; i1 = 2; i2 = 1; odd = false;
                }
                else
                {
                    i0 = 0; i1 = 1; i2 = 2; odd = true;
                }
            }
            else
            {
                if (d[2] < d[1])
                {
                    i0 = 2; i1 = 1; i2 = 0; odd = false;
                }
                else if (d[2] < d[0])
                {
                    i0 = 1; i1 = 2; i2 = 0; odd = true;
                }
                else
                {
                    i0 = 1; i1 = 0; i2 = 2; odd = false;
                }
            }
            return odd;
        }
    }
}
