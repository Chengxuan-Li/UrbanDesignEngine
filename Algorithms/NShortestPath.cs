using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanDesignEngine.DataStructure;
using QuikGraph.Algorithms.Observers;
using Rhino;
using Rhino.Geometry;
using QuikGraph;
using UrbanDesignEngine.Triangulation;

namespace UrbanDesignEngine.Algorithms
{
    public class VGShortestPath
    {
        public UndirectedGraph<VGVertex, VGEdge> Graph;
        public Func<VGEdge, double> WeightFunc = e => e.Source.Location.DistanceTo(e.Target.Location);
        public QuikGraph.Algorithms.ShortestPath.UndirectedDijkstraShortestPathAlgorithm<VGVertex, VGEdge> Dijkstra;

        UndirectedVertexPredecessorRecorderObserver
            <VGVertex, VGEdge>
            predecessorObserver;

        UndirectedVertexDistanceRecorderObserver
            <VGVertex, VGEdge>
            distanceObserver;

        IDisposable pOb;
        IDisposable dOb;

        public VGShortestPath(UndirectedGraph<VGVertex, VGEdge> graph)
        {
            Graph = graph;
            Dijkstra = new
            QuikGraph.Algorithms.ShortestPath.
            UndirectedDijkstraShortestPathAlgorithm
                <VGVertex, VGEdge>
                (Graph, WeightFunc);
        }

        public VGShortestPath(UndirectedGraph<VGVertex, VGEdge> graph, Func<VGEdge, double> weightFunc)
        {
            Graph = graph;
            WeightFunc = weightFunc;
            Dijkstra = new
            QuikGraph.Algorithms.ShortestPath.
            UndirectedDijkstraShortestPathAlgorithm
                <VGVertex, VGEdge>
                (Graph, WeightFunc);
        }

        public void Solve(VGVertex root)
        {
            // attach a Vertex Predecessor Recorder Observer to give us the paths
            predecessorObserver = new
                UndirectedVertexPredecessorRecorderObserver
                <VGVertex, VGEdge>();
            pOb = predecessorObserver.Attach(Dijkstra);

            // attach a distance observer to give us the shortest path distances
            distanceObserver = new
                UndirectedVertexDistanceRecorderObserver
                <VGVertex, VGEdge>(WeightFunc);
            dOb = distanceObserver.Attach(Dijkstra);

            Dijkstra.Compute(root);
        }


        public void Dispose()
        {
            pOb.Dispose();
            dOb.Dispose();
        }

        public bool PathTo(VGVertex node, out List<VGEdge> path)
        {
            bool result = predecessorObserver.TryGetPath(node, out IEnumerable<VGEdge> p);
            path = result ? p.ToList() : new List<VGEdge>();
            return result;
        }

        public IDictionary<VGVertex, double> Distances()
        {
            return distanceObserver.Distances;
        }

        public bool DistanceTo(VGVertex node, out double distance)
        {
            distance = -1;
            if (Distances().ContainsKey(node))
            {
                return Distances().TryGetValue(node, out distance);
            }
            else
            {
                return false;
            }
        }
    }


    public class NShortestPath
    {
        public NetworkGraph Graph;
        public Func<NetworkEdge, double> WeightFunc = e => e.UnderlyingCurve.GetLength(); // default weight factor using curve length
        public QuikGraph.Algorithms.ShortestPath.
                UndirectedDijkstraShortestPathAlgorithm<NetworkNode, NetworkEdge> Dijkstra;

        UndirectedVertexPredecessorRecorderObserver
                <NetworkNode, NetworkEdge>
                predecessorObserver;

        UndirectedVertexDistanceRecorderObserver
               <NetworkNode, NetworkEdge>
               distanceObserver;

        IDisposable pOb;
        IDisposable dOb;

        public NShortestPath(NetworkGraph graph)
        {
            Graph = graph;
            Dijkstra = new
            QuikGraph.Algorithms.ShortestPath.
            UndirectedDijkstraShortestPathAlgorithm
            <NetworkNode, NetworkEdge>
            (Graph.Graph, WeightFunc);
        }

