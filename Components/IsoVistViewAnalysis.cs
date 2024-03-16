using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.Algorithms;

namespace UrbanDesignEngine.Components
{
    public class IsoVistViewAnalysis : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IsoVistViewAnalysis class.
        /// </summary>
        public IsoVistViewAnalysis()
          : base("IsoVistViewAnalysis", "ISVTView",
              "IsoVistViewAnalysis",
              "UrbanDesignEngine", "View Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddScriptVariableParameter("ISVTModel", "ISVT", "IsoVist Model", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "Pt", "Point", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Pt", "Point", GH_ParamAccess.item); // 0
            pManager.AddPointParameter("HitLocs", "HitLocs", "Hit Locations", GH_ParamAccess.list); // 1
            pManager.AddIntegerParameter("Ids", "Ids", "Ids", GH_ParamAccess.list); // 2
            pManager.AddNumberParameter("Radii", "Rs", "Radii", GH_ParamAccess.list); // 3

            pManager.AddCurveParameter("Outline", "OL", "Outline", GH_ParamAccess.item); // 4
            pManager.AddNumberParameter("Perimeter", "P", "Perimeter", GH_ParamAccess.item); // 5
            pManager.AddNumberParameter("Area", "A", "Area", GH_ParamAccess.item); // 6

            pManager.AddPointParameter("Centroid", "Ce", "Centroid", GH_ParamAccess.item); // 7

            pManager.AddNumberParameter("Compactness", "Cp", "Compactness", GH_ParamAccess.item); // 8
            pManager.AddNumberParameter("ClosedPerimeter", "CP", "ClosedPerimeter", GH_ParamAccess.item); // 9
            pManager.AddNumberParameter("Occlusivity", "Occ", "Occlusivity", GH_ParamAccess.item); // 10
            pManager.AddNumberParameter("Vista", "MV", "Vista", GH_ParamAccess.item); // 11
            pManager.AddNumberParameter("AverageRadii", "MR", "AverageRadii", GH_ParamAccess.item); // 12
            pManager.AddNumberParameter("Drift", "Dr", "Drift", GH_ParamAccess.item); // 13
            pManager.AddNumberParameter("Variance", "Var", "Variance", GH_ParamAccess.item); // 14
            pManager.AddNumberParameter("Skewness", "Sk", "Skewness", GH_ParamAccess.item); // 15
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IsoVist isoVist = default;
            if (ScriptVariableGetter.GetScriptVariable<IsoVist>(this, DA, 0, true, out isoVist) != VariableGetterStatus.Success) return;
            Point3d point = new Point3d();
            if (!DA.GetData(1, ref point)) return;

            IsoVistResult result = isoVist.Compute(point);

            DA.SetData(0, result.Point);
            DA.SetDataList(1, result.Points);
            DA.SetDataList(2, result.Ids);
            DA.SetDataList(3, result.Radii);
            DA.SetData(4, result.BoundaryLine);
            DA.SetData(5, result.Perimeter);
            DA.SetData(6, result.Area);
            DA.SetData(7, result.Centroid);
            DA.SetData(8, result.Compactness);
            DA.SetData(9, result.ClosedPerimeter);
            DA.SetData(10, result.Occlusivity);
            DA.SetData(11, result.Vista);
            DA.SetData(12, result.AverageRadii);
            DA.SetData(13, result.Drift);
            DA.SetData(14, result.Variance);
            DA.SetData(15, result.Skewness);
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
            get { return new Guid("a475be58-bbdc-48a9-8798-d04c5de05d8f"); }
        }
    }
}