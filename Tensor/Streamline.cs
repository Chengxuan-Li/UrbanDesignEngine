using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Tensor
{
    public class Streamline
    {
        public MultipleTensorFields TFs;

        public Streamline(MultipleTensorFields tfs)
        {
            TFs = tfs;
        }

        public bool Evaluate(Point3d pt, int hierarchy, bool major, double stepLength, out Point3d nextPt)
        {
            bool result = TFs.Evaluate(hierarchy, pt, out Vector3d majorVector, out Vector3d minorVector, out double scalar);
            nextPt = new Point3d(pt + scalar * stepLength * (major ? majorVector : minorVector));
            return result;
        }

        public Polyline Evaluate(Point3d pt, int hierarchy, bool major, double stepLength, int iterations)
        {
            
            List<Point3d> pts = new List<Point3d>();
            pts.Add(pt);
            Point3d ptnext = pt;
            for (int i = 0; i < iterations; i++)
            {
                if (Evaluate(ptnext, hierarchy, major, stepLength, out Point3d nextPt))
                {
                    pts.Add(nextPt);
                    ptnext = nextPt;
                } else
                {
                    break;
                }
            }
            Polyline pl = (pts.Count >= 2) ? new Polyline(pts) : default;
            return pl;
        }
    }
}
