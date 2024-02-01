using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph;
using Rhino;
using Rhino.Geometry;

namespace UrbanDesignEngine.DataStructure
{
    public class NetworkNode : IComparable<NetworkNode>, IEquatable<NetworkNode>
    {
        public Point3d Point;
        public UndirectedGraph<NetworkNode, NetworkEdge> Graph;
        public bool IsActive => PossibleGrowthsLeft > 0;
        public int PossibleGrowthsLeft = 2;
        public int Generation = -1;

        public NetworkNode(Point3d point, UndirectedGraph<NetworkNode, NetworkEdge> graph)
        {
            Point = point;
            Graph = graph;
        }

        public int CompareTo(NetworkNode other)
        {
            if (other == null)
            {
                return -1;
            } else if (Point.DistanceTo(other.Point) <= GlobalSettings.AbsoluteTolerance)
            {
                return 0;
            } else
            {
                return Point.CompareTo(other.Point);
            }
        }

        public bool Equals(NetworkNode other)
        {
            return CompareTo(other) == 0;
        }
    }
}
