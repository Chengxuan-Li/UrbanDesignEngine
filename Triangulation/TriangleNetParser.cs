using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using TriangleNet.Meshing;

using TriangleNet.Geometry;

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
    }
}
