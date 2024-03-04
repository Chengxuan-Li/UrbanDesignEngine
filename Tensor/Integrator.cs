using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Tensor
{

    public abstract class FieldIntegrator
    {
        public ITensorField TensorField;

        public FieldIntegrator(ITensorField field)
        {
            TensorField = field;
        }

        public abstract Vector3d Integrate(Point3d point, bool major);

        protected Vector3d SampleFieldVector(Point3d point, bool major)
        {
            TensorField.ContextAwareEvaluate(-1, point, out Vector3d majorVector, out Vector3d minorVector, out double scalar);
            return major ? majorVector : minorVector;
        }

        public bool OnLand(Point3d point)
        {
            return TensorField.Contains(point);
        }
    }

    public class EulerIntegrator : FieldIntegrator
    {
        public StreamlineParams SParams;

        public EulerIntegrator(ITensorField field, StreamlineParams parameters) : base(field)
        {
            SParams = parameters;
        }

        public override Vector3d Integrate(Point3d point, bool major)
        {
            return SampleFieldVector(point, major) * SParams.Dstep;
        }
    }

    public class RK4Integrator : FieldIntegrator
    {
        private StreamlineParams SParams;

        public RK4Integrator(ITensorField field, StreamlineParams parameters) : base(field)
        {
            SParams = parameters;
        }

        public override Vector3d Integrate(Point3d point, bool major)
        {
            Vector3d k1 = SampleFieldVector(point, major);
            Vector3d k23 = SampleFieldVector(point + new Vector3d(SParams.Dstep / 2, SParams.Dstep / 2, 0), major);
            Vector3d k4 = SampleFieldVector(point + new Vector3d(SParams.Dstep / 2, SParams.Dstep / 2, 0), major);

            return (k1 + k23 * 4 + k4) * SParams.Dstep / 6;
        }
    }


}
