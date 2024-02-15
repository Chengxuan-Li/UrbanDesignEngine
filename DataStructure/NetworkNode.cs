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
    public class NetworkNode : HasScriptRuntimeGeometry<Point>, IComparable<NetworkNode>, IEquatable<NetworkNode>, IAttributable, IHasGHIOPreviewGeometricParam<NetworkNode, GHIOPointParam<NetworkNode>, Point>
    {
        public Point3d Point;
        public NetworkGraph Graph;
        public Attributes Attributes = new Attributes();
        public List<NetworkFace> Faces = new List<NetworkFace>();
        public bool IsActive => PossibleGrowthsLeft > 0;
        public int PossibleGrowthsLeft = 2;
        public int Generation = -1;

        public int Id;

        public override Point Geometry => new Point(Point);

        public GHIOPointParam<NetworkNode> gHIOParam => new GHIOPointParam<NetworkNode>() { ScriptClassVariable = this };

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

        public List<int> AdjacentNodeIds
        {
            get
            {
                List<int> ids = new List<int>();
                Graph.Graph.AdjacentVertices(this).ToList().ForEach(v => ids.Add(v.Id));
                return ids;
            }
        }

        public List<int> AdjacentEdgeIds
        {
            get
            {
                List<int> ids = new List<int>();
                Graph.Graph.AdjacentEdges(this).ToList().ForEach(e => ids.Add(e.Id));
                return ids;
            }
        }

        public List<int> AdjacentFaceIds
        {
            get
            {
                List<int> ids = new List<int>();
                Faces.ToList().ForEach(f => ids.Add(f.Id));
                return ids;
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
            } else if (this.Id == other.Id)
            {
                return 0;
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

    public class NetworkNodeEqualityComparer : IEqualityComparer<NetworkNode>
    {
        public bool Equals(NetworkNode x, NetworkNode y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(NetworkNode obj)
        {
            return obj.Id;
        }
    }
}
