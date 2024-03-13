using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.Triangulation;

namespace UrbanDesignEngine.Components
{
    public class VisibilityGraphPathFindingComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the VisibilityGraphPathFindingComponent class.
        /// </summary>
        public VisibilityGraphPathFindingComponent()
          : base("VisibilityGraphPathFindingComponent", "VisGNav",
              "VisibilityGraphPathFindingComponent",
              "UrbanDesignEngine", "Triangulation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("ConsDTMesh", "CDTM", "Constrained Delaunay Triangulation Mesh", GH_ParamAccess.item);
            pManager.AddPointParameter("StartPoint", "SP", "Start Point", GH_ParamAccess.item);
            pManager.AddPointParameter("EndPoint", "EP", "End Point", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Path", "P", "Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = default;
            Point3d start = default;
            Point3d end = default;
            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref start)) return;
            if (!DA.GetData(2, ref end)) return;

            VisibilityGraph vg = new VisibilityGraph(mesh, new List<Point3d> { start, end }, VisibilityGraphSettings.Default);
            var pathPts = AStarAlgorithm.AStar(start, end, vg.Graph);

            DA.SetData(0, new Polyline(pathPts).ToNurbsCurve());
            
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
            get { return new Guid("2d918222-ad4e-4dbe-978f-3a51e34d689e"); }
        }
    }
}