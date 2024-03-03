using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using UrbanDesignEngine.Maths;

namespace UrbanDesignEngine.Tensor
{
    public class RadialTensorField : ObjectTensorField<Point>
    {
        public RadialTensorField(Point3d point, double range) : base(new Point(point), range)
        {
            geometry = new Point(point);
            TensorFieldType = TensorFieldType.PointAttractor;
            Range = range;
        }

        public override double Distance(Point3d point)
        {
            return geometry.Location.DistanceTo(point);
        }

        public override bool Evaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
        {
            majorVector = new Vector3d(-geometry.Location + point);
            majorVector.Unitize();
            minorVector = new Vector3d(majorVector);
            minorVector.Rotate(Math.PI / 2.0, Vector3d.ZAxis);
            scalar = Distance(point) * Decay(point);
            return ActivationHierarchy.Invoke(hierarchy);
        }
    }
}
