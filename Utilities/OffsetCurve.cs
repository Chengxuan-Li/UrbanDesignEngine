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
            Curve c = default;
            Polyline pl;
            if (!curve.TryGetPolyline(out pl)) return c;
            
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
            List<double> anglesTurned = new List<double>();
            for (int i = 0; i < polyline.Count - 2; i++)
            {
                anglesTurned.Add(Trigonometry.AngleTurned(polyline[i], polyline[i + 1], polyline[i + 1], polyline[i + 2]));
            }
            anglesTurned.Add(Trigonometry.AngleTurned(polyline[polyline.Count - 2], polyline[polyline.Count - 1], polyline[0], polyline[1]));
            return anglesTurned.Sum() >= 2 * Math.PI && anglesTurned.Sum() <= 4 * Math.PI;
        }

    }
}
