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
        //public static Func<double> TruncatedSolutionSpaceRandomGeneration(Interval fullSolutionSpace, List<Interval> )

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

        public static Func<T> WeightedRandomPick<T>(List<T> vals, List<double> weights, Random random)
        {
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

        public static Func<double, double> LinearIntepolation(double x1, double y1, double x2, double y2)
        {
            double k = (y2 - y1) / (x2 - x1);
            return x => y1 + k * (x - x1);
        }

        public static Func<double, double> FiniteIntegral(Func<double, double> func, double start, double end, double step)
        {
            // needs validation
            List<double> xs = new List<double>();
            List<double> ys = new List<double>();

            if (end < start)
            {
                double d = end;
                end = start;
                start = d;
            }

            double current = start;
            while (current < end)
            {
                xs.Add(current);
                ys.Add(func.Invoke(current));
                current += step;
            }

            List<double> cumYs = new List<double>();
            double sum = 0;
            for (int i = 0; i < ys.Count; i++)
            {
                sum = sum + ys[i];
                cumYs.Add(sum * step);
            }

            return x =>
            {
                if (x <= start)
                {
                    return cumYs[0];
                }
                else if (x >= end)
                {
                    return cumYs.Last();
                }
                else
                {
                    for (int i = 0; i < xs.Count; i++)
                    {
                        if (xs[i] >= x)
                        {
                            return i == 0 ? cumYs[0] : cumYs[i - 1];
                        }
                    }
                    return cumYs.Last();
                }
            };

        }
    }


}
