using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Tensor
{
    public enum MultipleTensorFieldsEvaluationMethod
    {
        MaxScalar = 0,
        Average = 1,
        WeightedAverage = 2,
    }

    public class MultipleTensorFields
    {
        public List<SimpleTensorField> TensorFields = new List<SimpleTensorField>();

        public MultipleTensorFields(SimpleTensorField field)
        {
            TensorFields.Add(field);
        }

        public void Multiply(SimpleTensorField field)
        {
            TensorFields.Add(field);
        }

        public void Multiply(MultipleTensorFields fields)
        {
            TensorFields.AddRange(fields.TensorFields);
        }

        public bool Evaluate(int hierarchy, Point3d point, MultipleTensorFieldsEvaluationMethod method, bool contextAwareEvaluation, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
        {
            majorVector = default;
            minorVector = default;
            scalar = default;

            List<Vector3d> majorVectors = new List<Vector3d>();
            List<Vector3d> minorVectors = new List<Vector3d>();
            List<double> scalars = new List<double>();

            foreach (var tf in TensorFields)
            {
                if (tf.ActivationHierarchy(hierarchy) && tf.Contains(point))
                {
                    Vector3d maV;
                    Vector3d miV;
                    double sc;
                    if (contextAwareEvaluation)
                    {
                        tf.Evaluate(hierarchy, point, out maV, out miV, out sc);
                    } else
                    {
                        tf.Evaluate(hierarchy, point, out maV, out miV, out sc);
                    }
                    majorVectors.Add(maV);
                    minorVectors.Add(miV);
                    scalars.Add(sc);
                }
            }

            if (majorVectors.Count == 0) return false;

            if (method == MultipleTensorFieldsEvaluationMethod.MaxScalar)
            {
                int i = scalars.IndexOf(scalars.Max());
                majorVector = majorVectors[i];
                minorVector = minorVectors[i];
                scalar = scalars[i];
            }

            if (method == MultipleTensorFieldsEvaluationMethod.WeightedAverage)
            {
                Vector3d sumMajorVector = new Vector3d(0, 0, 0);
                Vector3d sumMinorVector = new Vector3d(0, 0, 0);
                double sumScalar = 0;
                for (int i = 0; i < majorVectors.Count; i++)
                {
                    sumMajorVector += majorVectors[i] * scalars[i];
                    sumMinorVector += minorVectors[i] * scalars[i];
                    sumScalar += scalars[i];
                }
                sumMajorVector = sumMajorVector / sumScalar;
                sumMinorVector = sumMinorVector / sumScalar;
                majorVector = sumMajorVector;
                minorVector = sumMinorVector;
            }

            if (method == MultipleTensorFieldsEvaluationMethod.Average)
            {
                Vector3d sumMajorVector = new Vector3d(0, 0, 0);
                Vector3d sumMinorVector = new Vector3d(0, 0, 0);
                double sumScalar = 0;
                for (int i = 0; i < majorVectors.Count; i++)
                {
                    sumMajorVector += majorVectors[i];
                    sumMinorVector += minorVectors[i];
                    sumScalar += 1;
                }
                sumMajorVector = sumMajorVector / sumScalar;
                sumMinorVector = sumMinorVector / sumScalar;
                majorVector = sumMajorVector;
                minorVector = sumMinorVector;
            }

            return true;
        }
    }
}
