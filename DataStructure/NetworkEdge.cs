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

        public NetworkNode Source => NodeA.CompareTo(NodeB) > 0 ? NodeB : NodeA ;

        public NetworkNode Target => NodeA.CompareTo(NodeB) > 0 ? NodeA : NodeB;

        public NetworkEdge(NetworkNode nodeA, NetworkNode nodeB, UndirectedGraph<NetworkNode, NetworkEdge> graph)
        {
            NodeA = nodeA;
            NodeB = nodeB;
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
    }
}
