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
    public class ReferencePointGeometry : HasReferenceGeometry<Point>, IAttributable, IHasGHIOPreviewGeometricParam<ReferencePointGeometry, GHIOPointParam<ReferencePointGeometry>, Point>
    {
        public Attributes Attributes;

        public GHIOPointParam<ReferencePointGeometry> gHIOParam => new GHIOPointParam<ReferencePointGeometry> { ScriptClassVariable = this };

        public Attributes AttributesInstance => Attributes;

        public ReferencePointGeometry(Guid guid, RhinoDoc rhinoDoc, Attributes attributes)
        {
            Guid = guid;
            doc = rhinoDoc;
            attributes.Guid = guid;
        }
    }
}
