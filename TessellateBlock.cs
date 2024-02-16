using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using UrbanDesignEngine.Algorithms;
using UrbanDesignEngine.Components;
using UrbanDesignEngine.Constraints;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.Growth;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.Maths;
using UrbanDesignEngine.Utilities;

namespace UrbanDesignEngine
{
    public class TessellateBlock
    {
        public List<Polyline> SidePlotBoundaries;
        public List<Polyline> SideBlockBoundaries;
        

        public TessellateBlock(List<Polyline> sidePlotBoundaries)
        {
            SidePlotBoundaries = sidePlotBoundaries;
            SideBlockBoundaries = new List<Polyline>();
            sidePlotBoundaries.ForEach(b => SideBlockBoundaries.Add(new Polyline(new List<Point3d> { b.First, b.Last })));
        }
        public TessellateBlock(List<Polyline> sidePlotBoundaries, List<Polyline> sideBlockBoundaries)
        {
            SidePlotBoundaries = sidePlotBoundaries;
            SideBlockBoundaries = sideBlockBoundaries;
        }

        public void Solve()
        {


        }
    }

    public class PlotsBoundarySide
    {
        public Polyline PlotsBoundary;
        public Polyline BlockBoundary;

        public List<Line> PlotsBoundarySegments => PlotsBoundary.GetSegments().ToList();

        public List<double> ProjectedLengths
        {
            get
            {
                List<double> lengths = new List<double>();
                Vector3d vec = new Vector3d(-BlockBoundary.First + BlockBoundary.Last);
                foreach(Line line in PlotsBoundarySegments)
                {
                    double angle = Vector3d.VectorAngle(vec, new Vector3d(-line.From + line.To));
                    lengths.Add(line.Length * Math.Cos(angle));
                }
                return lengths;
            }
        }

        public double ProjectedLengthSum => ProjectedLengths.Sum();

        public List<double> ProjectedLengthsCumulated
        {
            get
            {
                List<double> lengths = new List<double>();
                double sum = 0;
                foreach(double length in ProjectedLengths)
                {
                    lengths.Add(length + sum);
                    sum = sum + length;
                }
                return lengths;
            }
        }

        public Point3d LengthToPoint(double length)
        {
            int index = ProjectedLengthsCumulated.FindIndex(x => x >= length);
            double diff = length - (ProjectedLengthsCumulated[index] - ProjectedLengths[index]);
            Vector3d direction = PlotsBoundarySegments[index].Direction;
            direction.Unitize();
            return PlotsBoundarySegments[index].From + direction * diff;
        }

