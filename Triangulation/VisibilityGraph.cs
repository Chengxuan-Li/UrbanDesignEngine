using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using QuikGraph;
using QuikGraph.Collections;
using Rhino.Geometry.Intersect;
using UrbanDesignEngine.Algorithms;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.IO;


namespace UrbanDesignEngine.Triangulation
{
    public class VisibilityGraphSettings
    {
        public double AdjacentPointDistance = 0;
        public int AdjacentPointsCount = 1;
        public double MaxVisibleDistance = 50;

        public static VisibilityGraphSettings Default => new VisibilityGraphSettings();

    }


    public class VGVertex : HasScriptRuntimeGeometry<Point>, IComparable<VGVertex>, IEquatable<VGVertex>, IHasGHIOPreviewGeometricParam<VGVertex, GHIOPointParam<VGVertex>, Point>
    {
        public Point3d Location;
        public int Id;
        UndirectedGraph<VGVertex, VGEdge> Graph;

        public override Point Geometry => new Point(this.Location);

        public GHIOPointParam<VGVertex> gHIOParam => new GHIOPointParam<VGVertex>() { ScriptClassVariable = this };

        public VGVertex(int id, Point3d location, UndirectedGraph<VGVertex, VGEdge> graph)
        {
            Id = id;
            Location = location;
            Graph = graph;
        }

        public int CompareTo(VGVertex other)
        {
            return Location.CompareTo(other.Location);
        }

        public bool Equals(VGVertex other)
        {
            return Location.EpsilonEquals(other.Location, GlobalSettings.AbsoluteTolerance);
        }
    }

    public class VGEdge : HasScriptRuntimeGeometry<Curve>, IEdge<VGVertex>, IComparable<VGEdge>, IEquatable<VGEdge>,  IHasGHIOPreviewGeometricParam<VGEdge, GHIOGraphCurveParam<VGEdge>, Curve>
    {
        protected VGVertex A;
        protected VGVertex B;
        public int Id;
        UndirectedGraph<VGVertex, VGEdge> Graph;

        public override Curve Geometry => new Line(this.Source.Location, this.Target.Location).ToNurbsCurve();

        public VGVertex Source => A.CompareTo(B) > 0 ? B : A;

        public VGVertex Target => A.CompareTo(B) > 0 ? A : B;

        public GHIOGraphCurveParam<VGEdge> gHIOParam => new GHIOGraphCurveParam<VGEdge>() { ScriptClassVariable = this };

        public VGEdge(int id, VGVertex a, VGVertex b, UndirectedGraph<VGVertex, VGEdge> graph)
        {
            Id = id;
            A = a;
            B = b;
            Graph = graph;
        }

        public int CompareTo(VGEdge other)
        {
            var ss = Source.CompareTo(other.Source);
            return ss == 0 ? Target.CompareTo(other.Target) : ss;
        }

        public bool Equals(VGEdge other)
        {
            return (Source.Id == other.Source.Id && Target.Id == other.Target.Id) || (Source.Id == other.Target.Id && Target.Id == other.Source.Id);
        }
    }


    public class VisibilityGraph
    {
        public List<Point3d> AdditionalPoints = new List<Point3d>();
        public Mesh Mesh;
        public List<Polyline> Contours = new List<Polyline>();
        public List<Point3d> AllPoints = new List<Point3d>();
        public VisibilityGraphSettings Settings = VisibilityGraphSettings.Default;
        public UndirectedGraph<VGVertex, VGEdge> Graph = new UndirectedGraph<VGVertex, VGEdge>();
        public GHIOParam<VisibilityGraph> GHIOParam => new GHIOParam<VisibilityGraph>(this);

        public int NextVertexId => Graph.VertexCount;
        public int NextEdgeId => Graph.EdgeCount;
        
        protected List<Line> obsSeg
        {
            get
            {
                List<Line> lines = new List<Line>();
                Contours.ForEach(c => lines.AddRange(c.GetSegments().ToList()));
                return lines;
            }
        }

        public VisibilityGraph(Mesh mesh, List<Point3d> additionalPoints, VisibilityGraphSettings settings)
        {
            Mesh = mesh;
            AdditionalPoints = additionalPoints.ToList();
            Settings = settings;
            Contours.AddRange(Mesh.GetOutlines(Plane.WorldXY).ToList());
            GenPoints();
            BuildGraph();
        }


        int AddVertex(Point3d location)
        {
            bool result = Graph.AddVertex(new VGVertex(NextVertexId, location, Graph));
            return result ? NextVertexId - 1 : -1;
        }

        int AddEdge(int a, int b)
        {
            VGEdge e = new VGEdge(NextEdgeId, Graph.Vertices.ToList()[a], Graph.Vertices.ToList()[b], Graph);
            int index = Graph.Edges.ToList().FindIndex(ee => ee.Equals(e));
            if (index == -1)
            {
                Graph.AddEdge(e);
                return NextEdgeId - 1;
            } else
            {
                return index;
            }
        }

        void BuildGraph()
        {
            foreach (Point3d point in AllPoints)
            {
                AddVertex(point);
            }

            int count = Graph.VertexCount;

            for (int i = 0; i < count; i++)
            {
                for (int j = i; j < count; j++)
                {
                    if (!Intersecting(
                        new Line(
                            Graph.Vertices.ToList()[i].Location,
                            Graph.Vertices.ToList()[j].Location
                            ))) AddEdge(i, j);
                }
            }
        }

        void GenPoints()
        {
            foreach (Point3d pt in AdditionalPoints)
            {
                if (Inside(pt))
                {
                    AllPoints.Add(pt);
                }
            }

            foreach (var vpt in Mesh.Vertices)
            {
                var adjPts = AdjacentPoints(new Point3d(vpt.X, vpt.Y, 0), Settings.AdjacentPointDistance, Settings.AdjacentPointsCount, true);
                if (adjPts.Count > 0) AllPoints.AddRange(adjPts);
            }
        }

        List<Point3d> AdjacentPoints(Point3d pt, double distance, int count, bool checkInclusivity)
        {
            double angleInterval = Math.PI * 2.0 / count;
            List<Point3d> result = new List<Point3d>();

            for (int i = 0; i < count; i++)
            {
                Point3d newPt = pt + new Vector3d(Math.Cos(i * angleInterval), Math.Sin(i * angleInterval), 0) * distance;
                if (checkInclusivity)
                {
                    if (Inside(newPt)) result.Add(newPt);
                } else
                {
                    result.Add(newPt);
                }
            }
            return result;
        }

        bool Inside(Point3d point)
        {
            return Mesh.ClosestPoint(point).DistanceTo(point) < GlobalSettings.AbsoluteTolerance;
        }

        bool Inside(Line line)
        {
            if (!Inside(line.From)) return false;
            if (!Inside(line.To)) return false;
            if (!Inside((line.From + line.To) / 2)) return false;
            return true;
        }

        bool Intersecting(Line line)
        {
            if (!Inside(line)) return true;
            if (line.Length >= Settings.MaxVisibleDistance) return true; // applies the max distance setting
            foreach (Line x in obsSeg.ToList())
            {
                /*
                Intersection.LineLine(line, x, out double a, out double b, GlobalSettings.AbsoluteTolerance, true);
                if (line.PointAt(a).EpsilonEquals(x.PointAt(b), GlobalSettings.AbsoluteTolerance))
                {
                    return true;
                }*/
                if (SweepLineIntersection.LineLineIntersection(x, line)) return true;
            }
            return false;
        }
    }
}
