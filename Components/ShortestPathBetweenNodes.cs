using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Components
{
    public class ShortestPathBetweenNodes : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ShortestPathComponent class.
        /// </summary>
        public ShortestPathBetweenNodes()
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
            pManager.AddIntegerParameter("RootIndex", "RootId", "Root node index", GH_ParamAccess.item);
            pManager.AddIntegerParameter("TargetIndex", "TarId", "Target node index", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("PathNodeIds", "PathNodeIds", "Path nodes ids", GH_ParamAccess.list);
            pManager.AddNumberParameter("Distance", "Dist", "Distance", GH_ParamAccess.item);
            pManager.AddCurveParameter("PathCurves", "PathCrvs", "Path curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!(ScriptVariableGetter.GetScriptVariable<NetworkGraph>(this, DA, 0, true, out NetworkGraph graph) == VariableGetterStatus.Success)) return;
            int root = -1;
            int target = -1;
            if (!DA.GetData(1, ref root)) return;
            if (!DA.GetData(2, ref target)) return;

            var sp = new Algorithms.NShortestPath(graph);
            sp.Solve(graph.Graph.Vertices.ToList()[root]);

            bool pathResult = sp.PathTo(graph.Graph.Vertices.ToList()[target], out List<NetworkEdge> path);
            bool distanceResult = sp.Distances().TryGetValue(graph.Graph.Vertices.ToList()[target], out double distance);

            if (pathResult)
            {
                DA.SetDataList(0, path.ConvertAll(p => p.Id));
                List<Curve> crvs = new List<Curve>();
                path.ToList().ForEach(e => crvs.Add(e.UnderlyingCurve));
                DA.SetDataList(2, crvs);
            }

            if (distanceResult)
            {
                DA.SetData(1, distance);
            }

            sp.Dispose();

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
                return Properties.Resources.SPNode.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a600179e-62e8-4c29-a0c6-199dc90a2298"); }
        }
    }
}