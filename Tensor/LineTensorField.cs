using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Tensor
{
    public class LineTensorField : ObjectTensorField<Curve>
    {
        public Line Line;
        public LineTensorField(Line line, double range, double extent) : base(line.ToNurbsCurve(), range, extent)
        {
            Line = line;
            geometry = line.ToNurbsCurve();
            TensorFieldType = TensorFieldType.LineAttractor;
            DecayRange = range;
            Extent = extent;
        }

        public override double Distance(Point3d point)
        {
            return Line.DistanceTo(point, true);
        }

        public override bool Evaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
        {
            Point3d closestPoint = Line.ClosestPoint(point, true);
            majorVector = new Vector3d(-closestPoint + point);
            majorVector.Unitize();
            minorVector = new Vector3d(majorVector);
            minorVector.Rotate(Math.PI / 2.0, Vector3d.ZAxis);
            scalar = Distance(point) * Decay(point);
            return ActivationHierarchy.Invoke(hierarchy);
        }
    }
}
