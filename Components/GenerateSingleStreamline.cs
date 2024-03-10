using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.Tensor;
using UrbanDesignEngine.IO;

namespace UrbanDesignEngine.Components
{
    public class GenerateSingleStreamline : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GenerateSingleStreamline class.
        /// </summary>
        public GenerateSingleStreamline()
          : base("GenerateSingleStreamline", "GSS",
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
            pManager.AddPointParameter("Point", "Pt", "Point", GH_ParamAccess.item);
            pManager.AddNumberParameter("StepLength", "SL", "Step length", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Major", "Maj", "Major", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumIterations", "N", "Number of iterations", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Hierarchy", "H", "Hierarchy", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("ResultCurve", "Crv", "Result curve", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /*
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
            */
            ScriptVariableGetter.GetScriptVariable<MultipleTensorFields>(this, DA, 0, true, out var tfm0);

            Point3d pt = default;

            if (!DA.GetData(1, ref pt)) return;

            double stepLength = 0.01;
            DA.GetData(2, ref stepLength);
            bool major = true;
            DA.GetData(3, ref major);
            int iterations = 100;
            DA.GetData(4, ref iterations);
            int hierarchy = -1;
            DA.GetData(5, ref hierarchy);


            Streamline sl = new Streamline(tfm0);
            Polyline pl = sl.Evaluate(pt, hierarchy, major, stepLength, iterations);
            DA.SetData(0, pl.ToNurbsCurve());
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
            get { return new Guid("d7315489-b8f8-43ff-aef0-525455ed862f"); }
        }
    }
}