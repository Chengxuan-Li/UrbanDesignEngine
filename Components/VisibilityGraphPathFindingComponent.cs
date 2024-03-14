using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using UrbanDesignEngine.Triangulation;
using UrbanDesignEngine.DataStructure;

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
            pManager.AddPointParameter("SamplePoints", "SPs", "Sample Points", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("VisibilityGraph", "VG", "Visibility Graph", GH_ParamAccess.item);
            
            pManager.AddGenericParameter("VisibilityVertices", "VVs", "Visibility Vertices", GH_ParamAccess.list);
            pManager.AddGenericParameter("VisibilityEdges", "VEs", "Visibility Edges", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = default;
            List<Point3d> sps = new List<Point3d>();
            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataList(1, sps)) return;


            List<Point3d> additionalPoints = sps.ToList();
         

            VisibilityGraph vg = new VisibilityGraph(mesh, additionalPoints, VisibilityGraphSettings.Default);

            List<GHIOPointParam<VGVertex>> verticesParams = new List<GHIOPointParam<VGVertex>>();
            vg.Graph.Vertices.ToList().ForEach(v => verticesParams.Add(v.gHIOParam));
            List<GHIOGraphCurveParam<VGEdge>> edgesParams = new List<GHIOGraphCurveParam<VGEdge>>();
            vg.Graph.Edges.ToList().ForEach(e => edgesParams.Add(e.gHIOParam));

            DA.SetData(0, vg.GHIOParam);
            DA.SetDataList(1, verticesParams);
            DA.SetDataList(2, edgesParams);
            
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