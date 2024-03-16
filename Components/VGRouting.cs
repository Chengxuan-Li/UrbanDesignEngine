using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.Triangulation;

namespace UrbanDesignEngine.Components
{
    public class VGRouting : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the VGRouting class.
        /// </summary>
        public VGRouting()
          : base("VGRouting", "VGNav",
              "VG Routing",
              "UrbanDesignEngine", "Triangulation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddScriptVariableParameter("VisibilityGraph", "VG", "Visibility Graph", GH_ParamAccess.item);
            pManager.AddPointParameter("StartPoints", "SPs", "Starting Points", GH_ParamAccess.list);
            pManager.AddPointParameter("EndPoints", "EPs", "Ending Points", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Routes", "R", "Routes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!(ScriptVariableGetter.GetScriptVariable<VisibilityGraph>(this, DA, 0, true, out VisibilityGraph vg) == VariableGetterStatus.Success)) return;
            List<Point3d> sps = new List<Point3d>();
            List<Point3d> eps = new List<Point3d>();
            if (!DA.GetDataList(1, sps)) return;
            if (!DA.GetDataList(2, eps)) return;

            Algorithms.VGShortestPath vsp = new Algorithms.VGShortestPath(vg.Graph);

            int count = Math.Min(sps.Count, eps.Count);
            List<Curve> paths = new List<Curve>();

            for (int i = 0; i < count; i++)
            {
                // change epsilonequals to perhaps min distance index (argmin) ?
                var vs = vg.Graph.Vertices.ToList().Find(v => v.Location.EpsilonEquals(sps[i], GlobalSettings.AbsoluteTolerance));
                var ve = vg.Graph.Vertices.ToList().Find(v => v.Location.EpsilonEquals(eps[i], GlobalSettings.AbsoluteTolerance));

                if (vs == null || ve == null)
                {
                    paths.Add(default);
                    continue;
                }
                vsp.Solve(vs);
                if (!vsp.PathTo(ve, out var path))
                {
                    path.Add(default);
                    continue;
                }
                paths.Add(Curve.JoinCurves(path.ConvertAll(p => new Line(p.Source.Location, p.Target.Location).ToNurbsCurve()))[0]);
            }

            DA.SetDataList(0, paths);

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
            get { return new Guid("59c43a36-a74c-49a9-9df7-fe55d2585216"); }
        }
    }
}