using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Components
{
    public class AddRhinoAttribute : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AddUDEAttribute class.
        /// </summary>
        public AddRhinoAttribute()
          : base("AddRhinoAttribute", "AddAttr",
              "Add attribute to a Rhino geometry",
              "UrbanDesignEngine", "Data Management")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("GUID", "GUID", "GUID", GH_ParamAccess.item);
            pManager.AddTextParameter("Key", "Key", "Key", GH_ParamAccess.item);
            pManager.AddTextParameter("Value", "Value", "Value", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("GUID", "GUID", "GUID", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Result", "R", "Result, true if success", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string guid = default;
            if (!DA.GetData(0, ref guid)) return;
            string key = default;
            string val = default;
            if (!DA.GetData(1, ref key)) return;
            if (!DA.GetData(2, ref val)) return;

            InFileAttributes attributes = InFileAttributes.FromGuid(new Guid(guid));
            DA.SetData(0, guid);
            DA.SetData(1, attributes.Set(key, val));
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
            get { return new Guid("2b2a436a-2069-4f1b-8a8f-0f37bbc9f137"); }
        }
    }
}