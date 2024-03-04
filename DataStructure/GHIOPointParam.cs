using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Display;
using GH_IO.Serialization;
using Rhino.Render;

namespace UrbanDesignEngine.DataStructure
{
    public class GHIOPointParam<ScriptVariable> : GHIOPreviewGeometricParam<ScriptVariable, Point> where ScriptVariable : IHasGeometry<Point>
    {
        public override Action<GH_PreviewWireArgs, Point> DrawWires => (a, g) => a.Pipeline.DrawPoint(g.Location, PointStyle.RoundActivePoint, PreviewSettings.PointRadius, PreviewSettings.GraphPreviewColor);

        public override Action<GH_PreviewMeshArgs, Point> DrawMeshes => (a, g) => a.Pipeline.DrawPoint(g.Location, PointStyle.RoundActivePoint, PreviewSettings.PointRadius, PreviewSettings.GraphPreviewColor);

    }
}
