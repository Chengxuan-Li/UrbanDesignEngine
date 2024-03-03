using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Maths
{
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
                    }
                    else
                    {
                        i++;
                    }
                }
                else if (relation == IntervalRelation.Touches || relation == IntervalRelation.Within)
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
                }
                else if (relation == IntervalRelation.Within)
                {
                    Intervals.RemoveAt(i);
                }
                else if (relation == IntervalRelation.Identical)
                {
                    Intervals.RemoveAt(i);
                    break;
                }
                else if (relation == IntervalRelation.Touches)
                {
                    double min = Intervals[i].Min;
                    double max = Intervals[i].Max;
                    if (min < intervalToRemove.Min)
                    {
                        Intervals.RemoveAt(i);
                        Intervals.Insert(i, new SolutionInterval(min, intervalToRemove.Min));
                        i++;
                    }
                    else
                    {
                        Intervals.RemoveAt(i);
                        Intervals.Insert(i, new SolutionInterval(intervalToRemove.Max, max));
                        break;
                    }
                }
                else // relation == IntervalRelation.Contains
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

}
