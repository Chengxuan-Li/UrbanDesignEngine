using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.Tensor;

namespace UrbanDesignEngine.Components
{
    public class CreateRadialTensorField : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateRadialTensorField class.
        /// </summary>
        public CreateRadialTensorField()
          : base("CreateRadialTensorField", "CRadTF",
              "Create a tensor field based on a point attractor",
              "UrbanDesignEngine", "TensorField")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("PointAttractor", "Pt", "Attractor point", GH_ParamAccess.item);
            pManager.AddNumberParameter("DecayRange", "DR", "Decay range", GH_ParamAccess.item);
            pManager.AddNumberParameter("ExtentRadius", "ER", "Extent radius", GH_ParamAccess.item);
            pManager.AddNumberParameter("Factor", "Fac", "Multiplication factor", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("UDETensorField", "UTF", "UDETensorField instance", GH_ParamAccess.item);
            pManager.AddCurveParameter("PHCrvsPreview", "PHCP", "Placeholder curves preview", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d pt = default;
            if (!DA.GetData(0, ref pt)) return;
            double decayRange = 600;
            DA.GetData(1, ref decayRange);
            double extentRadius = 500;
            DA.GetData(2, ref extentRadius);
            double factor = 1;
            DA.GetData(3, ref factor);

            RadialTensorField tf = new RadialTensorField(pt, decayRange, extentRadius);
            tf.Factor = factor;

            DA.SetData(0, tf.GHIOParam);
            DA.SetDataList(1, tf.PreviewGeometryList);
            
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
            get { return new Guid("875ad673-f19c-428d-a959-06eea8ebf1cd"); }
        }
    }
}