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

        Random random = new Random();                                                                                                                                                                  
        public List<Curve> FaceCurves
        {
            get
            {
                List<Curve> crvs = new List<Curve>();
                Graph.NetworkFaces.ForEach(f => crvs.Add(f.SimpleGeometry()));
                return crvs;
            }
        }


        int currentIteration = 0;
        public LSystem()
        {


        }

        public LSystem(Point3d origin)
        {
            Graph.AddNetworkNode(origin);
        }

        public void Solve()
        {
            while (currentIteration < Iterations)
            {
                GrowAll();
                currentIteration++;
            }
            //Graph.SolveFaces();
            // TODO make all edges, faces, nodes a unique index
            
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
                        List<double> parameters;
                        bool intersects = SweepLineIntersection.TempLineNetworkIntersection(newLine, lines, out parameters);
                        if (intersects)
                        {
                            result = newLine.PointAt(parameters.Min());
                        }
                        var resultNode = new NetworkNode(result, node.Graph, Graph.NextNodeId);
                        resultNode.PossibleGrowthsLeft = NumPossibleGrowth;
                        if (ProximityConstraint.Snap(resultNode, SnapDistance))
                        {
                            NetworkNode snapped = Graph.Graph.Vertices.ToList().Find(v => v.Equals(resultNode));
                            snapped.PossibleGrowthsLeft = NumPossibleGrowth;
                            if (!snapped.Equals(node)) Graph.AddNetworkEdge(new NetworkEdge(node, snapped, Graph, Graph.NextEdgeId));
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
