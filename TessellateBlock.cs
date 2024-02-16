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
