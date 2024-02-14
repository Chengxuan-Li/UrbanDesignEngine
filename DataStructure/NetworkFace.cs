using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph;
using Rhino.Geometry;
using UrbanDesignEngine.Utilities;

namespace UrbanDesignEngine.DataStructure
{
    public class NetworkFace : IEquatable<NetworkFace>, IAttributable
    {
        public NetworkGraph Graph;
        public Attributes Attributes = new Attributes();
        public int Id;
        public List<NetworkNode> NodesTraversed = new List<NetworkNode>();
        public List<NetworkEdge> EdgesTraversed = new List<NetworkEdge>();
        public List<bool> EdgesTraverseDirection = new List<bool>();
        public Point3d Centroid => AreaMassProperties.Compute(SimpleGeometry()).Centroid;

        public Curve UnderlyingGeometry
        {
            get
            {
                List<Curve> curves = new List<Curve>();
                EdgesTraversed.ForEach(e => curves.Add(e.UnderlyingCurve));
                return Curve.JoinCurves(curves)[0];
            }
        }

        public List<double> AnglesTurned
        {
            get
            {
                if (DevelopmentResult && IsComplete)
                {
                    List<double> angles = new List<double>();
                    int count = EdgesTraversed.Count;
                    for (int i = 0; i < count - 1; i++)
                    {
                        angles.Add(Trigonometry.AngleTurned(EdgesTraversed[i], EdgesTraversed[i + 1], EdgesTraverseDirection[i], EdgesTraverseDirection[i + 1]));
                    }
                    angles.Add(Trigonometry.AngleTurned(EdgesTraversed[count - 1], EdgesTraversed[0], EdgesTraverseDirection[count - 1], EdgesTraverseDirection[0]));
                    return angles;
                } else
                {
                    return new List<double>();
                }
                
            }
        }

        
        public bool IsAntiClockWise => AnglesTurned.Sum() >= 2*Math.PI && AnglesTurned.Sum() <= 4 * Math.PI; // TODO Check

        enum DevelopmentStatus
        {
            Developing,
            Stopped,
            Finished,
        }
        public bool IsComplete
            => (
            NodesTraversed.Count > 2
            && Graph.Graph.AdjacentVertices(NodesTraversed[0]).ToList().Contains(
                NodesTraversed[NodesTraversed.Count - 1]
                )
            && EdgesTraversed.Count == NodesTraversed.Count
            && EdgesTraverseDirection.Count == EdgesTraversed.Count
            );

        public bool DevelopmentResult = false;

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
            DevelopmentStatus status = DevelopmentStatus.Developing;
            int iterations = 0;
            while (status == DevelopmentStatus.Developing)
            {
                status = DevelopNext();
                iterations++;
                if (iterations > 250)
                {
                    break;
                }
            }
            DevelopmentResult = status == DevelopmentStatus.Finished;
            if (status == DevelopmentStatus.Finished && IsComplete)
            {
                NodesTraversed.ForEach(n => n.Faces.Add(this));
                for (int i = 0; i < EdgesTraversed.Count; i++)
                {
                    NetworkEdge e = EdgesTraversed[i];
                    if (EdgesTraverseDirection[i])
                    {
                        e.leftFace = this;
                    } else
                    {
                        e.rightFace = this;
                    }
                }
            }
        }

        DevelopmentStatus DevelopNext()
        {
            NetworkEdge nextEdge;
            bool nextEdgeTraverseDirection;
            // this will only return false if the the node contains only 0 angles which is impossible
            if (!NodesTraversed[NodesTraversed.Count - 1].NextEdge(EdgesTraversed[EdgesTraversed.Count - 1], out nextEdge, out nextEdgeTraverseDirection)) return DevelopmentStatus.Stopped;
            if (nextEdge.Equals(EdgesTraversed[0]))
            {
                //return false;
            }
            EdgesTraversed.Add(nextEdge);
            EdgesTraverseDirection.Add(nextEdgeTraverseDirection);
            NetworkNode nextNode = (nextEdgeTraverseDirection) ? nextEdge.Target : nextEdge.Source;
            if (nextNode.Equals(NodesTraversed[0])) return DevelopmentStatus.Finished;
            NodesTraversed.Add(nextNode);
            return DevelopmentStatus.Developing;
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

        public Curve SimpleGeometry()
        {
            
            List<Point3d> pts = new List<Point3d>();
            NodesTraversed.ForEach(n => pts.Add(n.Point));
            pts.Add(NodesTraversed[0].Point);
            var pl = new Polyline(pts);
            return pl.ToNurbsCurve();
            /*
            List<Curve> cs = new List<Curve>();
            EdgesTraversed.ForEach(e => cs.Add(new Line(e.Source.Point, e.Target.Point).ToNurbsCurve()));
            return Curve.JoinCurves(cs)[0];
            */
        }

        public override string ToString()
        {
            return String.Format("NFace {0}", Id);
        }

        public Attributes GetAttributesInstance()
        {
            return Attributes;
        }

        public void SetAttribute(string key, object val)
        {
            Attributes.Set(key, val);
        }

        public T GetAttribute<T>(string key)
        {
            return Attributes.Get<T>(key);
        }

        public bool TryGetAttribute<T>(string key, out T val)
        {
            return Attributes.TryGet<T>(key, out val);
        }

    }
}
