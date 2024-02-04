using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph;
using Rhino;
using Rhino.Geometry;
using UrbanDesignEngine.Utilities;

namespace UrbanDesignEngine.DataStructure
{
    public class NetworkNode : IComparable<NetworkNode>, IEquatable<NetworkNode>
    {
        public Point3d Point;
        public UndirectedGraph<NetworkNode, NetworkEdge> Graph;
        public List<NetworkFace> Faces = new List<NetworkFace>();
        public bool IsActive => PossibleGrowthsLeft > 0;
        public int PossibleGrowthsLeft = 2;
        public int Generation = -1;

        public int Id;

        public List<double> Angles
        {
            get
            {
                List<double> angles = new List<double>();
                Graph.AdjacentEdges(this).ToList().ForEach(e => angles.Add(Trigonometry.Angle(this, e.TryGetOtherNode(this))));
                return angles;
            }
        }

        public NetworkNode(Point3d point, UndirectedGraph<NetworkNode, NetworkEdge> graph, int id)
        {
            Point = point;
            Graph = graph;
            Id = id;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="traverseDirection">direction in which the face loop is traversing; true of anti-closewise; otherwise false</param>
        /// <param name="nextEdge"></param>
        /// <param name="nextEdgeTraverseDirection"></param>
        /// <returns></returns>
        public bool NextEdge(NetworkEdge edge, bool traverseDirection, out NetworkEdge nextEdge, out bool nextEdgeTraverseDirection)
        {
            NetworkNode otherNode;
            nextEdge = null;
            nextEdgeTraverseDirection = false;
            if (!edge.OtherNode(this, out otherNode)) return false;
            double angle = Trigonometry.Angle(this, otherNode);
            NetworkEdge smaller;
            NetworkEdge bigger;
            if (Angles.Count == 0)
            {
                return false;
            }
            DataManagement.Nearest<double, NetworkEdge>(angle, Angles, Graph.AdjacentEdges(this).ToList(), out smaller, out bigger);
            if (traverseDirection) // anti-clockwise face loop
            {
                nextEdge = smaller;
            } else // clock-wise face loop
            {
                nextEdge = bigger;
            }
            nextEdgeTraverseDirection = nextEdge.IsSource(this);
            return true;
        }
    }
}
