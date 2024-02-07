using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph;
using Rhino;
using Rhino.Geometry;
using UrbanDesignEngine.Utilities;
using Rhino.Geometry.Intersect;

namespace UrbanDesignEngine.DataStructure
{
    public class NetworkGraph
    {
        public UndirectedGraph<NetworkNode, NetworkEdge> Graph = new UndirectedGraph<NetworkNode, NetworkEdge>();

        public List<NetworkFace> NetworkFaces = new List<NetworkFace>();

        public List<Point3d> NetworkNodesGeometry
        {
            get
            {
                List<Point3d> pts = new List<Point3d>();
                Graph.Vertices.ToList().ForEach(v => pts.Add(v.Point));
                return pts;
            }
        }
        public List<Curve> NetworkFacesGeometry
        {
            get
            {
                List<Curve> geo = new List<Curve>();
                NetworkFaces.ForEach(f => geo.Add(f.GetGeometry()));
                return geo;
            }
        }
        public int NextNodeId => Graph.VertexCount;
        public int NextEdgeId => Graph.EdgeCount;
        public int NextFaceId => NetworkFaces.Count;

        /// <summary>
        /// Adds a node into a network
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Index of the node in the network</returns>
        public int AddNetworkNode(NetworkNode node)
        {
            if (Graph.Vertices.Contains(node, new NetworkNodeEqualityComparer()))
            {
                int found = Graph.Vertices.ToList().FindIndex(n => n.Equals(node));
                return found;
            } else
            {
                node.Id = NextNodeId;
                Graph.AddVertex(node);
                return node.Id;
            }
        }

        /// <summary>
        /// Adds a node from a Point3d into a network
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Index of the node in the network</returns>
        public int AddNetworkNode(Point3d point)
        {
            NetworkNode node = new NetworkNode(point, this, NextNodeId);
            return AddNetworkNode(node);
        }


        public int AddNetworkEdge(NetworkEdge edge)
        {
            if (Graph.Edges.Contains(edge, new NetworkEdgeEqualityComparer()))
            {
                int found = Graph.Edges.ToList().FindIndex(e => e.Equals(edge));
                return found;
            } else
            {
                edge.Id = NextEdgeId;
                Graph.AddEdge(edge);
                return edge.Id;
            }
        }

        public int AddNetworkEdge(Point3d pointA, Point3d pointB)
        {
            int indexA = AddNetworkNode(pointA);
            int indexB = AddNetworkNode(pointB);
            NetworkNode nodeA = Graph.Vertices.ToList()[indexA];
            NetworkNode nodeB = Graph.Vertices.ToList()[indexB];
            return AddNetworkEdge(new NetworkEdge(nodeA, nodeB, this, -1));
        }

        public List<Line> GetLines()
        {
            List<Line> lines = new List<Line>();
            Graph.Edges.ToList().ForEach(e => lines.Add(new Line(e.Source.Point, e.Target.Point)));
            return lines;
        }

        public void SolveFaces()
        {
            for(int i = 0; i < Graph.Edges.ToList().Count; i++)
            {
                NetworkEdge edge = Graph.Edges.ToList()[i];
                if (edge.leftFace == null)
                {
                    NetworkFace face = new NetworkFace(edge, true, NextFaceId);
                    if (face.DevelopmentResult && face.IsComplete)
                    {
                        NetworkFaces.Add(face);
                    }
                }
                if (edge.rightFace == null)
                {
                    NetworkFace face = new NetworkFace(edge, false, NextFaceId);
                    if (face.DevelopmentResult && face.IsComplete)
                    {
                        NetworkFaces.Add(face);
                    }
                }
            }
        }


    }
}