        public NShortestPath(NetworkGraph graph, Func<NetworkEdge, double> weightFunc)
        {
            Graph = graph;
            WeightFunc = weightFunc;
            Dijkstra = new
            QuikGraph.Algorithms.ShortestPath.
            UndirectedDijkstraShortestPathAlgorithm
            <NetworkNode, NetworkEdge>
            (Graph.Graph, WeightFunc);
        }

        public void Solve(NetworkNode root)
        {
            // attach a Vertex Predecessor Recorder Observer to give us the paths
            predecessorObserver = new 
            UndirectedVertexPredecessorRecorderObserver
            <NetworkNode, NetworkEdge>();
            pOb = predecessorObserver.Attach(Dijkstra);

            // attach a distance observer to give us the shortest path distances
            distanceObserver = new
            UndirectedVertexDistanceRecorderObserver
            <NetworkNode, NetworkEdge>(WeightFunc);
            dOb = distanceObserver.Attach(Dijkstra);

            Dijkstra.Compute(root);
           
        }

        public void Dispose()
        {
            pOb.Dispose();
            dOb.Dispose();
        }

        public bool PathTo(NetworkNode node, out List<NetworkEdge> path)
        {
            bool result = predecessorObserver.TryGetPath(node, out IEnumerable<NetworkEdge> p);
            path = result? p.ToList() : new List<NetworkEdge>();
            return result;
        }

        public IDictionary<NetworkNode, double> Distances()
        {
            return distanceObserver.Distances;
        }

        public bool DistanceTo(NetworkNode node, out double distance)
        {
            distance = -1;
            if (Distances().ContainsKey(node))
            {
                return Distances().TryGetValue(node, out distance);
            } else
            {
                return false;
            }
        }

        void InitialAndEndDistances(NetworkEdge edge, Point3d testPoint, out double viaSource, out double viaTarget)
        {
            double pS;
            double pT;
            double pTP;
            Curve crv = edge.UnderlyingCurve;
            crv.ClosestPoint(edge.Source.Point, out pS);
            crv.ClosestPoint(edge.Target.Point, out pT);
            crv.ClosestPoint(testPoint, out pTP);
            viaSource = crv.GetLength(new Interval(pS, pTP));
            viaTarget = crv.GetLength(new Interval(pTP, pT));
        }

