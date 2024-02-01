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
        public int NumAttempt = 5;
        Random random = new Random();


        int currentIteration = 0;
        public LSystem()
        {


        }

        public LSystem(Point3d origin)
        {
            Graph.AddNetworkNodeFromPoint(origin);
        }

        public void Solve()
        {
            while (currentIteration < Iterations)
            {
                GrowAll();
                currentIteration++;
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
                AngleControlledGrowth angleControlledGrowth = new AngleControlledGrowth(node, MinimumAngle);
                angleControlledGrowth.random = random;
                NetworkNode result;
                int currentAttempt = 0;
                while (currentAttempt < NumAttempt)
                {
                    if (!node.IsActive) break;
                    if (angleControlledGrowth.Next(random.NextDouble() * (MaxDistance - MinDistance) + MinDistance, out result))
                    {
                        List<Line> lines = Graph.GetLines();
                        List<int> indices = new List<int>();
                        Line newLine = new Line(node.Point, result.Point);
                        List<double> parameters;
                        bool intersects = SweepLineIntersection.TempLineNetworkIntersection(newLine, lines, out parameters);
                        if (intersects)
                        {
                            
                            result.Point = newLine.PointAt(parameters.Min());
                        }
                        ProximityConstraint.Snap(result, SnapDistance);
                        Graph.AddNetworkNode(result);
                        Graph.AddNetworkEdge(new NetworkEdge(node, result, Graph.Graph));
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
