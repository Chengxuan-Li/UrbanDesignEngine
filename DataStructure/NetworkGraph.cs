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
        public List<NetworkNode> networkNodes = new List<NetworkNode>();
        public List<NetworkEdge> networkEdges = new List<NetworkEdge>();

        public bool AddNetworkNode(NetworkNode node)
        {
            return Graph.AddVertex(node);
        }

        public bool AddNetworkNodeFromPoint(Point3d point)
        {
            NetworkNode node = new NetworkNode(point, Graph);
            return AddNetworkNode(node);
        }

        public bool AddNetworkEdge(NetworkEdge edge)
        {
            return Graph.AddEdge(edge);
        }

        public List<Line> GetLines()
        {
            List<Line> lines = new List<Line>();
            Graph.Edges.ToList().ForEach(e => lines.Add(new Line(e.Source.Point, e.Target.Point)));
            return lines;
        }

    }
}
