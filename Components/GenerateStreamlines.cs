using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.Tensor;

namespace UrbanDesignEngine.Components
{
    public class GenerateStreamlines : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GenerateStreamlines class.
        /// </summary>
        public GenerateStreamlines()
          : base("GenerateStreamlines", "GSl",
              "Create streamlines of a given tensor field",
              "UrbanDesignEngine", "TensorField")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddScriptVariableParameter("UDETensorField", "UTF", "UDETensorField instance", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Streamlines", "Sl", "Generated streamlines", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ScriptVariableGetter svg0 = ScriptVariableGetter.AllAttributableScriptVariableClassesGetter(this, DA, 0, true);
            MultipleTensorFields tfm0 = default;
            VariableGetterStatus result = svg0.GetVariableFromAllSimpleTensorFieldTypes(out SimpleTensorField tf0);
            if (result == VariableGetterStatus.TypeError)
            {
                result = ScriptVariableGetter.GetScriptVariable<MultipleTensorFields>(this, DA, 0, true, out tfm0);
                if (result != VariableGetterStatus.Success) return;
            }
            else
            {
                tfm0 = new MultipleTensorFields(tf0);
            }

            RK4Integrator integrator = new RK4Integrator(tfm0, StreamlineParams.Default);
            StreamlineGenerator sg = new StreamlineGenerator(integrator, new Vector3d(0, 0, 0), new Vector3d(100, 100, 0), StreamlineParams.Default);
            sg.RunCreateAllStreamlines();
            List<Curve> pls = sg.allStreamlinesSimple.ConvertAll(vs => (Curve) new Polyline(vs.ConvertAll(v => new Point3d(v))).ToNurbsCurve());
            DA.SetDataList(0, pls);

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
            get { return new Guid("e3747a2d-cef4-4b85-bf59-76ad295428d2"); }
        }
    }
}