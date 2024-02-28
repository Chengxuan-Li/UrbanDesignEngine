using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using UrbanDesignEngine.Algorithms;

namespace UrbanDesignEngine.Components
{
    public class KellyMcCabeLotSubdivision : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the KellyMcCabeLotSubdivision class.
        /// </summary>
        public KellyMcCabeLotSubdivision()
          : base("KellyMcCabeLotSubdivision", "Nickname",
              "Kelly-McCabe Lot Subdivision",
              "UrbanDesignEngine", "Models")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("PlotBoundary", "PB", "Plot Boundary Polyline Curve", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaximumAreaTarget", "MaxArea", "Maximum Area", GH_ParamAccess.item);
            pManager.AddNumberParameter("MinimumAreaTarget", "MinArea", "Minimum Area", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MaxLocalAttempts", "MaxLocalAttempts", "MaxLocalAttempts", GH_ParamAccess.item);
            pManager.AddNumberParameter("DivisionLineMaxDeviation", "DivisionLineMaxDeviation", "DivisionLineMaxDeviation", GH_ParamAccess.item);
            pManager.AddNumberParameter("DivisibleThreshold", "DivisibleThreshold", "DivisibleThreshold", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Seed", "Seed", "Seed", GH_ParamAccess.item);
            
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("TextMsg", "TextMsg", "TextMsg", GH_ParamAccess.item);
            pManager.AddCurveParameter("Subdivisions", "SubDCrvs", "Subdivision Curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv = default;
            if (!DA.GetData(0, ref crv)) return;
            double maxAreaTarget = 800;
            DA.GetData(1, ref maxAreaTarget);
            double minAreaTarget = 80;
            DA.GetData(2, ref minAreaTarget);
            int maxLocalAttempts = 3;
            DA.GetData(3, ref maxLocalAttempts);
            double divisionLineMaxDeviation = 4.0;
            DA.GetData(4, ref divisionLineMaxDeviation);
            double divisibleThreshold = 8.0;
            DA.GetData(5, ref divisibleThreshold);
            int seed = 0;
            DA.GetData(6, ref seed);

            KellyMcCabeSubdivitionSetting setting = new KellyMcCabeSubdivitionSetting
            {
                MaximumAreaTarget = maxAreaTarget,
                MinimumAreaTarget = minAreaTarget,
                MaxLocalAttempts = maxLocalAttempts,
                DivisionLineMaxDeviation = divisionLineMaxDeviation,
                DivisibleThreshold = divisibleThreshold,
            };


            if (!crv.TryGetPolyline(out Polyline pl)) return;
            KellyMcCabeLotSubdivisionModel model = new KellyMcCabeLotSubdivisionModel(pl, new Random(seed)) { Setting = setting };
            var result = model.Solve(out List<KellyMcCabeSubdivisionState> states);

            DA.SetData(0, result.ToString());

            List<Curve> resultCrvs = new List<Curve>();

            states.ForEach(s => {
                Curve c = Curve.JoinCurves(s.BoundaryLines.ConvertAll(l => l.ToNurbsCurve()))[0];
                resultCrvs.Add(c);
            });

            DA.SetDataList(1, resultCrvs);

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
            get { return new Guid("b1d23e7f-c798-480d-91bc-32d8a626a929"); }
        }
    }
}