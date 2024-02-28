using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using UrbanDesignEngine.Maths;

namespace UrbanDesignEngine.Algorithms
{
    public class KellyMcCabeSubdivitionSetting
    {
        public double MaximumAreaTarget = 800;
        public double MinimumAreaTarget = 80;
        public double DivisibleThreshold = 8;

        public static KellyMcCabeSubdivitionSetting Default()
        {
            return new KellyMcCabeSubdivitionSetting();
        }
    }

    public enum KellyMcCabeModelResult
    {
        Invalid = 0,
        Continue = 99,
        Success = 1,
    }

    public class KellyMcCabeLotSubdivisionModel
    {
        public KellyMcCabeSubdivitionSetting Setting = KellyMcCabeSubdivitionSetting.Default();
        public double MaximumAreaTarget => Setting.MaximumAreaTarget;

        public double MinimumAreaTarget => Setting.MinimumAreaTarget;

        public double DivisibleThreshold => Setting.DivisibleThreshold;

        public Polyline Boundary;

        public KellyMcCabeLotSubdivisionModel(Polyline boundary)
        {
            Boundary = boundary;
        }

        public void Solve()
        {

        }

    }

    public class KellyMcCabeSubdivisionState
    {
        public KellyMcCabeSubdivitionSetting Setting = KellyMcCabeSubdivitionSetting.Default();

        public double MaximumAreaTarget => Setting.MaximumAreaTarget;

        public double MinimumAreaTarget => Setting.MinimumAreaTarget;

        public double DivisibleThreshold => Setting.DivisibleThreshold;

        public Random Random;


        public KellyMcCabeLotSubdivisionModel AssociatedModel;

        public List<Line> BoundaryLines = new List<Line>();
        public List<bool> HasStreetAccess = new List<bool>();
        public Func<Line, double> WeightFunc;
        public List<double> Weights => BoundaryLines.ConvertAll(x => HasStreetAccess[BoundaryLines.IndexOf(x)] ? WeightFunc.Invoke(x) : 0);

        public Func<Line> BoundaryLinePicker => MathsHelper.WeightedRandomPick(BoundaryLines, Weights, Random);

        public KellyMcCabeModelResult Finalised
        {
            get
            {
                List<Point3d> pts = new List<Point3d>();
                foreach (Line line in BoundaryLines)
                {
                    pts.Add(line.From);
                }
                pts.Add(pts[0]);
                var pl = new Polyline(pts);
                var amp = AreaMassProperties.Compute(pl.ToNurbsCurve());
                if (amp.Area <= MinimumAreaTarget)
                {
                    return KellyMcCabeModelResult.Invalid;
                } else if (amp.Area >= MaximumAreaTarget)
                {
                    return KellyMcCabeModelResult.Continue;
                } else
                {
                    return KellyMcCabeModelResult.Success;
                }
            }
        }

        public KellyMcCabeSubdivisionState(Random random)
        {
            Random = random;

            WeightFunc = x => (x.Length > DivisibleThreshold) ? x.Length * x.Length : 0;
        }

        public KellyMcCabeModelResult Solve()
        {
            int maxIterations = 10;
            Line line = BoundaryLinePicker.Invoke();
            DivideLine(line, 4, out Line result);
            DivisionLineIntersect(result, out var stateA, out var stateB);
            // 0228 TODO

            return KellyMcCabeModelResult.Invalid;
        }

        public bool DivideLine(Line line, double maxDeviation, out Line result)
        {
            result = default;
            if (line.Length >= Setting.DivisibleThreshold)
            {
                double pos = Random.NextDouble() * maxDeviation * 2 + (line.Length / 2 - maxDeviation);
                Vector3d vec = new Vector3d(-line.From + line.To);
                vec.Rotate(Math.PI / 2.0, Vector3d.ZAxis);
                vec.Unitize();
                vec = vec * 9999;
                result = new Line(line.PointAt(pos), vec);
                return true;
            } else
            {
                return false;
            }    
        }

