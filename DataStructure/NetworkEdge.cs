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
    public class NetworkEdge : IUndirectedEdge<NetworkNode>, IEquatable<NetworkEdge>
    {
        NetworkNode NodeA;
        NetworkNode NodeB;
        public UndirectedGraph<NetworkNode, NetworkEdge> Graph;
        public NetworkFace leftFace = null;
        public NetworkFace rightFace = null;
        public int Id;

        public NetworkNode Source => NodeA.CompareTo(NodeB) > 0 ? NodeB : NodeA;

        public NetworkNode Target => NodeA.CompareTo(NodeB) > 0 ? NodeA : NodeB;

        public NetworkEdge(NetworkNode nodeA, NetworkNode nodeB, UndirectedGraph<NetworkNode, NetworkEdge> graph, int id)
        {
            NodeA = nodeA;
            NodeB = nodeB;
            Graph = graph;
            Id = id;
        }

        public bool Equals(NetworkEdge other)
        {
            if (other == null)
            {
                return false;
            }
            if (Source.Equals(other.Source) && Target.Equals(other.Target))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public NetworkNode TraverseSource(bool direction)
        {
            return direction ? Source : Target;
        }

        public NetworkNode TraverseTarget(bool direction)
        {
            return direction ? Target : Source;
        }

        public bool OtherNode(NetworkNode node, out NetworkNode otherNode)
        {
            if (node.Equals(Source))
            {
                otherNode = Target;
                return true;
            } else if (node.Equals(Target))
            {
                otherNode = Source;
                return true;
            } else
            {
                otherNode = node;
                return false;
            }
        }

        public NetworkNode TryGetOtherNode(NetworkNode node)
        {
            NetworkNode otherNode;
            OtherNode(node, out otherNode);
            return otherNode;
        }

        public bool IsSource(NetworkNode node)
        {
            return node.Equals(Source);
        }
    }
}
