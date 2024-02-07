using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph;
using Rhino.Geometry;

namespace UrbanDesignEngine.DataStructure
{
    public class NetworkFace : IEquatable<NetworkFace>
    {
        public NetworkGraph Graph;
        public int Id;
        public List<NetworkNode> NodesTraversed = new List<NetworkNode>();
        public List<NetworkEdge> EdgesTraversed = new List<NetworkEdge>();
        public List<bool> EdgesTraverseDirection = new List<bool>();

        public bool IsComplete
            => (
            NodesTraversed.Count > 1
            && Graph.Graph.AdjacentVertices(NodesTraversed[0]).ToList().Contains(
                NodesTraversed[NodesTraversed.Count - 1]
                )
            && EdgesTraversed.Count == NodesTraversed.Count
            && EdgesTraverseDirection.Count == EdgesTraversed.Count
            );

        public bool IsTraverseable
        {
            get
            {
                if (!IsComplete) return false;
                NetworkNode node = NodesTraversed[0];
                NetworkNode nextNode;
                for (int i = 0; i < NodesTraversed.Count; i++)
                {
                    if (!QueryNext(node, out nextNode, out NetworkEdge _, out bool __)) return false;
                    node = nextNode;
                }
                return node.Equals(NodesTraversed[0]);
            }
        }

        public NetworkFace(NetworkEdge edge, bool direction, int id)
        {
            Graph = edge.Graph;
            Id = id;
            NodesTraversed.Add(direction ? edge.Source : edge.Target);
            NodesTraversed.Add(direction ? edge.Target : edge.Source);
            EdgesTraversed.Add(edge);
            EdgesTraverseDirection.Add(direction);
            bool developing = true;
            int iterations = 0;
            while (developing)
            {
                developing = DevelopNext();
                iterations++;
                if (iterations > 25)
                {
                    break;
                }
            }
            if (IsComplete && IsTraverseable)
            {
                NodesTraversed.ForEach(n => n.Faces.Add(this));
                foreach (NetworkEdge e in EdgesTraversed)
                {
                    if (EdgesTraverseDirection[EdgesTraversed.IndexOf(e)])
                    {
                        e.leftFace = this;
                    } else
                    {
                        e.rightFace = this;
                    }
                }
            }
        }

        bool DevelopNext()
        {
            NetworkEdge nextEdge;
            bool nextEdgeTraverseDirection;
            if (!NodesTraversed[NodesTraversed.Count - 1].NextEdge(EdgesTraversed[EdgesTraversed.Count - 1], out nextEdge, out nextEdgeTraverseDirection)) return false;
            if (nextEdge.Equals(EdgesTraversed[0])) return false;
            EdgesTraversed.Add(nextEdge);
            EdgesTraverseDirection.Add(nextEdgeTraverseDirection);
            NetworkNode nextNode = (nextEdgeTraverseDirection) ? nextEdge.Target : nextEdge.Source;
            if (nextNode.Equals(NodesTraversed[0])) return false;
            NodesTraversed.Add(nextNode);
            return true;
        }

        public bool QueryNext(NetworkNode node, out NetworkNode nextNode, out NetworkEdge nextEdge, out bool nextEdgeDirection)
        {
            int index = NodesTraversed.FindIndex(n => n == node);

            if (index >= 0)
            {
                nextNode = NodesTraversed[(NodesTraversed.Count > index + 1) ? index + 1 : 0];
                nextEdge = EdgesTraversed[index];
                nextEdgeDirection = EdgesTraverseDirection[index];
                if (nextEdge.TraverseTarget(nextEdgeDirection).Equals(nextNode))
                {
                    return true;
                } else
                {
                    return false;
                }
            } else
            {
                nextNode = null;
                nextEdge = null;
                nextEdgeDirection = false;
                return false;
            }

        }
        public bool Equals(NetworkFace other)
        {
            if (!IsComplete) return false;
            if (!other.IsComplete) return false;
            if (NodesTraversed.Count != other.NodesTraversed.Count) return false;
            if (!IsTraverseable) return false;
            if (!other.IsTraverseable) return false;
            NetworkNode node = NodesTraversed[0];
            int index = other.NodesTraversed.FindIndex(n => n.Equals(node));
            if (index < 0) return false;
            NetworkNode nextNode;
            NetworkNode otherNextNode;
            for (int i = 0; i < NodesTraversed.Count; i++)
            {
                if (QueryNext(node, out nextNode, out NetworkEdge _, out bool __) && other.QueryNext(node, out otherNextNode, out NetworkEdge ___, out bool ____))
                {
                    if (nextNode.Equals(otherNextNode))
                    {
                        node = nextNode;
                    } else
                    {
                        return false;
                    }
                } else
                {
                    return false;
                }
            }
            return true;
        }

        public Curve GetGeometry()
        {
            List<Point3d> pts = new List<Point3d>();
            NodesTraversed.ForEach(n => pts.Add(n.Point));
            var pl = new Polyline(pts);
            return pl.ToNurbsCurve();
        }

        public override string ToString()
        {
            return String.Format("NFace {0}", Id);
        }
    }
}
