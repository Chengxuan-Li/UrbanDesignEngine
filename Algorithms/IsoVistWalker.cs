using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Algorithms
{
    public class IsoVistWalker
    {
        public IsoVist IsoVist;
        public Point3d CurrentPoint;
        public double StepLength;
        public IsoVistWalker(IsoVist isoVist, Point3d origin, double stepLength)
        {
            IsoVist = isoVist;
            CurrentPoint = origin;
            StepLength = stepLength;
        }

        void DriftWalk()
        {
            Point3d centroid = IsoVist.Compute(CurrentPoint).Centroid;
            Vector3d vec = new Vector3d(-CurrentPoint + centroid);
            vec.Unitize();
            vec = vec * StepLength;
            CurrentPoint = CurrentPoint + vec;
            return;
        }

        public List<Point3d> DriftWalker(int iterations)
        {
            List<Point3d> pts = new List<Point3d>();
            pts.Add(CurrentPoint);
            for (int i = 0; i < iterations; i++)
            {
                DriftWalk();
                pts.Add(CurrentPoint);
            }
            return pts; 
        }
    }
}
