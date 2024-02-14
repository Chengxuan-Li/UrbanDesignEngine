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
    public abstract class HasReferenceGeometry<T> : IHasGeometry<T> where T : GeometryBase
    {
        public Guid Guid;
        public RhinoDoc doc;
        public T PreviewGeometry => (T)doc.Objects.FindId(Guid).Geometry;
    }
}
