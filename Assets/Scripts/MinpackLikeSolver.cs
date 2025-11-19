using System;

public static class MinpackLikeSolver
{    public delegate void VectorFunction(double[] x, double[] f);

    public static bool Solve(
        VectorFunction func,
        double[] x,
        int maxIterations,
        double tol,
        double lambda0,
        double stepRel,
        out int iterations)
    {
        int n = x.Length;
        double[] f = new double[n];
        double[] fNew = new double[n];
        double[,] J = new double[n, n];
        double[,] Jscaled = new double[n, n];
        double[] s = new double[n];
        double[,] JTJ = new double[n, n];
        double[] JTF = new double[n];
        double[] pScaled = new double[n];
        double[] p = new double[n];
        double[] xNew = new double[n];

        iterations = 0;
        double lambda = lambda0;

        func(x, f);
        double phi = 0.5 * Norm2Squared(f);

        for (int it = 0; it < maxIterations; it++)
        {
            iterations = it + 1;

            if (Math.Sqrt(2.0 * phi) < tol)
                return true;

            for (int i = 0; i < n; i++)
                s[i] = Math.Max(Math.Abs(x[i]), 1.0);

            ComputeJacobian(func, x, f, J, stepRel);

            for (int j = 0; j < n; j++)
            {
                double sj = s[j];
                for (int i = 0; i < n; i++)
                    Jscaled[i, j] = J[i, j] * sj;
            }

            for (int i = 0; i < n; i++)
            {
                JTF[i] = 0.0;
                for (int j = 0; j < n; j++)
                    JTJ[i, j] = 0.0;
            }

            for (int i = 0; i < n; i++)
            {
                double fi = f[i];
                for (int j = 0; j < n; j++)
                {
                    double Jij = Jscaled[i, j];
                    JTF[j] += Jij * fi;
                    for (int k = 0; k < n; k++)
                        JTJ[j, k] += Jij * Jscaled[i, k];
                }
            }

            for (int i = 0; i < n; i++)
            {
                pScaled[i] = -JTF[i];
                JTJ[i, i] += lambda;
            }

            bool ok = SolveLinearSystem(JTJ, pScaled);
            if (!ok) return false;

            double normP2 = 0.0;
            for (int i = 0; i < n; i++)
            {
                p[i] = s[i] * pScaled[i];
                normP2 += p[i] * p[i];
            }

            if (Math.Sqrt(normP2) < tol)
                return true;

            for (int i = 0; i < n; i++)
                xNew[i] = x[i] + p[i];

            func(xNew, fNew);
            double phiNew = 0.5 * Norm2Squared(fNew);

            double ared = phi - phiNew;
            double pred = 0.0;
            for (int i = 0; i < n; i++)
                pred += -pScaled[i] * JTF[i] + 0.5 * pScaled[i] * pScaled[i] * lambda;

            double rho = (pred > 0.0) ? (ared / pred) : 0.0;

            if (rho > 1e-4 && phiNew < phi)
            {
                for (int i = 0; i < n; i++)
                {
                    x[i] = xNew[i];
                    f[i] = fNew[i];
                }
                phi = phiNew;

                lambda *= 0.3;
                if (lambda < 1e-12) lambda = 1e-12;
            }
            else
            {
                lambda *= 10.0;
                if (lambda > 1e12)
                    return false;
            }
        }

        return false;
    }

    private static void ComputeJacobian(
        VectorFunction func,
        double[] x,
        double[] f,
        double[,] J,
        double stepRel)
    {
        int n = x.Length;
        double[] xTemp = new double[n];
        double[] fTemp = new double[n];

        for (int j = 0; j < n; j++)
            xTemp[j] = x[j];

        for (int j = 0; j < n; j++)
        {
            double xj = x[j];
            double h = stepRel * (Math.Abs(xj) + 1.0);
            xTemp[j] = xj + h;

            func(xTemp, fTemp);

            for (int i = 0; i < n; i++)
                J[i, j] = (fTemp[i] - f[i]) / h;

            xTemp[j] = xj;
        }
    }

    private static bool SolveLinearSystem(double[,] A, double[] b)
    {
        int n = b.Length;

        for (int k = 0; k < n; k++)
        {
            int pivot = k;
            double maxVal = Math.Abs(A[k, k]);

            for (int i = k + 1; i < n; i++)
            {
                double val = Math.Abs(A[i, k]);
                if (val > maxVal)
                {
                    maxVal = val;
                    pivot = i;
                }
            }

            if (maxVal < 1e-14)
                return false;

            if (pivot != k)
            {
                for (int j = 0; j < n; j++)
                {
                    double tmp = A[k, j];
                    A[k, j] = A[pivot, j];
                    A[pivot, j] = tmp;
                }
                double tmpB = b[k];
                b[k] = b[pivot];
                b[pivot] = tmpB;
            }

            for (int i = k + 1; i < n; i++)
            {
                double factor = A[i, k] / A[k, k];
                A[i, k] = 0.0;

                for (int j = k + 1; j < n; j++)
                    A[i, j] -= factor * A[k, j];

                b[i] -= factor * b[k];
            }
        }

        for (int i = n - 1; i >= 0; i--)
        {
            double sum = b[i];
            for (int j = i + 1; j < n; j++)
                sum -= A[i, j] * b[j];

            if (Math.Abs(A[i, i]) < 1e-14)
                return false;

            b[i] = sum / A[i, i];
        }

        return true;
    }

    private static double Norm2Squared(double[] v)
    {
        double s = 0.0;
        for (int i = 0; i < v.Length; i++)
            s += v[i] * v[i];
        return s;
    }
}