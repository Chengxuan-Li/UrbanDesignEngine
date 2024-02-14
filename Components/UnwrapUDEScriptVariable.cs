using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Components
{
    public class UnwrapUDEScriptVariable : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the UnwrapUDEScriptVariable class.
        /// </summary>
        public UnwrapUDEScriptVariable()
          : base("UnwrapUDEScriptVariable", "UWSV",
              "Description",
              "UrbanDesignEngine", "Data Management")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddScriptVariableParameter("UDEAnything", "UDESV", "Any UDEScriptVariable", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("UDEAttributes", "Attr", "Attributes", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "G", "Geometry", GH_ParamAccess.item);
            pManager.AddTextParameter("GUID", "GUID", "GUID", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
           ScriptVariableGetter svg = ScriptVariableGetter.AllAttributableScriptVariableClassesGetter(this, DA, 0, true);

            VariableGetterStatus result = svg.GetAllAttributable(out IAttributable sv);
            
            if (result != VariableGetterStatus.Success) return;
            DA.SetData(0, sv.GetAttributesInstance().GHIOParam);
            // TODO: let all goo-lable, unwrappable svs implement a IGH_GeometricGoo to have Geo and GeoRef if possible
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Unwrap.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0930cedf-d2a2-4f67-bb40-460287acce0d"); }
        }
    }
}