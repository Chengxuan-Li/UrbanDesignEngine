using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.IO;



namespace UrbanDesignEngine.Tensor
{
    public enum TensorFieldType
    {
        PointAttractor = 1,
        Homogeneous = 0,
        LineAttractor = 2,
        PolylineAttractor = 3,
        CurveAttractor = 4,

    }

    public struct MatrixEntry<T> : IEquatable<MatrixEntry<T>>, IFormattable
    {
        public Func<T, double> Func;

        public MatrixEntry(Func<T, double> func)
        {
            Func = func;
        }

        public static MatrixEntry<T> operator +(MatrixEntry<T> a, MatrixEntry<T> b)
        {
            return new MatrixEntry<T>((t) => a.Func.Invoke(t) + b.Func.Invoke(t));
        }

        public static MatrixEntry<T> operator -(MatrixEntry<T> a, MatrixEntry<T> b)
        {
            return new MatrixEntry<T>((t) => a.Func.Invoke(t) - b.Func.Invoke(t));
        }

        public static MatrixEntry<T> operator *(MatrixEntry<T> a, MatrixEntry<T> b)
        {
            return new MatrixEntry<T>((t) => a.Func.Invoke(t) * b.Func.Invoke(t));
        }

        public static MatrixEntry<T> operator /(MatrixEntry<T> a, MatrixEntry<T> b)
        {
            return new MatrixEntry<T>((t) => a.Func.Invoke(t) / b.Func.Invoke(t));
        }
        public bool Equals(MatrixEntry<T> other)
        {
            return Func.Equals(other.Func);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return "Matrix Entry with nested function: {" + Func.ToString() + "}";
        }
    }

    //Add GHIOParam
    public class SimpleTensorField
        // : IDuplicable<SimpleTensorField
    {
        //public Matrix<double> Matrix = Matrix<double>.Build.Dense(2, 2, 0);

        /*
        public Matrix<MatrixEntry<Point3d>> Matrix = Matrix<MatrixEntry<Point3d>>.Build.Dense(2, 2, new MatrixEntry<Point3d>(_ => 0));

        public SimpleTensorField(MatrixEntry<Point3d> v00, MatrixEntry<Point3d> v01, MatrixEntry<Point3d> v10, MatrixEntry<Point3d> v11)
        {
            Matrix[0, 0] = v00;
            Matrix[0, 1] = v01;
            Matrix[1, 0] = v10;
            Matrix[1, 1] = v11;
        }
        */
        public TensorFieldType TensorFieldType = TensorFieldType.Homogeneous;

        public double Factor = 1.0;

        Vector3d vector;

        public virtual BoundingBox Boundary => new BoundingBox(new Point3d(-9999, -9999, 0), new Point3d(9999, 9999, 0));

        public Predicate<int> ActivationHierarchy = h => h >= -1;

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
                    ContextAwareEvaluate(-1, pt, out Vector3d av, out Vector3d iv, out double sc);
                    crvs.Add(new Line(pt, pt + av * (0.1 + sc)).ToNurbsCurve());
                    crvs.Add(new Line(pt, pt + iv * (0.1 + sc)).ToNurbsCurve());
                }
                return crvs;
            }
        }

        public SimpleTensorField()
        {
            vector = new Vector3d(1, 1, 0);
            vector.Unitize();
        }

        public SimpleTensorField(Vector3d vec)
        {
            vector = new Vector3d(vec.X, vec.Y, 0);
            vector.Unitize();
        }

        public virtual double Distance(Point3d point)
        {
            return 1.0;
        }

        public virtual double Decay(Point3d point)
        {
            return Factor * 1.0;
        }

        public virtual bool ContextAwareEvaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
        {
            double d = TensorFieldSettings.EvaluationNeighbourDistance;
            double w = TensorFieldSettings.EvaluationNeighbourWeight;
            Evaluate(hierarchy, point, out Vector3d currentMajorVector, out Vector3d currentMinorVector, out double currentScalar);
            Point3d lu = currentMajorVector * d + currentMinorVector * d + point;
            Point3d lb = -currentMajorVector * d + currentMinorVector * d + point;
            Point3d ru = currentMajorVector * d - currentMinorVector * d + point;
            Point3d rb = -currentMajorVector * d - currentMinorVector * d + point;
            Evaluate(hierarchy, lu, out Vector3d luMajorVector, out Vector3d luMinorVector, out double luScalar);
            Evaluate(hierarchy, lb, out Vector3d lbMajorVector, out Vector3d lbMinorVector, out double lbScalar);
            Evaluate(hierarchy, ru, out Vector3d ruMajorVector, out Vector3d ruMinorVector, out double ruScalar);
            Evaluate(hierarchy, rb, out Vector3d rbMajorVector, out Vector3d rbMinorVector, out double rbScalar);
            majorVector = (currentMajorVector + w * luMajorVector + w * lbMajorVector + w * ruMajorVector + w * rbMajorVector) / (4.0 * w + 1.0);
            minorVector = (currentMinorVector + w * luMinorVector + w * lbMinorVector + w * ruMinorVector + w * rbMinorVector) / (4.0 * w + 1.0);
            scalar = (currentScalar + w * luScalar + w * lbScalar + w * ruScalar + w * rbScalar) / (4.0 * w + 1.0);
            return ActivationHierarchy.Invoke(hierarchy);
        }

        public virtual bool Evaluate(int hierarchy, Point3d point, out Vector3d majorVector, out Vector3d minorVector, out double scalar)
        {
            scalar = 1 / Distance(point) * Decay(point);
            majorVector = new Vector3d(vector);
            minorVector = new Vector3d(vector);
            minorVector.Rotate(Math.PI / 2.0, Vector3d.ZAxis);
            return ActivationHierarchy.Invoke(hierarchy);
        }

        public virtual bool Contains(Point3d point)
        {
            return Boundary.Contains(point);
        }


        /*
        public virtual SimpleTensorField Duplicate()
        {
            return new SimpleTensorField() { vector = vector, Boundary = Boundary, ActivationHierarchy = ActivationHierarchy, TensorFieldType = TensorFieldType };
        }
        */
    }
}
