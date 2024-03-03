using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;

namespace UrbanDesignEngine.Tensor
{
    public class ObjectTensorField : SimpleTensorField
    {
        public override double Decay(Point3d point)
        {
            return base.Decay(point);
        }

        public override double Distance(Point3d point)
        {
            return base.Distance(point);
        }

        public override bool Evaluate(Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
        {
            return base.Evaluate(point, out majorVector, out minorVector, out scalar);
        }

        public override bool Contains(Point3d point)
        {
            return base.Contains(point);
        }
    }
}
