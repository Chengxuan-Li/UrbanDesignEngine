﻿using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Components
{
    public class GetUDEAttributesKeys : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetUDEAttributesKeys class.
        /// </summary>
        public GetUDEAttributesKeys()
          : base("GetUDEAttributesKeys", "AttrKys",
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Keys", "Kys", "List of keys", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (ScriptVariableGetter.GetScriptVariable<Attributes>(this, DA, 0, true, out Attributes attr) != VariableGetterStatus.Success) return;
            DA.SetDataList(0, attr.Content.Keys.ToList());
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
            get { return new Guid("0724fa41-2a88-4ec2-ae1d-fa3e51e8bfcf"); }
        }
    }
}