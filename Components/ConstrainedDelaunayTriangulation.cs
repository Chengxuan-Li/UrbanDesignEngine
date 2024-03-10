using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace UrbanDesignEngine.Components
{
    public class ConstrainedDelaunayTriangulation : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ConstrainedDelaunayTriangulation class.
        /// </summary>
        public ConstrainedDelaunayTriangulation()
          : base("ConstrainedDelaunayTriangulation", "ConsDT",
              "Constrained Delaunay Triangulation",
              "UrbanDesignEngine", "Triangulation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("BoundaryPolyline", "BPl", "Boundary Polyline", GH_ParamAccess.item);
            pManager.AddCurveParameter("HolesPolylines", "HPls", "Holes Polylines", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve boundary = default;
            List<Curve> holes = new List<Curve>();
            if (!DA.GetData(0, ref boundary)) return;
            if (!DA.GetDataList(1, holes)) return;
            if (!boundary.TryGetPolyline(out Polyline boundaryPl)) return;
            List<Polyline> holesPls = new List<Polyline>();
            foreach (Curve crv in holes)
            {
                if (!crv.TryGetPolyline(out Polyline pl)) return;
                holesPls.Add(pl);
            }
            TriangleNet.Geometry.Polygon polygon = Triangulation.TriangleNetParser.ToTNPolygon(boundaryPl, holesPls);
            var mesh = Triangulation.TriangleNetParser.ToRhinoMesh(Triangulation.Delaunay.ConsDT(polygon));
            DA.SetData(0, mesh);
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
            get { return new Guid("0494e22c-5497-4d73-81d4-3fe206f9b88d"); }
        }
    }
}