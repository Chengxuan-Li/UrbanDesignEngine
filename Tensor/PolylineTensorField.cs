using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Tensor
{
    public class PolylineTensorField : ObjectTensorField<Curve>
    {
        public Polyline Polyline;

        public PolylineTensorField(Polyline polyline, double range) : base(polyline.ToPolylineCurve(), range)
        {
            Polyline = polyline;
            geometry = polyline.ToNurbsCurve();
            TensorFieldType = TensorFieldType.PolylineAttractor;
            Range = range;
        }

        public override double Distance(Point3d point)
        {
            var plc = Polyline.ToPolylineCurve();
            plc.ClosestPoint(point, out double t);
            return plc.PointAt(t).DistanceTo(point);
        }

        public override bool Evaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
        {
            var plc = Polyline.ToPolylineCurve();
            plc.ClosestPoint(point, out double t);
            Point3d closestPoint = plc.PointAt(t);
            majorVector = new Vector3d(-closestPoint + point);
            majorVector.Unitize();
            minorVector = new Vector3d(majorVector);
            minorVector.Rotate(Math.PI / 2.0, Vector3d.ZAxis);
            scalar = Distance(point) * Decay(point);
            return ActivationHierarchy.Invoke(hierarchy);
        }
    }
}
