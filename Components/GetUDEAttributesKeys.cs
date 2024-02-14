using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Components
{
    public class QueryUDEAttributesForValue : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the QueryUDEAttributesForValue class.
        /// </summary>
        public QueryUDEAttributesForValue()
          : base("QueryUDEAttributesForValue", "QAttr",
              "Description",
              "UrbanDesignEngine", "Data Management")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddScriptVariableParameter("UDEAttributes", "Attr", "UDEAttributes instance", GH_ParamAccess.item);
            pManager.AddTextParameter("Key", "Ky", "Key", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Value", "Val", "Value", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (ScriptVariableGetter<Attributes>.GetScriptVariable(this, DA, 0, true, out Attributes attr) != VariableGetterStatus.Success) return;
            if (VariableGetter<string>.GetVariable(this, DA, 1, true, out string key) != VariableGetterStatus.Success) return;
            string result = attr.Get<string>(key);
            if (String.IsNullOrEmpty(result)) return;
            DA.SetData(0, result);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fff18837-e3bd-47ab-8ea7-473ed71cbd0a"); }
        }
    }
}