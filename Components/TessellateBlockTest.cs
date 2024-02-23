using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace UrbanDesignEngine.Components
{
    public class TessellateBlockTest : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public TessellateBlockTest()
          : base("TessellateBlockTest", "TessellateBlockTest",
              "Description",
              "UrbanDesignEngine", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Crv", "Curve", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MaxIterations", "MaxIterations", "MaxIterations", GH_ParamAccess.item);
            pManager.AddNumberParameter("IntersectOneLineMaxDistance", "IntersectOneLineMaxDistance", "IntersectOneLineMaxDistance", GH_ParamAccess.item);
            pManager.AddNumberParameter("MinArea", "MinArea", "MainArea", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaxArea", "MaxArea", "MaxArea", GH_ParamAccess.item);
            pManager.AddNumberParameter("MinAngleDiff", "MinAngleDiff", "MinAngleDiff", GH_ParamAccess.item);
            pManager.AddNumberParameter("MinGap", "MinGap", "MinGap", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaxGap", "MaxGap", "MaxGap", GH_ParamAccess.item);
            pManager.AddIntegerParameter("TargetCount", "TargetCount", "TargetCount", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Regions", "Regions", "Regions", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv = default;
            if (!DA.GetData(0, ref crv)) return;
            Polyline pl;
            if (!crv.TryGetPolyline(out pl)) return;
            int maxIterations = 3;
            DA.GetData(1, ref maxIterations);
            double IOLD = 50;
            DA.GetData(2, ref IOLD);
            double minArea = 100;
            DA.GetData(3, ref minArea);
            double maxArea = 800;
            DA.GetData(4, ref maxArea);
            double minAngleDiff = Math.PI / 6.0;
            DA.GetData(5, ref minAngleDiff);
            double minGap = 8;
            DA.GetData(6, ref minGap);
            double maxGap = 40;
            DA.GetData(7, ref maxGap);
            int targetCount = 3;
            DA.GetData(8, ref targetCount);

            TessellationTest tt = new TessellationTest(pl, new List<TessellationTest>());
            TessellationParameters tessellationParameters = TessellationParameters.DefaultParameters;
            tessellationParameters.MaxIterations = maxIterations;
            tessellationParameters.IntersectOneLineMaxDistance = IOLD;
            tessellationParameters.MinArea = minArea;
            tessellationParameters.MaxArea = maxArea;
            tessellationParameters.MinAngleDiff = minAngleDiff;
            tessellationParameters.MinGap = minGap;
            tessellationParameters.MaxGap = maxGap;
            tt.TessellationParameters = tessellationParameters;

            TessellationTest.SolutionStat result = TessellationTest.SolutionStat.continu;
            while (tt.Divisions.Count < targetCount && result != TessellationTest.SolutionStat.finished)
            {
                new TessellationTest(pl, new List<TessellationTest>());
                tt.TessellationParameters = tessellationParameters;
                result = tt.Solve();
            }
            
            List<Curve> crvs = new List<Curve>();
            foreach(var stt in tt.Divisions)
            {
                crvs.Add(stt.BoundaryLine.ToNurbsCurve());
            }
            DA.SetDataList(0, crvs);
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
            get { return new Guid("b7acbf88-1588-4065-8bd8-7b4a57f3a835"); }
        }
    }
}