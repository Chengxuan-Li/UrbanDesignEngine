﻿using System;
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
        public NetworkGraph Graph;
        public List<NetworkFace> Faces = new List<NetworkFace>();
        public bool IsActive => PossibleGrowthsLeft > 0;
        public int PossibleGrowthsLeft = 2;
        public int Generation = -1;

        public int Id;

        /// <summary>
        /// The list of directional angles from this point in the direction of each adjacent edge
        /// </summary>
        public List<double> Angles
        {
            get
            {
                List<double> angles = new List<double>();
                Graph.Graph.AdjacentEdges(this).ToList().ForEach(e => angles.Add(Trigonometry.Angle(this, e.TryGetOtherNode(this))));
                return angles;
            }
        }

        public NetworkNode(Point3d point, NetworkGraph graph, int id)
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
        /// <param name="nextEdge"></param>
        /// <param name="nextEdgeTraverseDirection"></param>
        /// <returns></returns>
        public bool NextEdge(NetworkEdge edge, out NetworkEdge nextEdge, out bool nextEdgeTraverseDirection)
        {
            NetworkNode otherNode;
            nextEdge = null;
            nextEdgeTraverseDirection = false;
            if (!edge.OtherNode(this, out otherNode)) return false;
            double angle = Trigonometry.Angle(this, otherNode);
            NetworkEdge smaller;
            if (Angles.Count == 0)
            {
                return false;
            }
            if (Angles.Count == 1)
            {
                nextEdge = edge;
                nextEdgeTraverseDirection = nextEdge.IsSource(this);
            } else
            {
                DataManagement.Nearest<double, NetworkEdge>(angle, Angles, Graph.Graph.AdjacentEdges(this).ToList(), out smaller, out NetworkEdge bigger);
                nextEdge = smaller;
                nextEdgeTraverseDirection = nextEdge.IsSource(this);
            }
            return true;
        }

        public override string ToString()
        {
            return String.Format("NNode {0}", Id);
        }
    }
}