        public void SolveShortestPathForPoints(Point3d fromPoint, List<Point3d> toPoints, out List<double> distances, out List<List<NetworkEdge>> edgePaths, out List<Curve> crvs)
        {
            List<double> distsFrom = Graph.Graph.Edges.ToList().ConvertAll(e =>
            {
                return e.UnderlyingCurve.ClosestPoint(fromPoint, out double t) ? e.UnderlyingCurve.PointAt(t).DistanceTo(fromPoint) : 99999999;
            });

            int fromEdgeId = distsFrom.IndexOf(distsFrom.Min());

            NetworkEdge fromEdge = Graph.Graph.Edges.ToList()[fromEdgeId];

            InitialAndEndDistances(fromEdge, fromPoint, out double fromDistanceViaSource, out double fromDistanceViaTarget);


            List<int> toEdgeIds = new List<int>();

            List<NetworkEdge> toEdges = new List<NetworkEdge>();

            List<double> toDistanceViaSourceList = new List<double>();
            List<double> toDistanceViaTargetList = new List<double>();

           
            List<double> distsSS = new List<double>(); // distances via fromEdge.Source and toEdge.Source
            List<double> distsST = new List<double>(); // distances via fromEdge.Source and toEdge.Target
            List<double> distsTS = new List<double>(); // distances via fromEdge.Target and toEdge.Source
            List<double> distsTT = new List<double>(); // distances via fromEdge.Target and toEdge.Target

            List<List<NetworkEdge>> pathsSS = new List<List<NetworkEdge>>(); // NetworkEdges via fromEdge.Source and toEdge.Source
            List<List<NetworkEdge>> pathsST = new List<List<NetworkEdge>>(); // NetworkEdges via fromEdge.Source and toEdge.Target
            List<List<NetworkEdge>> pathsTS = new List<List<NetworkEdge>>(); // NetworkEdges via fromEdge.Target and toEdge.Source
            List<List<NetworkEdge>> pathsTT = new List<List<NetworkEdge>>(); // NetworkEdges via fromEdge.Target and toEdge.Target

            List<double> distsChosen = new List<double>();
            List<List<NetworkEdge>> pathsChosen = new List<List<NetworkEdge>>();

            for (int i = 0; i < toPoints.Count; i++)
            {
                Point3d toPoint = toPoints[i];

                List<double> distsTo = Graph.Graph.Edges.ToList().ConvertAll(e =>
                {
                    return e.UnderlyingCurve.ClosestPoint(toPoint, out double t) ? e.UnderlyingCurve.PointAt(t).DistanceTo(toPoint) : 99999999;
                });

                toEdgeIds.Add(distsTo.IndexOf(distsTo.Min()));
                toEdges.Add(Graph.Graph.Edges.ToList()[distsTo.IndexOf(distsTo.Min())]);
            }
            for (int i = 0; i < toPoints.Count; i++)
            {
                InitialAndEndDistances(toEdges[i], toPoints[i], out double vS, out double vT);
                toDistanceViaSourceList.Add(vS);
                toDistanceViaTargetList.Add(vT);
            }

            // via fromEdge.Source
            Solve(fromEdge.Source);

            for (int i = 0; i < toPoints.Count; i++)
            {
                Point3d toPoint = toPoints[i];
                double d;
                List<NetworkEdge> ps;

                NetworkEdge toEdge = Graph.Graph.Edges.ToList()[toEdgeIds[i]];

                if (DistanceTo(toEdge.Source, out d))
                {
                    distsSS.Add(d);
                    pathsSS.Add(PathTo(toEdge.Source, out ps) ? ps : new List<NetworkEdge>());
                } else
                {
                    distsSS.Add(99999999);
                    pathsSS.Add(new List<NetworkEdge>());
                }

                if (DistanceTo(toEdge.Target, out d))
                {
                    distsST.Add(d);
                    pathsST.Add(PathTo(toEdge.Target, out ps) ? ps : new List<NetworkEdge>());
                } else
                {
                    distsST.Add(99999999);
                    pathsST.Add(new List<NetworkEdge>());
                }    
            }

            Dispose();

            // via fromEdge.Target
            Solve(fromEdge.Target);

            for (int i = 0; i < toPoints.Count; i++)
            {
                Point3d toPoint = toPoints[i];
                double d;
                List<NetworkEdge> ps;

                NetworkEdge toEdge = Graph.Graph.Edges.ToList()[toEdgeIds[i]];

                if (DistanceTo(toEdge.Source, out d))
                {
                    distsTS.Add(d);
                    pathsTS.Add(PathTo(toEdge.Source, out ps) ? ps : new List<NetworkEdge>());
                }
                else
                {
                    distsTS.Add(99999999);
                    pathsTS.Add(new List<NetworkEdge>());
                }

                if (DistanceTo(toEdge.Target, out d))
                {
                    distsTT.Add(d);
                    pathsTT.Add(PathTo(toEdge.Target, out ps) ? ps : new List<NetworkEdge>());
                }
                else
                {
                    distsTT.Add(99999999);
                    pathsTT.Add(new List<NetworkEdge>());
                }
            }

            Dispose();


            var subcrvs = new List<List<Curve>>();
            for (int i = 0; i < distsSS.Count; i++)
            {



                double k;
                subcrvs.Add(new List<Curve>());
                double ss = distsSS[i] + fromDistanceViaSource + toDistanceViaSourceList[i];
                double st = distsST[i] + fromDistanceViaSource + toDistanceViaTargetList[i];
                double ts = distsTS[i] + fromDistanceViaTarget + toDistanceViaSourceList[i];
                double tt = distsTT[i] + fromDistanceViaTarget + toDistanceViaTargetList[i];

                if (fromEdgeId == toEdgeIds[i]) // same edge start and end
                {
                    fromEdge.UnderlyingCurve.ClosestPoint(fromPoint, out double t1);
                    fromEdge.UnderlyingCurve.ClosestPoint(toPoints[i], out double t2);

                    if (t1 == 1 && t2 == 0 || t1 == 0 && t2 == 1)
                    {
                        distsChosen.Add(fromDistanceViaSource + toDistanceViaSourceList[i] + fromEdge.UnderlyingCurve.GetLength());
                        pathsChosen.Add(new List<NetworkEdge> { fromEdge });
                        subcrvs[i].Add(new Line(fromPoint, fromEdge.UnderlyingCurve.PointAt(t1)).ToNurbsCurve());
                        subcrvs[i].Add(fromEdge.UnderlyingCurve.ToNurbsCurve());
                        subcrvs[i].Add(new Line(toPoints[i], fromEdge.UnderlyingCurve.PointAt(t2)).ToNurbsCurve());
                    } else
                    {
                        distsChosen.Add(fromDistanceViaSource + toDistanceViaSourceList[i] + fromEdge.UnderlyingCurve.GetLength(new Interval(t1, t2)));
                        pathsChosen.Add(new List<NetworkEdge> { fromEdge });
                        var splitted = fromEdge.UnderlyingCurve.Split(new List<double> { t1, t2 }).ToList();
                        Curve crv = (t1 == 0 || t2 == 0) ? splitted[0] : splitted[1];
                        subcrvs[i].Add(new Line(fromPoint, fromEdge.UnderlyingCurve.PointAt(t1)).ToNurbsCurve());
                        subcrvs[i].Add(crv);
                        subcrvs[i].Add(new Line(toPoints[i], fromEdge.UnderlyingCurve.PointAt(t2)).ToNurbsCurve());
                    }
                } else if (ss < st && ss < ts && ss < tt)
                {
                    distsChosen.Add(ss);
                    pathsChosen.Add(pathsSS[i]);

                    fromEdge.UnderlyingCurve.ClosestPoint(fromPoint, out double t);
                    subcrvs[i].Add(new Line(fromPoint, fromEdge.UnderlyingCurve.PointAt(t)).ToNurbsCurve());

                    if (t == 1 || t == 0)
                    {
                        if (fromEdge.UnderlyingCurve.PointAt(t).EpsilonEquals(fromEdge.Source.Point, GlobalSettings.AbsoluteTolerance))
                        {

                        } else
                        {
                            subcrvs[i].Add(fromEdge.UnderlyingCurve);
                        }
                    } else
                    {
                        var splitted = fromEdge.UnderlyingCurve.Split(t).ToList();
                        foreach (Curve c in splitted)
                        {
                            if (c.ClosestPoint(fromEdge.Source.Point, out k) && c.PointAt(k).EpsilonEquals(fromEdge.Source.Point, GlobalSettings.AbsoluteTolerance))
                            {
                                subcrvs[i].Add(c);
                                break;
                            }
                        }
                    }
  

                    pathsSS[i].ForEach(p => subcrvs[i].Add(p.UnderlyingCurve));


                    toEdges[i].UnderlyingCurve.ClosestPoint(toPoints[i], out t);
                    if (t == 1 || t == 0)
                    {
                        if (toEdges[i].UnderlyingCurve.PointAt(t).EpsilonEquals(toEdges[i].Source.Point, GlobalSettings.AbsoluteTolerance))
                        {

                        }
                        else
                        {
                            subcrvs[i].Add(toEdges[i].UnderlyingCurve);
                        }
                    } else
                    {
                        var splitted = toEdges[i].UnderlyingCurve.Split(t).ToList();
                        foreach (Curve c in splitted)
                        {
                            if (c.ClosestPoint(toEdges[i].Source.Point, out k) & c.PointAt(k).EpsilonEquals(toEdges[i].Source.Point, GlobalSettings.AbsoluteTolerance))
                            {
                                subcrvs[i].Add(c);
                                break;
                            }
                        }
                    }
                    subcrvs[i].Add(new Line(toPoints[i], toEdges[i].UnderlyingCurve.PointAt(t)).ToNurbsCurve());

                } else if (st < ss && st < ts && st < tt)
                {
                    distsChosen.Add(st);
                    pathsChosen.Add(pathsST[i]);

                    fromEdge.UnderlyingCurve.ClosestPoint(fromPoint, out double t);
                    subcrvs[i].Add(new Line(fromPoint, fromEdge.UnderlyingCurve.PointAt(t)).ToNurbsCurve());

                    if (t == 1 || t == 0)
                    {
                        if (fromEdge.UnderlyingCurve.PointAt(t).EpsilonEquals(fromEdge.Source.Point, GlobalSettings.AbsoluteTolerance))
                        {

                        }
                        else
                        {
                            subcrvs[i].Add(fromEdge.UnderlyingCurve);
                        }
                    }
                    else
                    {
                        var splitted = fromEdge.UnderlyingCurve.Split(t).ToList();
                        foreach (Curve c in splitted)
                        {
                            if (c.ClosestPoint(fromEdge.Source.Point, out k) && c.PointAt(k).EpsilonEquals(fromEdge.Source.Point, GlobalSettings.AbsoluteTolerance))
                            {
                                subcrvs[i].Add(c);
                                break;
                            }
                        }
                    }

                    pathsST[i].ForEach(p => subcrvs[i].Add(p.UnderlyingCurve));


                    toEdges[i].UnderlyingCurve.ClosestPoint(toPoints[i], out t);
                    if (t == 1 || t == 0)
                    {
                        if (toEdges[i].UnderlyingCurve.PointAt(t).EpsilonEquals(toEdges[i].Target.Point, GlobalSettings.AbsoluteTolerance))
                        {

                        }
                        else
                        {
                            subcrvs[i].Add(toEdges[i].UnderlyingCurve);
                        }
                    }
                    else
                    {
                        var splitted = toEdges[i].UnderlyingCurve.Split(t).ToList();
                        foreach (Curve c in splitted)
                        {
                            if (c.ClosestPoint(toEdges[i].Target.Point, out k) & c.PointAt(k).EpsilonEquals(toEdges[i].Target.Point, GlobalSettings.AbsoluteTolerance))
                            {
                                subcrvs[i].Add(c);
                                break;
                            }
                        }
                    }
                    subcrvs[i].Add(new Line(toPoints[i], toEdges[i].UnderlyingCurve.PointAt(t)).ToNurbsCurve());

                }
                else if (ts < ss && ts < st && ts < tt)
                {
                    distsChosen.Add(ts);
                    pathsChosen.Add(pathsTS[i]);

                    fromEdge.UnderlyingCurve.ClosestPoint(fromPoint, out double t);
                    subcrvs[i].Add(new Line(fromPoint, fromEdge.UnderlyingCurve.PointAt(t)).ToNurbsCurve());

                    if (t == 1 || t == 0)
                    {
                        if (fromEdge.UnderlyingCurve.PointAt(t).EpsilonEquals(fromEdge.Target.Point, GlobalSettings.AbsoluteTolerance))
                        {

                        }
                        else
                        {
                            subcrvs[i].Add(fromEdge.UnderlyingCurve);
                        }
                    }
                    else
                    {
                        var splitted = fromEdge.UnderlyingCurve.Split(t).ToList();
                        foreach (Curve c in splitted)
                        {
                            if (c.ClosestPoint(fromEdge.Target.Point, out k) && c.PointAt(k).EpsilonEquals(fromEdge.Target.Point, GlobalSettings.AbsoluteTolerance))
                            {
                                subcrvs[i].Add(c);
                                break;
                            }
                        }
                    }

                    pathsTS[i].ForEach(p => subcrvs[i].Add(p.UnderlyingCurve));

                    toEdges[i].UnderlyingCurve.ClosestPoint(toPoints[i], out t);
                    if (t == 1 || t == 0)
                    {
                        if (toEdges[i].UnderlyingCurve.PointAt(t).EpsilonEquals(toEdges[i].Source.Point, GlobalSettings.AbsoluteTolerance))
                        {

                        }
                        else
                        {
                            subcrvs[i].Add(toEdges[i].UnderlyingCurve);
                        }
                    }
                    else
                    {
                        var splitted = toEdges[i].UnderlyingCurve.Split(t).ToList();
                        foreach (Curve c in splitted)
                        {
                            if (c.ClosestPoint(toEdges[i].Source.Point, out k) & c.PointAt(k).EpsilonEquals(toEdges[i].Source.Point, GlobalSettings.AbsoluteTolerance))
                            {
                                subcrvs[i].Add(c);
                                break;
                            }
                        }
                    }
                    subcrvs[i].Add(new Line(toPoints[i], toEdges[i].UnderlyingCurve.PointAt(t)).ToNurbsCurve());

                }
                else
                {
                    distsChosen.Add(tt);
                    pathsChosen.Add(pathsTT[i]);

                    fromEdge.UnderlyingCurve.ClosestPoint(fromPoint, out double t);
                    subcrvs[i].Add(new Line(fromPoint, fromEdge.UnderlyingCurve.PointAt(t)).ToNurbsCurve());

                    if (t == 1 || t == 0)
                    {
                        if (fromEdge.UnderlyingCurve.PointAt(t).EpsilonEquals(fromEdge.Target.Point, GlobalSettings.AbsoluteTolerance))
                        {

                        }
                        else
                        {
                            subcrvs[i].Add(fromEdge.UnderlyingCurve);
                        }
                    }
                    else
                    {
                        var splitted = fromEdge.UnderlyingCurve.Split(t).ToList();
                        foreach (Curve c in splitted)
                        {
                            if (c.ClosestPoint(fromEdge.Target.Point, out k) && c.PointAt(k).EpsilonEquals(fromEdge.Target.Point, GlobalSettings.AbsoluteTolerance))
                            {
                                subcrvs[i].Add(c);
                                break;
                            }
                        }
                    }

                    pathsTT[i].ForEach(p => subcrvs[i].Add(p.UnderlyingCurve));

                    toEdges[i].UnderlyingCurve.ClosestPoint(toPoints[i], out t);
                    if (t == 1 || t == 0)
                    {
                        if (toEdges[i].UnderlyingCurve.PointAt(t).EpsilonEquals(toEdges[i].Target.Point, GlobalSettings.AbsoluteTolerance))
                        {

                        }
                        else
                        {
                            subcrvs[i].Add(toEdges[i].UnderlyingCurve);
                        }
                    }
                    else
                    {
                        var splitted = toEdges[i].UnderlyingCurve.Split(t).ToList();
                        foreach (Curve c in splitted)
                        {
                            if (c.ClosestPoint(toEdges[i].Target.Point, out k) & c.PointAt(k).EpsilonEquals(toEdges[i].Target.Point, GlobalSettings.AbsoluteTolerance))
                            {
                                subcrvs[i].Add(c);
                                break;
                            }
                        }
                    }
                    subcrvs[i].Add(new Line(toPoints[i], toEdges[i].UnderlyingCurve.PointAt(t)).ToNurbsCurve());

                }
            }

            distances = distsChosen.ToList();
            edgePaths = pathsChosen.ToList();

            List<Curve> joinedCrvs = new List<Curve>();
            subcrvs.ForEach(sc => joinedCrvs.Add(Curve.JoinCurves(sc)[0]));

            crvs = joinedCrvs;
            crvs = Curve.JoinCurves(subcrvs[0]).ToList();
        }
    }
}
