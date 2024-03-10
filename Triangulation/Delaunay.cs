using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriangleNet.Meshing;
using TriangleNet.Geometry;

namespace UrbanDesignEngine.Triangulation
{
    public class Delaunay
    {
        public static IMesh ConsDT(Polygon polygon)
        {
            var options = new ConstraintOptions() { ConformingDelaunay = true };
            var quality = new QualityOptions();

            // Triangulate the polygon
            var mesh = polygon.Triangulate(options, quality);
            return mesh;
        }


    }
}
