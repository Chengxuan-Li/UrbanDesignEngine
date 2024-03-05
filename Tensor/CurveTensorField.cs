using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Tensor
{
    public class CurveTensorField : ObjectTensorField<Curve>
    {
        Curve Curve;
        public override TensorFieldType TensorFieldType => TensorFieldType.CurveAttractor;
        public CurveTensorField(Curve curve, double range, double extent) : base(curve, range, extent)
        {
            Curve = curve;
            geometry = Curve;
            
            DecayRange = range;
            Extent = extent;
        }

        public override double Distance(Point3d point)
        {
            Curve.ClosestPoint(point, out double t);
            return Curve.PointAt(t).DistanceTo(point);
        }

        public override bool Evaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
        {
            Curve.ClosestPoint(point, out double t);
            Point3d closestPoint = Curve.PointAt(t);
            majorVector = new Vector3d(-closestPoint + point);
            majorVector.Unitize();
            minorVector = new Vector3d(majorVector);
            minorVector.Rotate(Math.PI / 2.0, Vector3d.ZAxis);
            scalar = Distance(point) * Decay(point);
            return ActivationHierarchy.Invoke(hierarchy);
        }
    }
}
