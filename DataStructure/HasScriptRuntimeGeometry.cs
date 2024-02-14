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
    public abstract class HasScriptRuntimeGeometry<T> : IHasGeometry<T> where T : GeometryBase
    {
        public abstract T Geometry { get; }
        public T PreviewGeometry => Geometry;
    }
}
