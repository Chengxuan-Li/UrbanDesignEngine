using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Maths
{
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
            }
            else
            {
                return false;
            }
        }

        public List<SolutionInterval> Union(SolutionInterval interval)
        {
            if (interval.Min > Max || interval.Max < Min)
            {
                return new List<SolutionInterval> { this, interval };
            }
            else
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
            }
            else if (interval.Max < Min)
            {
                return IntervalRelation.Disjoint;
            }
            else if (interval.Max <= Max && interval.Min <= Min)
            {
                return IntervalRelation.Touches;
            }
            else if (interval.Min >= Min && interval.Max >= Max)
            {
                return IntervalRelation.Touches;
            }
            else if (interval.Max >= Max && interval.Min <= Min)
            {
                return IntervalRelation.Within;
            }
            else if (interval.Max <= Max && interval.Min >= Min)
            {
                return IntervalRelation.Contains;
            }
            else
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


}
