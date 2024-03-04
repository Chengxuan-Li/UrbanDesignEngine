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

namespace UrbanDesignEngine.DataStructure
{
    public enum HasGeometryType
    {
        NoGeometryData = 0,
        DocumentReferenceGeometry = 1,
        ScriptRuntimeGeometry = 2,
    }
    public interface IHasGeometry<T> where T : GeometryBase
    {
        T PreviewGeometry { get; }

    }

    public interface IHasGeometryList<T> where T: GeometryBase
    {
        List<T> PreviewGeometryList { get; }
        BoundingBox Boundary { get; }
    }
}
