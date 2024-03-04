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
    public abstract class GHIOPreviewGeometricParam<ScriptClass, G> : GHIOParam<ScriptClass>, IGH_BakeAwareData, IGH_PreviewData  where ScriptClass : IHasGeometry<G> where G : GeometryBase
    {
        public GH_Component Component;
        public virtual G PreviewGeometry => ScriptClassVariable.PreviewGeometry; 
        public virtual BoundingBox PreviewBoundingBox => PreviewGeometry.GetBoundingBox(true); // overriden when single variable contains multiple geometries
        public abstract Action<GH_PreviewWireArgs, G> DrawWires { get; } // override whenever something needs to be drawn
        public abstract Action<GH_PreviewMeshArgs, G> DrawMeshes { get; } // override whenever something needs to be drawn
        public virtual Func<GH_Component, bool> ComponentHidden => c => c.Hidden; // overriden for custom hidden/visible behaviours        

        #region IBakeWareData
        public virtual bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid) // override for custom bake behaviour or error handling
        {
            obj_guid = doc.Objects.Add(PreviewGeometry, att);
            return true;
        }
        #endregion
        #region IPreviewObject
        public bool Hidden { get => false/*ComponentHidden.Invoke(Component)*/; set => Component.Hidden = value; }

        public virtual bool IsPreviewCapable => PreviewGeometry != null && PreviewGeometry.IsValid; // override when custom validity check is required

        public BoundingBox ClippingBox => PreviewBoundingBox;
        public virtual void DrawViewportWires(GH_PreviewWireArgs args) // only overriden if overriding DrawWires action does not suffice
        {
            DrawWires.Invoke(args, PreviewGeometry);
        }

        public virtual void DrawViewportMeshes(GH_PreviewMeshArgs args) // only overriden if overriding DrawMeshes action does not suffice
        {
            DrawMeshes.Invoke(args, PreviewGeometry);
        }
        #endregion

    }

    public abstract class GHIOPreviewGeometryListParam<ScriptClass, G> : GHIOParam<ScriptClass>, IGH_PreviewData where ScriptClass : IHasGeometryList<G> where G : GeometryBase
    {
        public GH_Component Component;
        public virtual List<G> PreviewGeometryList => ScriptClassVariable.PreviewGeometryList;
        public virtual BoundingBox PreviewBoundingBox => ScriptClassVariable.Boundary;
            
            // overriden when single variable contains multiple geometries
        public abstract Action<GH_PreviewWireArgs, G> DrawWires { get; } // override whenever something needs to be drawn
        public abstract Action<GH_PreviewMeshArgs, G> DrawMeshes { get; } // override whenever something needs to be drawn
        public virtual Func<GH_Component, bool> ComponentHidden => c => c.Hidden; // overriden for custom hidden/visible behaviours        

        #region IPreviewObject
        public bool Hidden { get => false/*ComponentHidden.Invoke(Component)*/; set => Component.Hidden = value; }

        public virtual bool IsPreviewCapable => true; // override when custom validity check is required

        public BoundingBox ClippingBox => PreviewBoundingBox;
        public virtual void DrawViewportWires(GH_PreviewWireArgs args) // only overriden if overriding DrawWires action does not suffice
        {
            //List<G> pgl = PreviewGeometryList;
            //pgl.ForEach(g => DrawWires.Invoke(args, g));
        }

        public virtual void DrawViewportMeshes(GH_PreviewMeshArgs args) // only overriden if overriding DrawMeshes action does not suffice
        {
            List<G> pgl = PreviewGeometryList;
            pgl.ForEach(g => DrawMeshes.Invoke(args, g));
        }
        #endregion

    }
}