        //temporary
        public List<double> GenerateBreakParams(List<double> lengthsCumSum)
        {
            List<double> parameters = new List<double>();
            double minSep = 15.0;
            double maxSep = 50;
            double snapDist = 5.0;
            Random random = new Random();
            bool pass = false;
            int iterations = 0;
            int maxIterations = 30;
            Predicate<List<double>> minSepCheck = (dd) =>
            {
                var d = dd.ToList();
                d.Sort();
                if (d.Count >= 1)
                {
                    List<double> dplus = new List<double>();
                    dplus.Add(0);
                    dplus.AddRange(d);
                    dplus.Add(lengthsCumSum[lengthsCumSum.Count - 1]);

                    for (int i = 0; i < dplus.Count - 1; i++)
                    {
                        if (dplus[i + 1] - dplus[i] < minSep)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            };
            Predicate<List<double>> maxSepCheck = (dd) =>
            {
                var d = dd.ToList();
                d.Sort();
                if (d.Count >= 1)
                {
                    List<double> dplus = new List<double>();
                    dplus.Add(0);
                    dplus.AddRange(d);
                    dplus.Add(lengthsCumSum[lengthsCumSum.Count - 1]);

                    for (int i = 0; i < dplus.Count - 1; i++)
                    {
                        if (dplus[i + 1] - dplus[i] > maxSep)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            };
            Func<double> prop = () =>
            {
                double p = random.NextDouble() * lengthsCumSum[lengthsCumSum.Count - 1];
                int index = lengthsCumSum.FindIndex(l => Math.Abs(l - p) <= snapDist);
                return (index == -1) ? p : lengthsCumSum[index];
            };
            List<double> states = new List<double>();
            Exploration<double>(states, prop, minSepCheck, maxSepCheck);
            states.Sort();
            return states;
        }

        public static bool Exploration<T>(List<T> states, Func<T> propagation /*add input in func*/, Predicate<List<T>> runtimeCompliance, Predicate<List<T>> resultCompliance)
        {
            int maxAttempts = 20;
            int attempt = 0;
            while (attempt < maxAttempts)
            {
                states.Add(propagation.Invoke());
                if (runtimeCompliance.Invoke(states))
                {
                    if (resultCompliance.Invoke(states))
                    {
                        return true;
                    }
                    if (Exploration<T>(states, propagation, runtimeCompliance, resultCompliance))
                    {
                        return true;
                    }
                }
                states.RemoveAt(states.Count - 1);
                attempt++;
            }
            return false;
        }


    }

    public static class GeometryHelper
    {
        public static void LinesPrincipleDirections(List<Line> lines, out List<double> Angles, out List<double> Weights)
        {
            // make continuous later (aggregation of probability distribution functions)
            List<double> originalLengths = new List<double>();
            List<double> allLengthsDuplicated = new List<double>();
            lines.ForEach(l => originalLengths.Add(l.From.DistanceTo(l.To) / 4));

            for (int i = 0; i < 4; i ++)
            {
                allLengthsDuplicated.AddRange(originalLengths.ToList());
            }

            List<double> originalAngles = new List<double>();
            List<double> allAnglesDuplicated = new List<double>();
            lines.ForEach(l => originalAngles.Add(Trigonometry.Angle(l.From, l.To)));

            allAnglesDuplicated.AddRange(originalAngles.ToList());
            allAnglesDuplicated.AddRange(originalAngles.ConvertAll(x => x >= Math.PI ? x - Math.PI : x + Math.PI)); // + pi
            allAnglesDuplicated.AddRange(originalAngles.ConvertAll(x => x >= Math.PI / 2 ? x - Math.PI / 2 : x + Math.PI * 3 / 2)); // - pi/2
            allAnglesDuplicated.AddRange(originalAngles.ConvertAll(x => x >= Math.PI * 3 / 2 ? x - Math.PI * 3 / 2 : x + Math.PI / 2)); // + pi/2

            List<double> angles = new List<double>();
            List<double> weights = new List<double>();
            double identicalAngleTolerance = 0.1 / 180 * Math.PI;
            double combineAngleTolerance = 15.0 / 180 * Math.PI;
            for(int i = 0; i < allAnglesDuplicated.Count; i++)
            {
                double angle = allAnglesDuplicated[i];
                int index = angles.FindIndex(a => Trigonometry.AngleDifference(angle, a) <= identicalAngleTolerance);
                if (index == -1)
                {
                    angles.Add(angle);
                    weights.Add(allLengthsDuplicated[i]);
                } else
                {
                    weights[index] = weights[index] + allLengthsDuplicated[i];
                }
            }
            int pos = 0;
            while (pos < angles.Count)
            {
                int index = angles.FindIndex(a => pos != angles.IndexOf(a) && Trigonometry.AngleDifference(angles[pos], a) <= combineAngleTolerance);
                if (index == -1)
                {
                    pos++;
                } else
                {
                    if (weights[pos] >= weights[index])
                    {
                        weights[pos] = weights[pos] + weights[index];
                        weights.RemoveAt(index);
                        angles.RemoveAt(index);
                        pos++;
                    } else
                    {
                        weights[index] = weights[index] + weights[pos];
                        weights.RemoveAt(pos);
                        angles.RemoveAt(pos);
                    }
                }
            }
            Angles = angles;
            Weights = weights;
        }
    }



}
