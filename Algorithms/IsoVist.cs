using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Algorithms
{
    public class IsoVistComputationalSettings
    {
        public double RadiusForIntersectionCheck = 120.0; // disused

        public double MaxRadius = 800.0;

        public int RayCount = 45;

        public int SubRayCount = 10;

        public static IsoVistComputationalSettings Default => new IsoVistComputationalSettings();
    }

    public class Intersection
    {
        public bool HasIntersection = false;
        public double ParamOnRay = -1.0;
        public Point3d Point = new Point3d();
        public int ObstacleId;

        public Intersection(double p, Point3d point, int id)
        {
            HasIntersection = true;
            ParamOnRay = p;
            Point = point;
            ObstacleId = id;
        }

        public Intersection Duplicate()
        {
            return new Intersection(ParamOnRay, Point, ObstacleId) { HasIntersection = HasIntersection };
        }
    }

    /// <summary>
    /// IsoVist, aiming at high speed
    /// </summary>
    public class IsoVist
    {
        public List<Line> Obstacles;
        public List<int> ObstacleIds;
        public IsoVistComputationalSettings Settings = IsoVistComputationalSettings.Default;
        List<Vector3d> RayVecs = new List<Vector3d>();
        public DataStructure.GHIOParam<IsoVist> GHIOParam => new DataStructure.GHIOParam<IsoVist>() { ScriptClassVariable = this };

        protected IsoVist()
        {
            double angle;
            RayVecs = new List<Vector3d>();
            for (int i = 0; i < Settings.RayCount; i++)
            {
                angle = Math.PI * 2.0 / Settings.RayCount * i;
                RayVecs.Add(new Vector3d(
                    Math.Cos(angle),
                    Math.Sin(angle),
                    0) * Settings.MaxRadius
                    ); ;
            }
        }

        protected IsoVist(IsoVistComputationalSettings settings)
        {
            Settings = settings;
            double angle;
            RayVecs = new List<Vector3d>();
            for (int i = 0; i < Settings.RayCount; i++)
            {
                angle = Math.PI * 2.0 / Settings.RayCount * i;
                RayVecs.Add(new Vector3d(
                    Math.Cos(angle),
                    Math.Sin(angle),
                    0) * Settings.MaxRadius
                    ); ;
            }
        }

        public static IsoVist CreateObstacleModel(List<Line> lines)
        {
            List<int> ids = new List<int>(lines.Count);
            for (int i = 0; i < lines.Count; i++)
            {
                ids.Add(i);
            }
            return new IsoVist() { Obstacles = lines.ToList(), ObstacleIds = ids.ToList() };

        }

        public static IsoVist CreateObstacleModel(List<Line> lines, IsoVistComputationalSettings settings)
        {
            List<int> ids = new List<int>(lines.Count);
            for (int i = 0; i < lines.Count; i++)
            {
                ids.Add(i);
            }
            return new IsoVist(settings) { Obstacles = lines.ToList(), ObstacleIds = ids.ToList() };

        }

        public IsoVistResult Compute(Point3d point)
        {
            List<Line> rays = new List<Line>();
            RayVecs.ForEach(r => rays.Add(new Line(point, point + r)));
            List<Intersection> intersections = new List<Intersection>();
            rays.ForEach(r => intersections.Add(ComputeRay(r)));
            return new IsoVistResult(point, intersections);
        }

        Intersection Intersect(Line partialRay, List<Line> linesCloseEnough, List<int> ids)
        {
            Intersection intersection = new Intersection(Settings.MaxRadius, partialRay.To, -1);
            intersection.HasIntersection = false;
            for (int i = 0; i < linesCloseEnough.Count; i++)
            {
                bool intersectionResult = Rhino.Geometry.Intersect.Intersection.LineLine(partialRay, linesCloseEnough[i], out double a, out double b, GlobalSettings.AbsoluteTolerance, true);
                if (intersectionResult && partialRay.PointAt(a).EpsilonEquals(linesCloseEnough[i].PointAt(b), GlobalSettings.AbsoluteTolerance))
                {
                    if (a < intersection.ParamOnRay && a > 0) // added a>0 to avoid strange computational errors in LineLineIntersection
                    {
                        intersection = new Intersection(a, partialRay.PointAt(a), ids[i]);
                    }
                    
                }
            }
            return intersection;
            
        }
        Intersection ComputeRay(Line fullRay)
        {
            List<Line> linesCloseEnough = new List<Line>();
            List<int> ids = new List<int>();
            
            for (int i = 0; i < Obstacles.Count; i++)
            {
                if (CloseEnough(fullRay, Obstacles[i]))
                {
                    linesCloseEnough.Add(Obstacles[i]);
                    ids.Add(ObstacleIds[i]);
                }
            }

            int partialRayNumSeq = 0;
            Intersection intersection = default;

            while (partialRayNumSeq < Settings.SubRayCount)
            {
                intersection = Intersect(PartialRay(fullRay, partialRayNumSeq), linesCloseEnough, ids);
                intersection.ParamOnRay = fullRay.ClosestParameter(intersection.Point);
                if (intersection.HasIntersection) break;
                partialRayNumSeq++;
            }
            return intersection;
        }

        Line PartialRay(Line fullRay, int numSeq)
        {
            return new Line(
                new Point3d(
                    fullRay.FromX + numSeq * (fullRay.ToX - fullRay.FromX) / Settings.SubRayCount,
                    fullRay.FromY + numSeq * (fullRay.ToY - fullRay.FromY) / Settings.SubRayCount,
                    0),
                new Point3d(
                    fullRay.FromX + (numSeq + 1) * (fullRay.ToX - fullRay.FromX) / Settings.SubRayCount,
                    fullRay.FromY + (numSeq + 1) * (fullRay.ToY - fullRay.FromY) / Settings.SubRayCount,
                    0));
        }


        bool CloseEnough(Line ray, Line line)
        {
            double minX = Math.Min(line.FromX, line.ToX);
            double maxX = Math.Max(line.FromX, line.ToX);
            double minY = Math.Min(line.FromY, line.ToY);
            double maxY = Math.Max(line.FromY, line.ToY);

            if (ray.FromX < minX && ray.ToX < minX) return false;
            if (ray.FromX > maxX && ray.ToX > maxX) return false;
            if (ray.FromY < minY && ray.ToY < minY) return false;
            if (ray.FromY > maxY && ray.ToY > maxY) return false;
            return true;
        }
    }


    public class IsoVistResult
    {
        public Point3d Point;
        public List<Point3d> Points = new List<Point3d>();
        public List<int> Ids = new List<int>();
        List<Intersection> Intersections;
        public List<double> Radii = new List<double>();
        public Polyline BoundaryLine = new Polyline();
        public double Perimeter = 0.0;
        public double Area = 0.0;
        public Point3d Centroid = new Point3d();
        public double Compactness = 0.0;
        public double ClosedPerimeter = 0.0;
        public double Occlusivity = 0.0;
        public double Vista = 0.0;
        public double AverageRadii = 0.0;
        public double Drift = 0.0;
        public double Variance = 0.0;
        public double Skewness = 0.0;
        public IsoVistResult(Point3d point, List<Intersection> intersections)
        {
            Point = point;
            intersections.ForEach(t => Points.Add(t.Point));
            intersections.ForEach(t => Ids.Add(t.ObstacleId));
            Intersections = intersections.ConvertAll(i => i.Duplicate());
            Compute();
        }

        void Compute()
        {
            Points.ForEach(p => Radii.Add(p.DistanceTo(Point)));
            BoundaryLine = new Polyline(Points);
            BoundaryLine.Add(BoundaryLine[0]);
            Perimeter = 0.0;
            for (int i = 0; i < Points.Count; i++)
            {
                int prevId = i == 0 ? Points.Count - 1 : i - 1;
                int nextId = i == Points.Count - 1 ? 0 : i + 1;

                double distanceToNext = Points[i].DistanceTo(Points[nextId]);
                double distanceToPrev = Points[i].DistanceTo(Points[prevId]);

                Perimeter += distanceToNext;

                if (Intersections[i].HasIntersection)
                {
                    ClosedPerimeter += (distanceToNext + distanceToPrev) / 2;
                }

            }
            AreaMassProperties ams = AreaMassProperties.Compute(BoundaryLine.ToNurbsCurve());
            Area = ams.Area;
            Compactness = 4.0 * Math.PI * Area / Perimeter;
            Centroid = ams.Centroid;
            Occlusivity = 1 - ClosedPerimeter / Perimeter;
            Vista = Radii.Max();
            AverageRadii = Radii.Sum() / Radii.Count;
            Drift = Centroid.DistanceTo(Point);
            Variance = Radii.ConvertAll(r => (r - AverageRadii) * (r - AverageRadii)).Average();
            Skewness = Radii.ConvertAll(r => (r - AverageRadii) * (r - AverageRadii) * (r - AverageRadii)).Average();
        }
    }
}
