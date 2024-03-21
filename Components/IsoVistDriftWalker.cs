using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.Algorithms;

namespace UrbanDesignEngine.Components
{
    public class IsoVistDriftWalker : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IsoVistDriftWalker class.
        /// </summary>
        public IsoVistDriftWalker()
          : base("IsoVistDriftWalker", "ISVTDWalker",
              "IsoVistDriftWalker",
              "UrbanDesignEngine", "View Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddScriptVariableParameter("ISVTModel", "ISVT", "IsoVist Model", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "Pt", "Point", GH_ParamAccess.item);
            pManager.AddNumberParameter("StepLength", "SL", "Step Length", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumSteps", "NS", "Number of Steps", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Path", "Path", "Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IsoVist isoVist = default;
            if (ScriptVariableGetter.GetScriptVariable<IsoVist>(this, DA, 0, true, out isoVist) != VariableGetterStatus.Success) return;
            Point3d point = new Point3d();
            if (!DA.GetData(1, ref point)) return;

            double stepLength = 1.0;
            int numIterations = 20;

            DA.GetData(2, ref stepLength);
            DA.GetData(3, ref numIterations);

            IsoVistWalker walker = new IsoVistWalker(isoVist, point, stepLength);
            List<Point3d> pts = walker.DriftWalker(numIterations);

            DA.SetData(0, new Polyline(pts).ToNurbsCurve());

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
            get { return new Guid("0f644010-035f-4ec7-bff1-8e1f76875e99"); }
        }
    }
}