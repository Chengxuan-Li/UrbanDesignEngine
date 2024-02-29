using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Components
{
    public class ShortestPathBetweenPoints : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ShortestPathComponent class.
        /// </summary>
        public ShortestPathBetweenPoints()
          : base("ShortestPath", "SP",
              "Shortest Path using Dijkstra algorithm",
              "UrbanDesignEngine", "Graph")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddScriptVariableParameter("UDEGraph", "G", "UDEGraph (planar graph) to create dual from", GH_ParamAccess.item);
            pManager.AddPointParameter("FromPoint", "FromPt", "From point location", GH_ParamAccess.item);
            pManager.AddPointParameter("ToPoint", "ToPt", "From point location", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("ClosestPoint", "ClosestPT", "Closest point", GH_ParamAccess.item);
            pManager.AddIntegerParameter("PathNodeIds", "PathNodeIds", "Path nodes ids", GH_ParamAccess.list);
            pManager.AddNumberParameter("Distance", "Dist", "Distance", GH_ParamAccess.list);
            pManager.AddCurveParameter("PathCurves", "PathCrvs", "Path curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!(ScriptVariableGetter.GetScriptVariable<NetworkGraph>(this, DA, 0, true, out NetworkGraph graph) == VariableGetterStatus.Success)) return;
            Point3d fromPt = default;
            Point3d toPt = default;
            if (!DA.GetData(1, ref fromPt)) return;
            if (!DA.GetData(2, ref toPt)) return;

            var sp = new Algorithms.ShortestPath(graph);

            sp.SolveShortestPathForPoints(fromPt, new List<Point3d> { toPt }, out List<double> dists, out var _, out List<Curve> crvs);


            DA.SetDataList(2, dists);
            DA.SetDataList(3, crvs);


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

                return Properties.Resources.SPPt.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8939a3d6-4d8b-4df9-bc52-44f7df2c071f"); }
        }
    }
}