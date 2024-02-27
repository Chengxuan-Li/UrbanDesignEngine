using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using QuikGraph;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.Growth;
using UrbanDesignEngine.Algorithms;
using UrbanDesignEngine.Utilities;
using UrbanDesignEngine.Constraints;

namespace UrbanDesignEngine
{
    public class LSystem
    {
        public NetworkGraph Graph = new NetworkGraph();
        public int Iterations = 4;
        public double MinDistance = 1;
        public double MaxDistance = 20;
        public double SnapDistance = 5;
        public double MinimumAngle = 2.8 / 6.0 * Math.PI;
        public double MaximumAngle = Math.PI;
        public int NumAttempt = 5;
        public int NumPossibleGrowth = 2;
        public int CalculationTimeLimit = 3000;
        public Random random = new Random();
                                                                                                                                                                
        public List<Curve> FaceCurves
        {
            get
            {
                List<Curve> crvs = new List<Curve>();
                Graph.NetworkFaces.ForEach(f => crvs.Add(f.SimpleGeometry()));
                return crvs;
            }
        }

        public void SetSeed(int seed)
        {
            random = new Random(seed);
        }

        int currentIteration = 0;
        public LSystem()
        {
        }

        public LSystem(Point3d origin)
        {
            Graph.AddNetworkNode(origin);
        }

        public LSystem(NetworkGraph graph)
        {
            Graph = graph.Duplicate();
        }

        public void Solve()
        {
            var task = Task.Run(() => { 
                while (currentIteration < Iterations)
                {
                    GrowAll();
                    currentIteration++;
                }
            });
            bool isCompletedSuccessfully = task.Wait(TimeSpan.FromMilliseconds(CalculationTimeLimit));

            if (isCompletedSuccessfully)
            {
                
            }
            else
            {
                return;
                //throw new TimeoutException("The function has taken longer than the maximum time allowed.");
            }

        }

        void GrowAll()
        {

            Graph.Graph.Vertices.ToList().ForEach(v => Grow(v));

        }

        void Grow(NetworkNode node)
        {
            if (node.IsActive)
            {
                AngleControlledGrowth angleControlledGrowth = new AngleControlledGrowth(node, MinimumAngle, MaximumAngle);
                angleControlledGrowth.random = random;
                Point3d result;
                int currentAttempt = 0;
                while (currentAttempt < NumAttempt)
                {
                    if (!node.IsActive) break;
                    if (angleControlledGrowth.Next(random.NextDouble() * (MaxDistance - MinDistance) + MinDistance, out result))
                    {
                        List<Line> lines = Graph.NetworkEdgesSimpleGeometry;
                        List<int> indices = new List<int>();
                        Line newLine = new Line(node.Point, result);
                        /*
                        List<double> parameters;
                        bool intersects = SweepLineIntersection.TempLineNetworkIntersection(newLine, lines, out parameters);
                        if (intersects)
                        {
                            result = newLine.PointAt(parameters.Min());
                        }
                        */
                        var resultNode = new NetworkNode(result, node.Graph, Graph.NextNodeId);
                        resultNode.PossibleGrowthsLeft = NumPossibleGrowth;
                        // actually angle constraint should come after the snap constraint ?

                        Snap snap = new Snap(node.Point, resultNode.Point, node.AllButAdjacentEdges.ConvertAll(e => new Line(e.Source.Point, e.Target.Point)), SnapDistance);
                        var snapResult = snap.Solve(out double _, out Point3d snapPoint, out int lineId);
                        if (snapResult != SnapResult.NoSnap)
                        {
                            if (snapResult == SnapResult.Ends)
                            {
                                NetworkNode snapped = Graph.Graph.Vertices.ToList().Find(v => v.Point.EpsilonEquals(snapPoint, GlobalSettings.AbsoluteTolerance));
                                // post generation angle compliance
                                // post generation length compliance (minLength > snapDistance)
                                if (angleControlledGrowth.PostGenerationCompliance(snapped.Point) && snapped.Point.DistanceTo(node.Point) >= SnapDistance)
                                {
                                    snapped.PossibleGrowthsLeft = NumPossibleGrowth - 1;
                                    if (!snapped.Equals(node)) Graph.AddNetworkEdge(new NetworkEdge(node, snapped, Graph, Graph.NextEdgeId));
                                }
                            }
                            else if (snapResult == SnapResult.Midway)
                            {
                                NetworkNode snapped = new NetworkNode(snapPoint, Graph, Graph.NextNodeId);
                                // post generation angle compliance
                                // post generation length compliance (minLength > snapDistance)
                                if (angleControlledGrowth.PostGenerationCompliance(snapped.Point) && snapped.Point.DistanceTo(node.Point) >= SnapDistance)
                                {
                                    int snappedId = Graph.AddNetworkNode(snapped);
                                    snapped = Graph.Graph.Vertices.ToList()[snappedId];
                                    snapped.PossibleGrowthsLeft = node.PossibleGrowthsLeft - 1;// ADD -1
                                    // this part is changed (above) to avoid （no vertex exception）
                                    Graph.AddNetworkEdge(new NetworkEdge(node.AllButAdjacentEdges[lineId].Source, snapped, Graph, Graph.NextEdgeId));
                                    Graph.AddNetworkEdge(new NetworkEdge(node.AllButAdjacentEdges[lineId].Target, snapped, Graph, Graph.NextEdgeId));
                                    Graph.AddNetworkEdge(new NetworkEdge(node, snapped, Graph, Graph.NextEdgeId));
                                    Graph.Graph.RemoveEdge(node.AllButAdjacentEdges[lineId]);
                                }
                            }
                        } else
                        {
                            Graph.AddNetworkNode(resultNode);
                            Graph.AddNetworkEdge(new NetworkEdge(node, resultNode, Graph, Graph.NextEdgeId));
                        }
                        
                        node.PossibleGrowthsLeft += -1;
                    } else
                    {
                        currentAttempt += 1;
                    }
                }
            }

            

        }

        
    }
}
