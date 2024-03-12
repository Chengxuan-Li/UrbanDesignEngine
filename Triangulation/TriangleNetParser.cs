using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using TriangleNet.Meshing;

using TriangleNet.Geometry;
using DotRecast.Core.Numerics;
using DotRecast.Detour;

namespace UrbanDesignEngine.Triangulation
{
    public static class TriangleNetParser
    {
        public static Contour ToTNContour(Polyline polyline, int marker)
        {
            Vertex[] vertices = new Vertex[polyline.Count - 1];
            for (int i = 0; i < polyline.Count - 1; i++)
            {
                vertices[i] = new Vertex(polyline[i].X, polyline[i].Y);
            }
            return new Contour(vertices, marker);
        }

        public static Polygon ToTNPolygon(Polyline polyline)
        {
            Contour contour = ToTNContour(polyline, 0);
            Polygon polygon = new Polygon();
            polygon.Add(contour);
            return polygon;
        }

        public static Polygon ToTNPolygon(Polyline polylineBoundary, List<Polyline> polylineHoles)
        {
            int marker = 0;
            Contour contourBoundary = ToTNContour(polylineBoundary, marker);
            Polygon polygon = new Polygon();
            polygon.Add(contourBoundary, false);
            marker++;

            Contour[] contourHoles = new Contour[polylineHoles.Count];
            for (int i = 0; i < polylineHoles.Count; i++)
            {
                Contour c = ToTNContour(polylineHoles[i], marker);
                contourHoles[i] = c;
                marker++;
            }

            contourHoles.ToList().ForEach(c => polygon.Add(c, true));

            return polygon;
        }

        public static Rhino.Geometry.Mesh ToRhinoMesh(IMesh mesh)
        {
            Rhino.Geometry.Mesh resultantMesh = new Rhino.Geometry.Mesh();
            mesh.Vertices.ToList().ForEach(v => resultantMesh.Vertices.Add(new Point3d(v.x, v.y, 0)));
            mesh.Triangles.ToList().ForEach(t => resultantMesh.Faces.AddFace(t.vertices[0].id, t.vertices[1].id, t.vertices[2].id));

            return resultantMesh;            
        }

        public static void ToDotRecastVerticesFaces(IMesh mesh, out float[] vertices, out int[] faces)
        {
            vertices = new float[mesh.Vertices.Count * 3];
            faces = new int[mesh.Triangles.Count * 3];
            int vi = 0;
            foreach (var v in mesh.Vertices)
            {
                vertices[vi] = (float)v.x;
                vertices[vi + 1] = (float)v.y;
                vertices[vi + 2] = 0.0f;
                vi += 3;
            }
            int ti = 0;
            foreach (var t in mesh.Triangles)
            {
                faces[ti + 0] = t.vertices[0].id ;
                //faces[ti + 1] = t.vertices[0].id * 3 + 1;
                //faces[ti + 2] = t.vertices[0].id * 3 + 2;
                faces[ti + 1] = t.vertices[1].id ;
                //faces[ti + 4] = t.vertices[1].id * 3 + 1;
                //faces[ti + 5] = t.vertices[1].id * 3 + 2;
                faces[ti + 2] = t.vertices[2].id ;
                //faces[ti + 7] = t.vertices[2].id * 3 + 1;
                //faces[ti + 8] = t.vertices[2].id * 3 + 2;
                ti += 3;
            }
            return;
        }

        public static RcVec3f ToRcVec(Vector3d vec)
        {
            return new RcVec3f((float)vec.X, (float)vec.Y, (float)vec.Z);
        }

        public static Vector3d ToRHVec(RcVec3f vec)
        {
            return new Vector3d(vec.X, vec.Y, vec.Z);
        }

        public static List<RcVec3f> ToRcVecList(List<Vector3d> vecs)
        {
            return vecs.ConvertAll(v => ToRcVec(v));
        }
        
        public static Polyline StraightPathToPolyline(List<DtStraightPath> ps)
        {
            List<Point3d> pts = new List<Point3d>();
            foreach (var p in ps)
            {
                pts.Add(new Point3d(ToRHVec(p.pos)));
            }
            return new Polyline(pts);
        }
    }
}
