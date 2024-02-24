using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Maths
{
    public static class MathsHelper
    {
        public static Func<T> WeightedRandomPick<T>(List<T> vals, List<double> weights)
        {
            Random random = new Random();
            double weightSum = weights.Sum();
            List<double> weightsNormalised = weights.ConvertAll(w => w / weightSum);
            List<double> weightsCumSum = new List<double>();
            double sum = 0;
            foreach (double wn in weightsNormalised)
            {
                weightsCumSum.Add(wn + sum);
                sum += wn;
            }

            return () => vals[weightsCumSum.FindIndex(w => w >= random.NextDouble())];
            
        }

        public static Func<Point3d, int> PointLineRelation(Line line)
        {
            var lineEquation = LineEquation(line);
            if (double.IsNaN(lineEquation.Invoke(0)) || double.IsInfinity(lineEquation.Invoke(0)))
            {
                return p => (p.X > line.From.X) ? 1 : (p.X == line.From.X) ? 0 : -1;
            } else
            {
                return p => (p.Y > lineEquation.Invoke(p.X)) ? 1 : (p.Y == lineEquation.Invoke(p.X)) ? 0 : -1;
            }
        }

        public static Func<double, double> LineEquation(Line line)
        {
            double dy = line.To.Y - line.From.Y;
            double dx = line.To.X - line.From.X;
            if (dx == 0)
            {
                return x => (x == line.From.X) ? double.PositiveInfinity : double.NaN;
            } else
            {
                return x => (x - line.From.X) * dy / dx + line.From.Y;
            }
        }
    }

    public class Distribution
    {
        protected double s;
        public double Sigma => s;

        protected double m;
        public double Mu => m;

        protected Func<double, double> pdf;

        protected Func<double, double> decay;

        protected Distribution(double sigma, double mu)
        {
            s = sigma;
            m = mu;
        }

        public static Distribution Gaussian(double sigma, double mu)
        {
            Distribution d = new Distribution(sigma, mu);
            d.pdf = x => Math.Exp(-0.5 * Math.Pow((x - d.Mu) / d.Sigma, 2)) / d.Sigma / 2.50662827463; // sqrt(2*pi) is simplified as 2.50662827463
            d.decay = x => Math.Exp(-0.5 * Math.Pow((x - d.Mu) / d.Sigma, 2));
            return d;
        }

        public static Distribution GaussianDecay(double decayedTarget, double mu)
        {
            return Gaussian(decayedTarget / 3, mu); // assume the 3-sigma rule (99.7%)
        }

        public double Decay(double x)
        {
            return decay.Invoke(x);
        }

        public double PDF(double x)
        {
            return pdf.Invoke(x);
        }
    }

    public class Fitness
    {

    }
}
