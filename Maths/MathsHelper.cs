using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Maths
{
    public enum IntervalRelation
    {
        Disjoint = 0,
        Touches = 1,
        Contains = 2,
        Within = 3,
        Identical = 9,
        Error = 999,
    }
    public class SolutionInterval
    {
        public double Min => a > b ? b : a;
        public double Max => a > b ? a : b;

        public double Length => Max - Min;

        double a;
        double b;

        public SolutionInterval(double p1, double p2)
        {
            a = p1;
            b = p2;
        }

        public bool Contains(SolutionInterval interval)
        {
            if (interval.Min >= Min && interval.Max <= Max)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public List<SolutionInterval> Union(SolutionInterval interval)
        {
            if (interval.Min > Max || interval.Max < Min)
            {
                return new List<SolutionInterval> { this, interval };
            } else
            {
                return new List<SolutionInterval> { new SolutionInterval(Math.Min(Min, interval.Min), Math.Max(Max, interval.Max)) };
            }
        }

        public IntervalRelation Relation(SolutionInterval interval)
        {
            if (interval.Min == Min && interval.Max == Max)
            {
                return IntervalRelation.Identical;
            }

            if (interval.Min > Max)
            {
                return IntervalRelation.Disjoint; 
            } else if (interval.Max < Min)
            {
                return IntervalRelation.Disjoint;
            } else if (interval.Max <= Max && interval.Min <= Min)
            {
                return IntervalRelation.Touches;
            } else if (interval.Min >= Min && interval.Max >= Max)
            {
                return IntervalRelation.Touches;
            } else if (interval.Max >= Max && interval.Min <= Min)
            {
                return IntervalRelation.Within;
            } else if (interval.Max <= Max && interval.Min >= Min)
            {
                return IntervalRelation.Contains;
            } else
            {
                return IntervalRelation.Error;
            }
        }

        public double NextDouble(Random random)
        {
            return random.NextDouble() * Length + Min;
        }

        public override string ToString()
        {
            return String.Format("i[{0}, {1}]", Min, Max);
        }
    }

    public class MultiInterval
    {
        public List<SolutionInterval> Intervals = new List<SolutionInterval>();

        public List<double> IntervalBounds
        {
            get
            {
                List<double> ps = new List<double>();
                Intervals.ForEach(i =>
                {
                    ps.Add(i.Min);
                    ps.Add(i.Max);
                });
                return ps;
            }
        }

        public MultiInterval()
        {

        }

        public MultiInterval(SolutionInterval interval)
        {
            Intervals.Add(interval);
        }
        
        public void Union(MultiInterval multiIntervalToUnion)
        {
            if (Intervals.Count == 0)
            {
                Intervals = multiIntervalToUnion.Intervals.ToList();
                return;
            }
            foreach (var interval in multiIntervalToUnion.Intervals)
            {
                Union(interval);
            }
        }

        public void Union(SolutionInterval intervalToUnion)
        {
            if (Intervals.Count == 0)
            {
                Intervals.Add(intervalToUnion);
                return;
            }

            int i = 0;
            bool added = false;
            while (i < Intervals.Count)
            {
                IntervalRelation relation = Intervals[i].Relation(intervalToUnion);
                if (relation == IntervalRelation.Contains || relation == IntervalRelation.Identical)
                {
                    added = true;
                    break;
                }

                if (relation == IntervalRelation.Disjoint)
                {
                    if (intervalToUnion.Max < Intervals[i].Min)
                    {
                        Intervals.Insert(i, intervalToUnion);
                        added = true;
                        break;
                    } else
                    {
                        i++;
                    }
                } else if (relation == IntervalRelation.Touches || relation == IntervalRelation.Within)
                {
                    intervalToUnion = intervalToUnion.Union(Intervals[i])[0];
                    Intervals.RemoveAt(i);
                }
            }
            if (!added)
            {
                Intervals.Add(intervalToUnion);
            }
        }

        public void Subtraction(SolutionInterval intervalToRemove)
        {
            int i = 0;
            while (i < Intervals.Count)
            {
                IntervalRelation relation = Intervals[i].Relation(intervalToRemove);
                if (relation == IntervalRelation.Disjoint)
                {
                    i++;
                } else if (relation == IntervalRelation.Within)
                {
                    Intervals.RemoveAt(i);
                } else if (relation == IntervalRelation.Identical)
                {
                    Intervals.RemoveAt(i);
                    break;
                } else if (relation == IntervalRelation.Touches)
                {
                    double min = Intervals[i].Min;
                    double max = Intervals[i].Max;
                    if (min < intervalToRemove.Min)
                    {
                        Intervals.RemoveAt(i);
                        Intervals.Insert(i, new SolutionInterval(min, intervalToRemove.Min));
                        i++;
                    } else
                    {
                        Intervals.RemoveAt(i);
                        Intervals.Insert(i, new SolutionInterval(intervalToRemove.Max, max));
                        break;
                    }
                } else // relation == IntervalRelation.Contains
                {
                    double min = Intervals[i].Min;
                    double max = Intervals[i].Max;
                    Intervals.RemoveAt(i);
                    Intervals.Insert(i, new SolutionInterval(min, intervalToRemove.Min));
                    Intervals.Insert(i + 1, new SolutionInterval(intervalToRemove.Max, max));
                    break;
                }
            }
        }

        public void Subtraction(MultiInterval multiInterval)
        {
            foreach (var interval in multiInterval.Intervals)
            {
                Subtraction(interval);
            }
        }

        public double NextRandomDouble(Random random)
        {
            List<double> weights = new List<double>();
            Intervals.ForEach(i => weights.Add(i.Length));
            var generator = MathsHelper.WeightedRandomPick(Intervals, weights, random);
            var interval = generator.Invoke();
            return interval.NextDouble(random);
        }

        public override string ToString()
        {
            return String.Join(", ", Intervals.ToList());
        }
    }

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
