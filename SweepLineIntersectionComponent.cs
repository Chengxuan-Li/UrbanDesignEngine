using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.Algorithms;


namespace UrbanDesignEngine
{
    public class SweepLineIntersectionComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SweepLineIntersection class.
        /// </summary>
        public SweepLineIntersectionComponent()
          : base("SweepLineIntersection", "SLI",
              "Description",
              "UrbanDesignEngine", "Models")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "L", "Line", GH_ParamAccess.item);
            pManager.AddLineParameter("Network", "Nw", "Network", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Indices", "Id", "Indices", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Line line = new Line();
            if (!DA.GetData(0, ref line)) return;
            List<Line> network = new List<Line>();
            if (!DA.GetDataList(1, network)) return;
            List<double> result;
            SweepLineIntersection.TempLineNetworkIntersection(line, network, out result);
            DA.SetDataList(0, result);
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
            get { return new Guid("6f28e802-78cc-4ccb-b4ef-c495e8087cb2"); }
        }
    }
}