        public void DivisionLineIntersect(Line line, out KellyMcCabeSubdivisionState stateA, out KellyMcCabeSubdivisionState stateB)
        {
            bool currentA = true;
            int idA = -1;
            int idB = -1;
            Point3d intersectionA = default;
            Point3d intersectionB = default;

            for (int i = 0; i < BoundaryLines.Count; i++)
            {
                Intersection.LineLine(BoundaryLines[i], line, out double p1, out double p2, GlobalSettings.AbsoluteTolerance, true);
                if (BoundaryLines[i].PointAt(p1).EpsilonEquals(line.PointAt(p2), GlobalSettings.AbsoluteTolerance))
                {
                    if (currentA)
                    {
                        idA = i; // idA < idB
                        intersectionA = line.PointAt(p2);
                        currentA = false;
                    } else
                    {
                        idB = i;
                        intersectionB = line.PointAt(p2);
                    }
                }
            }

            List<Line> segmentsA = new List<Line>();
            List<bool> accessA = new List<bool>();

            List<Line> segmentsB = new List<Line>();
            List<bool> accessB = new List<bool>();

            for (int i = 0; i < BoundaryLines.Count; i++)
            {
                if (i < idA)
                {
                    segmentsA.Add(BoundaryLines[i]);
                    accessA.Add(HasStreetAccess[i]);
                } else if (i == idA)
                {
                    segmentsA.Add(new Line(BoundaryLines[i].From, intersectionA));
                    accessA.Add(HasStreetAccess[i]);
                    segmentsA.Add(new Line(intersectionA, intersectionB));
                    accessA.Add(false);
                    segmentsB.Add(new Line(intersectionB, intersectionA));
                    accessB.Add(false);
                    segmentsB.Add(new Line(intersectionA, BoundaryLines[i].To));
                    accessB.Add(HasStreetAccess[i]);
                } else if (i < idB)
                {
                    segmentsB.Add(BoundaryLines[i]);
                    accessB.Add(HasStreetAccess[i]);
                } else if (i == idB)
                {
                    segmentsB.Add(new Line(BoundaryLines[i].From, intersectionB));
                    accessB.Add(HasStreetAccess[i]);
                    segmentsA.Add(new Line(intersectionB, BoundaryLines[i].To));
                    accessA.Add(HasStreetAccess[i]);
                } else
                {
                    segmentsA.Add(BoundaryLines[i]);
                    accessA.Add(HasStreetAccess[i]);
                }
            }

            stateA = new KellyMcCabeSubdivisionState(Random) { BoundaryLines = segmentsA.ToList(), HasStreetAccess = accessA.ToList(), AssociatedModel = this.AssociatedModel};

            stateB = new KellyMcCabeSubdivisionState(Random) { BoundaryLines = segmentsB.ToList(), HasStreetAccess = accessB.ToList(), AssociatedModel = this.AssociatedModel };



            /*
            List<bool> containsIntersection = new List<bool>();
            List<double> intersectionParams = new List<double>();
            for (int i = 0; i < BoundaryLines.Count; i++)
            {
                Intersection.LineLine(BoundaryLines[i], line, out double p1, out double p2, GlobalSettings.AbsoluteTolerance, true);
                if (BoundaryLines[i].PointAt(p1).EpsilonEquals(line.PointAt(p2), GlobalSettings.AbsoluteTolerance))
                {
                    containsIntersection.Add(true);
                    intersectionParams.Add(p1);
                } else
                {
                    containsIntersection.Add(false);
                    intersectionParams.Add(-1);
                }
            }

            List<List<Line>> segmentsLists = new List<List<Line>>();
            List<List<bool>> hasAccessLists = new List<List<bool>>();
            Dictionary<int, Point3d> prevIntersectionPoints = new Dictionary<int, Point3d>();

            bool isLeft = true;
            int currentId = 0;
            
            for (int i = 0; i < BoundaryLines.Count; i++)
            {
                if (containsIntersection[i])
                {
                    if (segmentsLists.Count <= currentId)
                    {
                        segmentsLists.Add(new List<Line>());
                        segmentsLists[currentId].Add(new Line(BoundaryLines[i].From, BoundaryLines[i].PointAt(intersectionParams[i])));
                        prevIntersectionPoints.Add(currentId, )
                    }


                } else
                {
                    if (segmentsLists.Count <= currentId)
                    {
                        segmentsLists.Add(new List<Line>());
                        segmentsLists[currentId].Add(BoundaryLines[i]);
                    } else
                    {
                        segmentsLists[currentId].Add(BoundaryLines[i]);
                    }

                    if (hasAccessLists.Count <= currentId)
                    {
                        hasAccessLists.Add(new List<bool>());
                        hasAccessLists[currentId].Add(HasStreetAccess[i]);
                    }
                    else
                    {
                        hasAccessLists[currentId].Add(HasStreetAccess[i]);
                    }
                }
            }*/

        }


    }
}
