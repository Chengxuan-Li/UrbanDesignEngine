using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

namespace UrbanDesignEngine.Utilities
{
    public class OffsetCurve
    {
        public static Curve OffsetWithDirection(Curve curve, double distance, bool inwards)
        {
            // if a closed curve is in an anti-clockwise direction, the positive offset value will result in a curve on its outside
            Polyline pl;
            if (!curve.TryGetPolyline(out pl))
            {
                if (IsAntiClockwise(curve))
                {
                    return curve.Offset(Plane.WorldXY, inwards ? -distance : distance, GlobalSettings.AbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
                }
                else
                {
                    return curve.Offset(Plane.WorldXY, inwards ? distance : -distance, GlobalSettings.AbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
                }
            }
            
            if (IsAntiClockwise(pl))
            {
                return curve.Offset(Plane.WorldXY, inwards ? -distance : distance, GlobalSettings.AbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
            } else
            {
                return curve.Offset(Plane.WorldXY, inwards ? distance : -distance, GlobalSettings.AbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
            }
        }

        public static bool IsAntiClockwise(Polyline polyline)
        {
            return IsAntiClockwise(polyline.ToList());
        }

        public static bool IsAntiClockwise(Curve curve)
        {
            List<Point3d> controlPts = new List<Point3d>();
            curve.ToNurbsCurve().Points.ToList().ForEach(p => controlPts.Add(p.Location));
            return IsAntiClockwise(controlPts);

        }

        public static bool IsAntiClockwise(List<Point3d> pts)
        {
            List<double> anglesTurned = new List<double>();
            for (int i = 0; i < pts.Count - 2; i++)
            {
                anglesTurned.Add(Trigonometry.AngleTurned(pts[i], pts[i + 1], pts[i + 1], pts[i + 2]));
            }
            anglesTurned.Add(Trigonometry.AngleTurned(pts[pts.Count - 2], pts[pts.Count - 1], pts[0], pts[1]));
            return anglesTurned.Sum() >= 2 * Math.PI && anglesTurned.Sum() <= 4 * Math.PI;
        }

    }
}
