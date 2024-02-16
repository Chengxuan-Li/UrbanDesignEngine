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
    public class NetworkEdge : HasScriptRuntimeGeometry<Curve>, IUndirectedEdge<NetworkNode>, IEquatable<NetworkEdge>, IAttributable, IHasGHIOPreviewGeometricParam<NetworkEdge, GHIOCurveParam<NetworkEdge>, Curve>
    {
        NetworkNode NodeA;
        NetworkNode NodeB;
        public NetworkGraph Graph;
        public Attributes Attributes = new Attributes();
        public NetworkFace leftFace = null;
        public NetworkFace rightFace = null;
        public int Id;
        public override Curve Geometry => new Line(Source.Point, Target.Point).ToNurbsCurve();
        public GHIOCurveParam<NetworkEdge> gHIOParam =>  new GHIOCurveParam<NetworkEdge>() { ScriptClassVariable = this };
        public Curve UnderlyingCurve
        {
            get
            {
                if (Attributes.Contains("UnderlyingCurve"))
                {
                    Curve curve = Attributes.Get<Curve>("UnderlyingCurve");
                    if (curve.PointAtStart.DistanceTo(Source.Point) <= GlobalSettings.AbsoluteTolerance)
                    {
                        return curve;
                    } else
                    {
                        curve.Reverse();
                        return curve;
                    }
                } else
                {
                    return new Line(Source.Point, Target.Point).ToNurbsCurve();
                }
            }
        }

        public NetworkNode Source => NodeA.CompareTo(NodeB) > 0 ? NodeB : NodeA;

        public NetworkNode Target => NodeA.CompareTo(NodeB) > 0 ? NodeA : NodeB;



        public NetworkEdge(NetworkNode nodeA, NetworkNode nodeB, NetworkGraph graph, int id)
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
            if (Source.Id == other.Source.Id && Target.Id == other.Target.Id)
            {
                return true;
            } else if (Source.Id == other.Target.Id && Target.Id == other.Source.Id)
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

        /// <summary>
        /// Gets the other node of this edge different to the given node
        /// </summary>
        /// <param name="node">The given node</param>
        /// <param name="otherNode">The other node</param>
        /// <returns>true if successfully gets the other node; otherwise false</returns>
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

        public override string ToString()
        {
            return String.Format("NEdge {0}: ({1}, {2})", Id, Source.Id, Target.Id);
        }

        public Attributes AttributesInstance
        {
            get
            {
                Attributes.Set("Type", GetType().ToString());
                Attributes.Set("Id", Id.ToString());
                Attributes.Set("SourceNodeId", Source.Id.ToString());
                Attributes.Set("TargetNodeId", Target.Id.ToString());
                return Attributes;
            }
        }


    }

    public class NetworkEdgeEqualityComparer : IEqualityComparer<NetworkEdge>
    {
        public bool Equals(NetworkEdge x, NetworkEdge y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(NetworkEdge obj)
        {
            return obj.Id;
        }
    }
}
