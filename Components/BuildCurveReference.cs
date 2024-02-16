using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using UrbanDesignEngine.Utilities;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.IO;

namespace UrbanDesignEngine.Components
{
    public class BuildCurveReference : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BuildCurveReference class.
        /// </summary>
        public BuildCurveReference()
          : base("BuildCurveReference", "Nickname",
              "Description",
              "UrbanDesignEngine", "Data Management")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("CurveGUID", "GUID", "CurveGUID", GH_ParamAccess.item);
            pManager.AddScriptVariableParameter("UDEAttributes", "Attr", "UDE Attribute class definition to modify; leave empty if creating from scratch", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("UDECurveReferences", "CrvRefs", "UDECurveReferences", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string guidString = default;
            if(!DA.GetData(0, ref guidString)) return;
            ScriptVariableGetter svg = ScriptVariableGetter.AllAttributableScriptVariableClassesGetter(this, DA, 1, true);
            if (svg.GetVariableFromAllAttributableTypes(out IAttributable result) != VariableGetterStatus.Success) return;
            if (result.GetType() != typeof(Attributes)) return; // add error message
            Guid guid = new Guid(guidString);
            ReferenceCurveGeometry cref = new ReferenceCurveGeometry(guid, Rhino.RhinoDoc.ActiveDoc, (Attributes)result);
            if (!cref.IsTypeValid) return; // add error message
            DA.SetData(0, cref.gHIOParam);
            
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
            get { return new Guid("471bb90c-80fc-4edf-8c65-a35f23093bc6"); }
        }
    }
}