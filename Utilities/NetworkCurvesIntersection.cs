using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Utilities
{
    public class NetworkCurvesIntersection
    {
        public List<Curve> Curves;
        public NetworkGraph Graph;

        List<CurveDelegate> delegates = new List<CurveDelegate>();
        
        public List<double> GetCurveParameters(int index)
        {
            CurveDelegate cd = delegates[index];
            return cd.SortedIntersectionParameters;
        }

        public static NetworkGraph NetworkGraphFromCurves(List<Curve> curves)
        {
            NetworkCurvesIntersection nci = new NetworkCurvesIntersection(curves);
            // TODO
            return nci.Graph;
        }

        public NetworkCurvesIntersection(List<Curve> curves)
        {
            Curves = curves;
            Curves.ForEach(c => delegates.Add(new CurveDelegate(c)));
            Graph = new NetworkGraph();

            int count = curves.Count;
            double tolerance = GlobalSettings.AbsoluteTolerance;
            for (int i = 0; i < count; i++)
            {
                for (int j = i; j < count; j++)
                {
                    if (i != j)
                    {
                        CurveIntersections intersectionEvents = Intersection.CurveCurve(Curves[i], Curves[j], tolerance, tolerance);
                        foreach (IntersectionEvent e in intersectionEvents)
                        {
                            if (e.IsPoint)
                            {
                                delegates[i].AddParameter(e.ParameterA);
                                delegates[j].AddParameter(e.ParameterB);

                                Graph.AddNetworkNode(Curves[i].PointAt(e.ParameterA));
                            } else if (e.IsOverlap)
                            {
                                delegates[i].AddParameter(e.OverlapA.Min);
                                delegates[i].AddParameter(e.OverlapA.Max);
                                delegates[j].AddParameter(e.OverlapB.Min);
                                delegates[j].AddParameter(e.OverlapB.Max);

                                Graph.AddNetworkNode(Curves[i].PointAt(e.OverlapA.Min));
                                Graph.AddNetworkNode(Curves[i].PointAt(e.OverlapA.Max));
                            }
                        }
                    }
                }

            }

            for (int i = 0; i < count; i++)
            {
                Curve c = Curves[i];
                List<double> ps = GetCurveParameters(i);
                for (int j = 0; j < ps.Count - 1; j++)
                {
                    Graph.AddNetworkEdge(c.PointAt(ps[j]), c.PointAt(ps[j + 1]));
                }
            }
            
        }

        internal class CurveDelegate
        {
            public Curve curve;
            List<double> IntersectionParameters = new List<double>();
            public List<double> SortedIntersectionParameters
            {
                get
                {
                    DoubleEqualityComparer comparer = new DoubleEqualityComparer();
                    List<double> ps = IntersectionParameters.Distinct(comparer).ToList();
                    ps.Sort();
                    return ps;
                }
            }

            public CurveDelegate(Curve curve)
            {
                IntersectionParameters.Add(curve.Domain.Min);
                IntersectionParameters.Add(curve.Domain.Max);
            }

            public void AddParameter(double p)
            {
                IntersectionParameters.Insert(IntersectionParameters.Count - 2, p);
            }

            public void AddParameters(List<double> pp)
            {
                IntersectionParameters.InsertRange(IntersectionParameters.Count - 2, pp);
            }
        }
    }
}
