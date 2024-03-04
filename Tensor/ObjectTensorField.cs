using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;
using UrbanDesignEngine.Maths;

namespace UrbanDesignEngine.Tensor
{
    public abstract class ObjectTensorField<T> : SimpleTensorField where T: GeometryBase
    {
        protected T geometry;
        public T Geometry => geometry;
        public double DecayRange;
        public double Extent;
        public void SetGeometry(T geo)
        {
            geometry = geo;
        }

        public override BoundingBox Boundary
        {
            get
            {
                var bbox = Geometry.GetBoundingBox(false);
                bbox.Inflate(Extent);
                bbox.Min = new Point3d(bbox.Min.X, bbox.Min.Y, 0.0);
                bbox.Max = new Point3d(bbox.Max.X, bbox.Max.Y, 0.0);
                return bbox;
            }
        }

        public ObjectTensorField(T geo, double range, double extent)
        {
            geometry = geo;
            DecayRange = range;
            Extent = extent;
        }

        public override double Decay(Point3d point)
        {
            return Factor * DecayFuncs.Gaussian(DecayRange).Invoke(Distance(point));
        }

        public override bool Contains(Point3d point)
        {
            return Distance(point) <= Extent;
        }

        public abstract override double Distance(Point3d point);

        public abstract override bool Evaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar);

        // needs override:
        // double Distance(Point3d point)

        // needs override:
        // bool Evaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
    }
}
