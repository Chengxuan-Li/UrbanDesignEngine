using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.Utilities;

namespace UrbanDesignEngine.Growth
{
    public class AngleControlledGrowth
    {
        public NetworkNode Node;
        public List<double> Angles = new List<double>();
        public double MinimumAngle;
        public Random random = new Random();

        public AngleControlledGrowth(NetworkNode node, double minimumAngle)
        {
            Node = node;
            MinimumAngle = minimumAngle;
            
        }

        public bool Next(double distance, out NetworkNode result)
        {
            
            double nextAngle = random.NextDouble() * 2 * Math.PI;
            bool valid = true;
            Angles = new List<double>();
            Node.Graph.AdjacentVertices(Node).ToList().ForEach(n => Angles.Add(Trigonometry.Angle(Node, n)));

            foreach (double angle in Angles)
            {
                if (Trigonometry.Angle(angle, nextAngle) < MinimumAngle)
                {
                    valid = false;
                    break;
                }
            }
            result = new NetworkNode(
                new Rhino.Geometry.Point3d(
                    Node.Point.X + distance * Math.Cos(nextAngle),
                    Node.Point.Y + distance * Math.Sin(nextAngle),
                    0),
                Node.Graph
                );
            return valid;
        }
    }
}
