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
    /// <summary>
    /// Snap algorithm. See Kelly and McCabe, "Citygen: An Interactive System for Procedural City Generation"
    /// </summary>
    
    public enum SnapResult
    {
        NoSnap = 0,
        Ends = 2,
        Midway = 1,
    }

    public class Snap
    {
        public double R = 1.0;
        public double LocalRadiusFactor = 10.0; // the radius of the "local" region / R
        public Point3d Source;
        public Point3d IntendedTarget;
        public List<Line> Obstacles;

        public Predicate<Point3d> Test1_PointInRoughProximity;

        //public Predicate<Line> Test2_SegmentIntersectingProximity;

        public Predicate<Point3d> Test3_PointInLocalRegion;

        Func<Point3d, int> func0;
        Func<Point3d, int> funcA;
        Func<Point3d, int> funcB;
        Func<Point3d, int> funcC;
        Func<Point3d, int> funcD;
        Func<Point3d, int> funcM;

        //    A    L    B
        //    |    |    |
        // D----------------D
        //    | /  |  \R|
        //    |/   |   \|
        // M----------------M
        //    |    |    |
        //    |    |    |
        //    |    |    |
        //    |    |    |
        // C-------O--------C
        //    |    |    |
        //    A    L    B

        Func<Point3d, int> funcLocalLB;
        Func<Point3d, int> funcLocalRB;
        Func<Point3d, int> funcLocalUB;
        Func<Point3d, int> funcLocalBB;

        Line lineExtended;

        /// <summary>
        /// WARNING: PLEASE IGNORE ADJACENT NODES WHEN REQUIRED!
        /// </summary>
        /// <param name="source"></param>
        /// <param name="intendedTarget"></param>
        /// <param name="obstacles"></param>
        public Snap(Point3d source, Point3d intendedTarget, List<Line> obstacles, double r)
        {
            Source = source;
            IntendedTarget = intendedTarget;
            Obstacles = obstacles;
            R = r;

            Vector3d vec = new Vector3d(intendedTarget - source);
            vec.Unitize();
            Vector3d vecRo = new Vector3d(vec);
            vecRo.Rotate(Math.PI / 2.0, Vector3d.ZAxis);

            Line line = new Line(
                source,
                intendedTarget
                );
            func0 = MathsHelper.PointLineRelation(line);

            lineExtended = new Line(
                source,
                intendedTarget + vec * R
                );

            Line lineA = new Line(
                source + vecRo * R,
                intendedTarget + vec * R + vecRo * R
                );
            funcA = MathsHelper.PointLineRelation(lineA);

            Line lineB = new Line(
                source - vecRo * R,
                intendedTarget + vec * R - vecRo * R
                );
            funcB = MathsHelper.PointLineRelation(lineB);

            Line lineC = new Line(
                source + vecRo * R,
                source - vecRo * R
                );
            funcC = MathsHelper.PointLineRelation(lineC);

            Line lineD = new Line(
                intendedTarget + vec * R + vecRo * R,
                intendedTarget + vec * R - vecRo * R
                );
            funcD = MathsHelper.PointLineRelation(lineD);

            Line lineM = new Line(
                intendedTarget + vecRo * R,
                intendedTarget - vecRo * R
                );
            funcM = MathsHelper.PointLineRelation(lineM);


            Line localLeftBound = new Line(
                intendedTarget - Vector3d.XAxis * LocalRadiusFactor * R + Vector3d.YAxis * LocalRadiusFactor * R,
                intendedTarget - Vector3d.XAxis * LocalRadiusFactor * R - Vector3d.YAxis * LocalRadiusFactor * R
                );
            funcLocalLB = MathsHelper.PointLineRelation(localLeftBound);

            Line localRightBound = new Line(
                intendedTarget + Vector3d.XAxis * LocalRadiusFactor * R - Vector3d.YAxis * LocalRadiusFactor * R,
                intendedTarget + Vector3d.XAxis * LocalRadiusFactor * R + Vector3d.YAxis * LocalRadiusFactor * R
                );
            funcLocalRB = MathsHelper.PointLineRelation(localRightBound);

            Line localUpperBound = new Line(
                intendedTarget + Vector3d.XAxis * LocalRadiusFactor * R + Vector3d.YAxis * LocalRadiusFactor * R,
                intendedTarget - Vector3d.XAxis * LocalRadiusFactor * R + Vector3d.YAxis * LocalRadiusFactor * R
                );
            funcLocalUB = MathsHelper.PointLineRelation(localUpperBound);

            Line localBottomBound = new Line(
                intendedTarget - Vector3d.XAxis * LocalRadiusFactor * R - Vector3d.YAxis * LocalRadiusFactor * R,
                intendedTarget + Vector3d.XAxis * LocalRadiusFactor * R - Vector3d.YAxis * LocalRadiusFactor * R
                );
            funcLocalBB = MathsHelper.PointLineRelation(localBottomBound);

            Test1_PointInRoughProximity = p =>
            {
                return
                funcA.Invoke(p) * funcB.Invoke(p) == -1 && // different sides of A and B
                funcC.Invoke(p) * funcD.Invoke(p) == -1;   // different sides of C and D
            };

            Test3_PointInLocalRegion = p =>
            {
                return
                funcLocalLB.Invoke(p) * funcLocalRB.Invoke(p) == -1 &&
                funcLocalUB.Invoke(p) * funcLocalBB.Invoke(p) == -1;
            };
  
        }

        public SnapResult Solve(out double distance, out Point3d point, out int lineIndex)
        {
            List<double> dists = new List<double>();
            List<Point3d> pts = new List<Point3d>();
            List<int> lineIndices = new List<int>();
            List<SnapResult> results = new List<SnapResult>(); 


            for (int i = 0; i < Obstacles.Count; i++)
            {
                Line line = Obstacles[i];
                double dist = -1;
                double distTemp;
                Point3d pt = Point3d.Origin;
                Point3d ptTemp;
                SnapResult result = SnapResult.NoSnap;
                bool performTest3 = true;
                bool snapPtFound = false;
                if (Test1(line.From, out distTemp))
                {
                    pt = line.From;
                    dist = distTemp;
                    performTest3 = false;
                    snapPtFound = true;
                    result = SnapResult.Ends;
                }
                if (Test1(line.To, out distTemp))
                {
                    if (!snapPtFound || (distTemp < dist && distTemp >= 0))
                    {
                        dist = distTemp;
                        pt = line.To;
                        performTest3 = false;
                        snapPtFound = true;
                        result = SnapResult.Ends;
                    }
                }
                if (Test2(line, out distTemp, out ptTemp))
                {
                    if (!snapPtFound || (distTemp < dist && distTemp >= 0))
                    {
                        dist = distTemp;
                        pt = ptTemp;
                        performTest3 = false;
                        snapPtFound = true;
                        result = SnapResult.Midway;
                    }
                }
                if (performTest3)
                {
                    var result3 = Test3(line, out distTemp, out ptTemp);
                    if (result3 != SnapResult.NoSnap)
                    {
                        dist = distTemp;
                        pt = ptTemp;
                        snapPtFound = true;
                        result = result3;
                    }
                }

                if (snapPtFound)
                {
                    dists.Add(dist);
                    pts.Add(pt);
                    lineIndices.Add(i);
                    results.Add(result);
                }

            }

            if (dists.Count > 0)
            {
                int argMin = dists.IndexOf(dists.Min());
                distance = dists[argMin];
                point = pts[argMin];
                lineIndex = lineIndices[argMin];
                return results[argMin];
            } else
            {
                distance = -1;
                point = Point3d.Origin;
                lineIndex = -1;
                return SnapResult.NoSnap;
            }
        }



        /// <summary>
        /// Determine if there are points inside the proximity zone
        /// </summary>
        /// <param name="testPt">Test point</param>
        /// <returns>true if test point is inside the proximity zone</returns>
        bool Test1(Point3d testPt, out double distance)
        {
            distance = -1;
            if (Test1_PointInRoughProximity.Invoke(testPt))
            {
                if (funcC.Invoke(testPt) * funcM.Invoke(testPt) == -1) // not in the semicircle
                {
                    distance = testPt.DistanceTo(Source);
                    return true;
                } else
                {
                    distance = testPt.DistanceTo(Source);
                    return testPt.DistanceTo(IntendedTarget) <= R;
                }
            } else
            {
                return false;
            }
        }

        /// <summary>
        /// Determine if there are line segments traversing the proximity zone
        /// </summary>
        bool Test2(Line line, out double distance, out Point3d point)
        {
            distance = -1;
            point = Point3d.Origin;
            if (func0.Invoke(line.From) * func0.Invoke(line.To) == -1) // different sides of line(0)
            {
                double a;
                double b;
                if (Rhino.Geometry.Intersect.Intersection.LineLine(lineExtended, line, out a, out b, GlobalSettings.AbsoluteTolerance, true))
                {
                    distance = lineExtended.PointAt(a).DistanceTo(Source);
                    point = lineExtended.PointAt(a);
                    return true;
                }
                else
                {
                    // disjoint
                    distance = Source.DistanceTo(line.PointAt(b));
                    point = line.PointAt(b);
                    return distance <= R;
                }
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Determine if the intended target point is sufficiently away from proximate segments
        /// </summary>
        SnapResult Test3(Line line, out double distance, out Point3d point)
        {
            distance = -1;
            point = Point3d.Origin;

            if (Test3_PointInLocalRegion.Invoke(line.From) || Test3_PointInLocalRegion.Invoke(line.To))
            {
                distance = line.ClosestPoint(Source, true).DistanceTo(Source);
                point = line.ClosestPoint(IntendedTarget, true);
                return line.ClosestPoint(IntendedTarget, true).DistanceTo(IntendedTarget) > R ? SnapResult.NoSnap
                    : (line.ClosestPoint(IntendedTarget, true).EpsilonEquals(line.From, GlobalSettings.AbsoluteTolerance)
                    || line.ClosestPoint(IntendedTarget, true).EpsilonEquals(line.To, GlobalSettings.AbsoluteTolerance)
                    )? SnapResult.Ends : SnapResult.Midway;
            } else
            {
                return SnapResult.NoSnap;
            }
        }
    }
}
