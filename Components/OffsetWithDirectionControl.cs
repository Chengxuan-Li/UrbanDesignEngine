using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.Utilities;

namespace UrbanDesignEngine.Components
{
    public class OffsetWithDirectionControl : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the OffsetWithDirectionControl class.
        /// </summary>
        public OffsetWithDirectionControl()
          : base("OffsetWithDirectionControl", "ODC",
              "Offset curve with direction control",
             "UrbanDesignEngine", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curves to offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "D", "Distance of offset", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Inwards", "In", "True if offsetting inwards, otherwise false; if the input curve is not enclosed, this control is meaningless", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Offset Curve", "OC", "Resultant curve after offset", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = default;
            if (!DA.GetData(0, ref curve)) return;
            double distance = default;
            if (!DA.GetData(1, ref distance)) return;
            bool inwards = true;
            DA.GetData(2, ref inwards);
            Polyline pl;
            Curve result;
            result = OffsetCurve.OffsetWithDirection(curve, distance, inwards);
            DA.SetData(0, result);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Offset.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f0705444-2689-4fb7-868a-d875094514e9"); }
        }
    }
}