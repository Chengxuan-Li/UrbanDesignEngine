using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Tensor
{
    public enum MultipleTensorFieldsEvaluationMethod
    {
        MaxScalar = 0,
        Average = 1,
        WeightedAverage = 2,
    }

    public class MultipleTensorFields : IDuplicable<MultipleTensorFields>, ITensorField, IHasGHIOPreviewGeometryListParam<MultipleTensorFields, GHIOTensorFieldCurvesParam<MultipleTensorFields>, Curve>, IHasGeometryList<Curve>
    {
        public List<ITensorField> TensorFields = new List<ITensorField>(); 

        public TensorFieldType TensorFieldType => TensorFieldType.Multiple;

        public MultipleTensorFieldsEvaluationMethod Method = MultipleTensorFieldsEvaluationMethod.MaxScalar;

        public List<Curve> PreviewGeometryList
        {
            get
            {
                double w = Boundary.Max.X - Boundary.Min.X;
                double h = Boundary.Max.Y - Boundary.Min.Y;
                int numW = (int)Math.Floor(w / TensorFieldSettings.PreviewGeometryInterval);
                int numH = (int)Math.Floor(h / TensorFieldSettings.PreviewGeometryInterval);
                List<Point3d> pts = new List<Point3d>();
                List<Curve> crvs = new List<Curve>();
                for (int i = 0; i < numW; i++)
                {
                    for (int j = 0; j < numH; j++)
                    {
                        Point3d pt = new Point3d(
                            Boundary.Min.X + i * TensorFieldSettings.PreviewGeometryInterval,
                            Boundary.Min.Y + j * TensorFieldSettings.PreviewGeometryInterval,
                            0);
                        pts.Add(pt);
                    }
                }
                foreach (Point3d pt in pts)
                {
                    Evaluate(-1, pt, Method, true, out Vector3d av, out Vector3d iv, out double sc);
                    crvs.Add(new Line(pt - av * (0.1 + sc) / 2, pt + av * (0.1 + sc) / 2).ToNurbsCurve());
                    crvs.Add(new Line(pt - iv * (0.1 + sc) / 2, pt + iv * (0.1 + sc) / 2).ToNurbsCurve());
                }
                return crvs;
            }
        }

        public BoundingBox Boundary
        {
            get
            {
                if (TensorFields.Count == 0)
                {
                    return default;
                } else
                {
                    BoundingBox bbox = TensorFields[0].Boundary;
                    if (TensorFields.Count == 1)
                    {
                        return bbox;
                    } else
                    {
                        for (int i = 1; i < TensorFields.Count; i++)
                        {
                            bbox.Union(TensorFields[i].Boundary);
                        }
                        return bbox;
                    }
                }

            }
        }

        public GHIOTensorFieldCurvesParam<MultipleTensorFields> gHIOParam => new GHIOTensorFieldCurvesParam<MultipleTensorFields>() { ScriptClassVariable = this};

        public Predicate<int> ActivationHierarchy => throw new NotImplementedException();

        public MultipleTensorFields()
        {

        }

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

        public bool Evaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
        {
            return Evaluate(hierarchy, point, MultipleTensorFieldsEvaluationMethod.MaxScalar, false, out majorVector, out minorVector, out scalar);
        }

        public bool ContextAwareEvaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
        {
            return Evaluate(hierarchy, point, MultipleTensorFieldsEvaluationMethod.MaxScalar, true, out majorVector, out minorVector, out scalar);
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

        public bool Contains(Point3d point)
        {
            foreach (var tf in TensorFields)
            {
                if (tf.Contains(point))
                {
                    return true;
                }
            }
            return false;
        }


        public MultipleTensorFields Duplicate()
        {
            return new MultipleTensorFields() { Method = Method, TensorFields = TensorFields.ConvertAll(tf => tf.Duplicate()) };
        }

        ITensorField IDuplicable<ITensorField>.Duplicate()
        {
            return Duplicate();
        }
    }
}
