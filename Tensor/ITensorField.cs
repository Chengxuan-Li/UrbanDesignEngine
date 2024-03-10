using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Tensor
{
    public interface ITensorField : IDuplicable<ITensorField>
    {
        TensorFieldType TensorFieldType { get; }

        BoundingBox Boundary { get; }

        Predicate<int> ActivationHierarchy { get; }

        bool Contains(Point3d point);

        bool Evaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar);

        bool ContextAwareEvaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar);
    }
}
