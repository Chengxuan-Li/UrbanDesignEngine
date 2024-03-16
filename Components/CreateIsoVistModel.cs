using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace UrbanDesignEngine.Components
{
    public class CreateIsoVistModel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateIsoVistModel class.
        /// </summary>
        public CreateIsoVistModel()
          : base("CreateIsoVistModel", "ISVT",
              "Create an IsoVist model for view analysis",
              "UrbanDesignEngine", "View Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Obstacles", "oLns", "Obstacles as Lines", GH_ParamAccess.list);
            pManager.AddNumberParameter("MaxRadius", "MaxR", "Max Radius", GH_ParamAccess.item);
            pManager.AddIntegerParameter("RayCount", "NRay", "Ray Count", GH_ParamAccess.item);
            pManager.AddIntegerParameter("SubRayCount", "NSubRay", "Ray Subdivision Count", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("IsoVistModel", "ISVTModel", "IsoVist Model for view analysis", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Line> lines = new List<Line>();
            double maxRadius = 800.0;
            int rayCount = 45;
            int subRayCount = 10;
            if (!DA.GetDataList(0, lines)) return;
            DA.GetData(1, ref maxRadius);
            DA.GetData(2, ref rayCount);
            DA.GetData(3, ref subRayCount);

            Algorithms.IsoVistComputationalSettings settings = new Algorithms.IsoVistComputationalSettings();
            settings.MaxRadius = maxRadius;
            settings.RayCount = rayCount;
            settings.SubRayCount = subRayCount;

            Algorithms.IsoVist isoVist = Algorithms.IsoVist.CreateObstacleModel(lines, settings);

            DA.SetData(0, isoVist.GHIOParam);

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
            get { return new Guid("18957f9f-635d-458b-a2b5-60852b467a4c"); }
        }
    }
}