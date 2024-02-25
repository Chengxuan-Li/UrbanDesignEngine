using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace UrbanDesignEngine.Components
{
    public class SnapDemoComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SnapDemoComponent class.
        /// </summary>
        public SnapDemoComponent()
          : base("SnapDemoComponent", "SnapDemo",
              "SnapDemoComponent",
              "UrbanDesignEngine", "Models")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Source", "Src", "Source", GH_ParamAccess.item);
            pManager.AddPointParameter("IntendedTarget", "InTgt", "Intended target", GH_ParamAccess.item);
            pManager.AddLineParameter("Obstacles", "Obs", "Onstacles", GH_ParamAccess.list);
            pManager.AddNumberParameter("R", "R", "Radius", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("HasSnapEvent", "HSE", "Has Snap Event", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "Dist", "Distance", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "Pt", "Point to snap to", GH_ParamAccess.item);
            pManager.AddIntegerParameter("LineId", "LID", "Index of the line that causes snap event", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d source = default;
            Point3d intendedTarget = default;
            List<Line> obstacles = new List<Line>();
            double r = default;
            if (!DA.GetData(0, ref source)) return;
            if (!DA.GetData(1, ref intendedTarget)) return;
            if (!DA.GetDataList(2, obstacles)) return;
            if (!DA.GetData(3, ref r)) return;
            Algorithms.Snap snap = new Algorithms.Snap(source, intendedTarget, obstacles, r);
            var result = snap.Solve(out double distance, out Point3d pt, out int id);
            DA.SetData(0, result.ToString());
            DA.SetData(1, distance);
            DA.SetData(2, pt);
            DA.SetData(3, id);

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
            get { return new Guid("10d78566-0f4c-4014-9052-f5c731c2b347"); }
        }
    }
}