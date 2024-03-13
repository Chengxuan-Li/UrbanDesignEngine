using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.Triangulation;

namespace UrbanDesignEngine.Components
{
    public class NavMeshTestComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the NavMeshTestComponent class.
        /// </summary>
        public NavMeshTestComponent()
          : base("NavMeshTestComponent", "NavMesh",
              "NavMeshTest",
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
            //pManager.AddPointParameter("StartPts", "SPtS", "Start Points", GH_ParamAccess.list);
            pManager.AddPointParameter("StartPt", "SPt", "Start Point", GH_ParamAccess.item);
            pManager.AddPointParameter("EndPts", "EPtS", "End Points", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Paths", "Paths", "Paths", GH_ParamAccess.list);
            pManager.AddMeshParameter("TM", "TM", "TempMesh", GH_ParamAccess.item);
            pManager.AddPointParameter("TS", "TS", "TempStart", GH_ParamAccess.item);
            pManager.AddPointParameter("TE", "TE", "TempEnd", GH_ParamAccess.item);
            pManager.AddLineParameter("TLs", "TLs", "TempLines", GH_ParamAccess.list);
            pManager.AddCurveParameter("RBL", "RBL", "Right Boundary", GH_ParamAccess.item);
            pManager.AddCurveParameter("LBL", "LBL", "Left Boundary", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve boundary = default;
            List<Curve> holes = new List<Curve>();
            List<Point3d> spts = new List<Point3d>();
            List<Point3d> epts = new List<Point3d>();
            if (!DA.GetData(0, ref boundary)) return;
            if (!DA.GetDataList(1, holes)) return;
            if (!boundary.TryGetPolyline(out Polyline boundaryPl)) return;
            Point3d spt = default;
            if (!DA.GetData(2, ref spt)) return;
            //if (!DA.GetDataList(2, spts)) return;
            if (!DA.GetDataList(3, epts)) return;
            List<Polyline> holesPls = new List<Polyline>();
            foreach (Curve crv in holes)
            {
                if (!crv.TryGetPolyline(out Polyline pl)) return;
                holesPls.Add(pl);
            }
            TriangleNet.Geometry.Polygon polygon = Triangulation.TriangleNetParser.ToTNPolygon(boundaryPl, holesPls);
            var imesh = Triangulation.Delaunay.ConsDT(polygon);
            var mesh = Triangulation.TriangleNetParser.ToRhinoMesh(imesh);
            DA.SetData(0, mesh);

            /*
            NavMesher nvm = new NavMesher(imesh);

            nvm.Path(TriangleNetParser.ToRcVecList(spts.ConvertAll(p => new Vector3d(p))),
                TriangleNetParser.ToRcVecList(epts.ConvertAll(p => new Vector3d(p))),
                out var paths,
                out var sPaths
                );
            List<Polyline> pls = new List<Polyline>();
            sPaths.ForEach(sp => pls.Add(TriangleNetParser.StraightPathToPolyline(sp)));
            var crvs = pls.ConvertAll(p => p.ToNurbsCurve());
            DA.SetDataList(1, crvs);

            */

            MeshNav mnv = new MeshNav(mesh);
            mnv.SolveFaceChain(spt, epts);
            DA.SetData(2, mnv.FaceChains[0].Mesh);
            DA.SetData(3, mnv.FaceChains[0].Start);
            DA.SetData(4, mnv.FaceChains[0].End);
            DA.SetData(6, mnv.FaceChains[0].RightOutline.ToNurbsCurve());
            DA.SetData(7, mnv.FaceChains[0].LeftOutline.ToNurbsCurve());
            List<Polyline> pls  = mnv.SolveStraightPaths();
            DA.SetDataList(1, pls.ConvertAll(p => p.ToNurbsCurve()));
            DA.SetDataList(5, mnv.FaceChains[0].PortalLines);
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
            get { return new Guid("2541ea1f-6526-4a95-82d9-df10f59f4b18"); }
        }
    }
}