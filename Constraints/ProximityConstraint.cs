using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Constraints
{
    public class ProximityConstraint
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="snapDistance"></param>
        /// <returns>true if snapped to an existing node</returns>
        public static bool Snap(NetworkNode node, double snapDistance)
        {
            List<double> qualifiedDistances = new List<double>();
            List<NetworkNode> qualifiedNodes = new List<NetworkNode>();
            foreach (NetworkNode n in node.Graph.Vertices.ToList())
            {
                double distance = n.Point.DistanceTo(node.Point);
                if (distance < snapDistance)
                {
                    qualifiedDistances.Add(distance);
                    qualifiedNodes.Add(n);
                }
            }
            if (qualifiedDistances.Count > 0)
            {
                int i = qualifiedDistances.FindIndex(x => x == qualifiedDistances.Min());
                node.Point = qualifiedNodes[i].Point;
                return true;
            } else
            {
                return false;
            }
}
    }
}
