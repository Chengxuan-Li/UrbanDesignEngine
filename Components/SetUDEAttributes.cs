using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Components
{
    public class SetUDEAttributes : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateGHAttributes class.
        /// </summary>
        public SetUDEAttributes()
          : base("SetUDEAttributes", "CreateGHAttr",
              "Create or modify an instance of the UDEAttributes class for use in Grasshopper",
              "UrbanDesignEngine", "Data Management")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddScriptVariableParameter("UDEAttributes", "Attr", "UDE Attribute class definition to modify; leave empty if creating from scratch", GH_ParamAccess.item);
            pManager.AddTextParameter("GUID", "GUID", "GUID", GH_ParamAccess.item);
            pManager.AddTextParameter("Key", "Key", "Key", GH_ParamAccess.item);
            pManager.AddTextParameter("Value", "Value", "Value", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ModifiedUDEAttributes", "MAttr", "Modified or created attributes", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Attributes attributes = new DataStructure.Attributes();
            VariableGetterStatus getterStatus = ScriptVariableGetter.GetScriptVariable<Attributes>(this, DA, 0, false, out attributes);
            attributes = new DataStructure.Attributes();//TODO Critical
            string guid = default;
            bool guidInput = DA.GetData(1, ref guid);
            string key = default;
            if (!DA.GetData(2, ref key)) return;
            string val = default;
            if (!DA.GetData(3, ref val)) return;


            if (getterStatus == VariableGetterStatus.TypeError) return;
            if (guidInput)
            {
                attributes.Guid = new Guid(guid);
                attributes.SetGeometry(Rhino.RhinoDoc.ActiveDoc.Objects.FindId(attributes.Guid).Geometry);
            }
            attributes.Set(key, val);
            DA.SetData(0, attributes.GHIOParam);
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
            get { return new Guid("45c18b8e-5aea-4381-a0bb-a9fdb81b058c"); }
        }
    }
}