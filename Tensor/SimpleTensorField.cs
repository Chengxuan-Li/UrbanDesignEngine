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


    public class SimpleTensorField
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

        Vector3d vector;

        public BoundingBox Boundary = new BoundingBox(new Point3d(-9999, -9999, 0), new Point3d(9999, 9999, 0));

        public Predicate<int> ActivationHierarchy => h => h >= 0; 

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
            return 1.0;
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

      
    }
}
