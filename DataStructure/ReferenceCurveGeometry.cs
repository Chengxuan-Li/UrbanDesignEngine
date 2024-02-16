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
    public class ReferenceCurveGeometry : HasReferenceGeometry<Curve>, IAttributable, IHasGHIOPreviewGeometricParam<ReferenceCurveGeometry, GHIOCurveParam<ReferenceCurveGeometry>, Curve>
    {
        public Attributes Attributes;

        public GHIOCurveParam<ReferenceCurveGeometry> gHIOParam => new GHIOCurveParam<ReferenceCurveGeometry> { ScriptClassVariable = this };

        public Attributes AttributesInstance => Attributes;

        public ReferenceCurveGeometry(Guid guid, RhinoDoc rhinoDoc, Attributes attributes) : base()
        {
            Guid = guid;
            doc = rhinoDoc;
            Attributes = new Attributes();
            Attributes.Guid = guid;
            Attributes.Concat(attributes);
        }
    }


}
