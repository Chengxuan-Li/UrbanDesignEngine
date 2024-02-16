using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanDesignEngine.DataStructure;
using Rhino.Geometry;

namespace UrbanDesignEngine.Constraints
{
    public class AngleConstraint
    {
        public static bool MinimumAngle(NetworkNode currentNode, NetworkNode newNode, double minAngle)
        {
            List<NetworkNode> adjacentNodes = currentNode.Graph.Graph.AdjacentVertices(currentNode).ToList();
            double angle;
            bool valid = true;
            foreach (NetworkNode node in adjacentNodes)
            {
                angle = Vector3d.VectorAngle(
                    new Vector3d(
                        node.Point.X - currentNode.Point.X,
                        node.Point.Y - currentNode.Point.Y,
                    0),
                    new Vector3d(
                        newNode.Point.X - currentNode.Point.X,
                        newNode.Point.Y - currentNode.Point.Y,
                        0));
                if (minAngle > angle)
                {
                    valid = false;
                    break;
                }
            }
            return valid;
        }
    }
}
