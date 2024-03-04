using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using UrbanDesignEngine.Maths;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Tensor
{
    public class RadialTensorField : ObjectTensorField<Point>, IHasGHIOPreviewGeometryListParam<RadialTensorField, GHIOTensorFieldCurvesParam<RadialTensorField>, Curve>, IHasGeometryList<Curve>
    {
        public RadialTensorField(Point3d point, double range, double extent) : base(new Point(point), range, extent)
        {
            geometry = new Point(point);
            TensorFieldType = TensorFieldType.PointAttractor;
            DecayRange = range;
            Extent = extent;
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

        public new GHIOTensorFieldCurvesParam<RadialTensorField> gHIOParam => new GHIOTensorFieldCurvesParam<RadialTensorField>() { ScriptClassVariable = this };
    }
}
