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
    public class NetworkGraph
    {
        public UndirectedGraph<NetworkNode, NetworkEdge> Graph = new UndirectedGraph<NetworkNode, NetworkEdge>();
        public List<NetworkNode> NetworkNodes = new List<NetworkNode>();
        public List<NetworkEdge> NetworkEdges = new List<NetworkEdge>();
        public List<NetworkFace> NetworkFaces = new List<NetworkFace>();

        public int NextNodeId => NetworkNodes.Count;
        public int NextEdgeId => NetworkEdges.Count;
        public int NextFaceId => NetworkFaces.Count;

        public bool AddNetworkNode(NetworkNode node)
        {
            node.Id = NextNodeId;
            return Graph.AddVertex(node);
        }

        public bool AddNetworkNodeFromPoint(Point3d point)
        {
            NetworkNode node = new NetworkNode(point, Graph, NextNodeId);
            return AddNetworkNode(node);
        }

        public bool AddNetworkEdge(NetworkEdge edge)
        {
            edge.Id = NextEdgeId;
            return Graph.AddEdge(edge);
        }

        public List<Line> GetLines()
        {
            List<Line> lines = new List<Line>();
            Graph.Edges.ToList().ForEach(e => lines.Add(new Line(e.Source.Point, e.Target.Point)));
            return lines;
        }

        public void SolveFaces()
        {
            foreach(NetworkEdge edge in Graph.Edges)
            {
                if (edge.leftFace == null)
                {
                    NetworkFaces.Add(new NetworkFace(edge, true));
                }
                if (edge.rightFace == null)
                {
                    NetworkFaces.Add(new NetworkFace(edge, false));
                }
            }
        }

    }
}
