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
    public interface IHasGHIOPreviewGeometricParam<ScriptClass, T, G> where T : GHIOPreviewGeometricParam<ScriptClass, G> where ScriptClass : IHasGeometry<G> where G : GeometryBase
    {
        T gHIOParam { get; }
    }

    public interface IHasGHIOPreviewGeometryListParam<ScriptClass, T, G> where T : GHIOPreviewGeometryListParam<ScriptClass, G> where ScriptClass : IHasGeometryList<G> where G : GeometryBase
    {
        T gHIOParam { get; }
    }
}
