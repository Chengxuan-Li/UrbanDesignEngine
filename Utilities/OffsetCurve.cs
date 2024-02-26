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

        public static List<Curve> OffsetVariable(List<Curve> curves, List<double> distances)
        {
            // here we need a simplification - to be improved later!
            // incurve - multiple intersections - outcurve => incurve - last intersection - outcurve
            // acknowledged: multiple intersection should have the ideal resultant curve as sth like in-out-in-out-out
            // this will need point-curve-side checks to determine which side a point (on the subcurve of the other curve) lies.
            // to be improved in the future!
            //          --        ----- --------        ------------------- out curve
            //           |        |   | |      |        |
            // in curve -+--------+---+-+------+--------+-----
            //           |        |   | |      |        |
            //           ----------   ---      ----------
            List<Curve> offsetCurves = new List<Curve>();
            for (int i = 0; i < curves.Count; i++)
            {
                Curve offsetCurve = curves[i].Offset(Plane.WorldXY, -distances[i], GlobalSettings.AbsoluteTolerance, CurveOffsetCornerStyle.Chamfer)[0];
                offsetCurves.Add(offsetCurve);
            }
            return TrimOffsetCurveRing(offsetCurves);
        }

        public static Curve ExtendCurveEnds(Curve curve, CurveEnd curveEndToExtend, double distance)
        {
            if (curveEndToExtend == CurveEnd.None) return curve;
            distance = Math.Abs(distance);
            Point3d start = curve.PointAtStart;
            Point3d end = curve.PointAtEnd;
            Point3d startExtendTargetPt = start - curve.TangentAtStart * distance;
            Point3d endExtendTargetPt = end + curve.TangentAtEnd * distance;
            Vector3d rotatedTangentAtStart = new Vector3d(curve.TangentAtStart);
            rotatedTangentAtStart.Rotate(Math.PI * 0.5, Vector3d.ZAxis);
            Vector3d rotatedTangentAtEnd = new Vector3d(curve.TangentAtEnd);
            rotatedTangentAtEnd.Rotate(Math.PI * 0.5, Vector3d.ZAxis);
            Curve startExtendTargetCurve = new Line(
                startExtendTargetPt + rotatedTangentAtStart * 0.01 * distance,
                startExtendTargetPt - rotatedTangentAtStart * 0.01 * distance
                ).ToNurbsCurve();
            Curve endExtendTargetCurve = new Line(
                endExtendTargetPt + rotatedTangentAtEnd * 0.01 * distance,
                endExtendTargetPt - rotatedTangentAtEnd * 0.01 * distance
                ).ToNurbsCurve();
            Curve curveStartExtended = curve.ExtendByLine(CurveEnd.Start, new List<Curve> { startExtendTargetCurve });
            if (curveEndToExtend != CurveEnd.End) curve = curveStartExtended == null ? curve : curveStartExtended;
            Curve curveEndExtended = curve.ExtendByLine(CurveEnd.End, new List<Curve> { endExtendTargetCurve });
            if (curveEndToExtend != CurveEnd.Start) curve = curveEndExtended == null ? curve : curveEndExtended;
            return curve;
        }

        public static bool IntersectCurveCurveExtendable(Curve curveA, Curve curveB, out Curve curveA_, out Curve curveB_)
        {
            // here we need a simplification - to be improved later!
            // incurve - multiple intersections - outcurve => incurve - last intersection - outcurve
            // acknowledged: multiple intersection should have the ideal resultant curve as sth like in-out-in-out-out
            // this will need point-curve-side checks to determin which side a point (on the subcurve of the other curve) lies.
            // to be improved in the future!
            //          --        ----- --------        ------------------- out curve
            //           |        |   | |      |        |
            // in curve -+--------+---+-+------+--------+-----
            //           |        |   | |      |        |
            //           ----------   ---      ----------
            curveA_ = curveA;
            curveB_ = curveB;
            curveA = ExtendCurveEnds(curveA, CurveEnd.Both, GlobalSettings.DefaultOffsetExtensionDistance);
            curveB = ExtendCurveEnds(curveB, CurveEnd.Both, GlobalSettings.DefaultOffsetExtensionDistance);
            CurveIntersections cis = Intersection.CurveCurve(curveA, curveB, GlobalSettings.AbsoluteTolerance, GlobalSettings.AbsoluteTolerance);
            if (cis.Count < 1) return false;
            IntersectionEvent ie = cis.ToList()[cis.Count - 1];
            double paramA = ie.ParameterA;
            double paramB = ie.ParameterB;
            curveA_ = curveA.Split(paramA)[0];
            curveB_ = curveB.Split(paramB)[1];
            return true;
        }

        public static Curve TrimCurve(Curve baseCurve, Curve prevCurve, Curve nextCurve)
        {
            Curve baseCurveExtended = ExtendCurveEnds(baseCurve, CurveEnd.Both, GlobalSettings.DefaultOffsetExtensionDistance);
            Curve prevCurveExtended = ExtendCurveEnds(prevCurve, CurveEnd.Both, GlobalSettings.DefaultOffsetExtensionDistance);
            Curve nextCurveExtended = ExtendCurveEnds(nextCurve, CurveEnd.Both, GlobalSettings.DefaultOffsetExtensionDistance);
            List<Curve> tapeCurves = new List<Curve>();
            CurveIntersections cisp = Intersection.CurveCurve(baseCurveExtended, prevCurveExtended, GlobalSettings.AbsoluteTolerance, GlobalSettings.AbsoluteTolerance);
            CurveIntersections cisn = Intersection.CurveCurve(nextCurveExtended, baseCurveExtended, GlobalSettings.AbsoluteTolerance, GlobalSettings.AbsoluteTolerance);

            bool startTaped = false;
            bool endTaped = false;
            double paramStart;
            double paramEnd;
            if (cisp.Count < 1)
            {
                startTaped = true;
                tapeCurves.Add(new Line(prevCurve.PointAtEnd, baseCurve.PointAtStart).ToNurbsCurve());
            }
            if (cisn.Count < 1)
            {
                endTaped = true;
                tapeCurves.Add(new Line(baseCurve.PointAtEnd, nextCurve.PointAtStart).ToNurbsCurve());
            }
            if (startTaped && endTaped)
            {
                return Curve.JoinCurves(new List<Curve> { tapeCurves[0], baseCurve, tapeCurves[1] })[0];
            }


            if (startTaped)
            {
                if (cisn.ToList()[0].IsOverlap)
                {
                    paramEnd = baseCurve.Domain.Max;
                } else
                {
                    paramEnd = cisn.ToList()[0].ParameterB;
                }
                var splitted = baseCurveExtended.Split(new List<double> { baseCurve.Domain.Min, paramEnd });
                return Curve.JoinCurves(new List<Curve> { splitted[1], tapeCurves[0] })[0];
            }


            if (endTaped)
            {
                if (cisp.ToList()[cisp.Count - 1].IsOverlap)
                {
                    paramStart = baseCurve.Domain.Min;
                }
                else
                {
                    paramStart = cisp.ToList()[cisp.Count - 1].ParameterA;
                }
                var splitted = baseCurveExtended.Split(new List<double> { paramStart, baseCurve.Domain.Max });
                return Curve.JoinCurves(new List<Curve> { splitted[1], tapeCurves[0] })[0];
            }

            paramStart = cisp.ToList()[cisp.Count - 1].IsOverlap ? baseCurve.Domain.Min
                : cisp.ToList()[cisp.Count - 1].ParameterA;
            paramEnd = cisn.ToList()[0].IsOverlap ? baseCurve.Domain.Max
                : cisn.ToList()[0].ParameterB;

            return baseCurveExtended.Split(new List<double> { paramStart, paramEnd })[1];
        }

        public static List<Curve> TrimOffsetCurveRing(List<Curve> curveRing)
        {
            curveRing = curveRing.ToList();
            for (int i = 0; i < curveRing.Count; i++)
            {
                Curve current = curveRing[i];
                Curve prev = curveRing[i == 0 ? curveRing.Count - 1 : i - 1];
                Curve next = curveRing[i == curveRing.Count - 1 ? 0 : i + 1];
                curveRing[i] = TrimCurve(current, prev, next);
            }
            return curveRing;
        }

        public static List<Curve> OffsetGraphFaceCurve(NetworkFace face, string distanceFieldName)
        {
            List<Curve> curveRing = new List<Curve>();
            List<double> distances = new List<double>();
            for (int i = 0; i < face.EdgesTraversed.Count; i++)
            {
                NetworkEdge e = face.EdgesTraversed[i];
                if (face.EdgesTraverseDirection[i])
                {
                    curveRing.Add(e.UnderlyingCurve.DuplicateCurve());
                } else
                {
                    Curve crv = e.UnderlyingCurve.DuplicateCurve();
                    crv.Reverse();
                    curveRing.Add(crv);                
                }
                if (e.AttributesInstance.TryGet<string>(distanceFieldName, out string val))
                {
                    distances.Add(Convert.ToDouble(val));
                } else
                {
                    distances.Add(GlobalSettings.DefaultOffsetDitance);
                    e.AttributesInstance.Set(distanceFieldName, GlobalSettings.DefaultOffsetDitance.ToString());
                }
            }
            return OffsetVariable(curveRing, distances);
        }

        public static List<Curve> OffsetGraphFaces(NetworkGraph graph, string distanceFieldName)
        {
            List<Curve> result = new List<Curve>();
            foreach (NetworkFace f in graph.NetworkFaces)
            {
                List<Curve> offsetCurves = OffsetGraphFaceCurve(f, distanceFieldName);
                result.Add(Curve.JoinCurves(offsetCurves)[0]);
            }
            return result;
        }

        public static List<Curve> OffsetGraphFaces(NetworkGraph graph)
        {
            return OffsetGraphFaces(graph, "OffsetDistance");
        }

    }
}
