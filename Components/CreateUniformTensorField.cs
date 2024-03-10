using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.Tensor;

namespace UrbanDesignEngine.Components
{
    public class CreateUniformTensorField : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateUniformTensorField class.
        /// </summary>
        public CreateUniformTensorField()
          : base("CreateUniformTensorField", "CUniTF",
              "Create a uniform tensor field based on a vector direction",
              "UrbanDesignEngine", "TensorField")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector", "Vec", "Vector direction", GH_ParamAccess.item);
            //pManager.AddNumberParameter("DecayRange", "DR", "Decay range", GH_ParamAccess.item);
            //pManager.AddNumberParameter("ExtentRadius", "ER", "Extent radius", GH_ParamAccess.item);
            pManager.AddNumberParameter("Factor", "Fac", "Multiplication factor", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MinHierarchy", "MinH", "Minimum level of hierarchy to apply this field", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MaxHierarchy", "MaxH", "Maximum level of hierarchy to apply this field", GH_ParamAccess.item);
            pManager.AddCurveParameter("BoundaryCurve", "BCrv", "Boundary curve for the tensor field", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("UDETensorField", "UTF", "UDETensorField instance", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Vector3d vec = default;
            if (!DA.GetData(0, ref vec)) return;
            double factor = 1;
            DA.GetData(1, ref factor);
            int minH = 0;
            DA.GetData(2, ref minH);
            int maxH = 999;
            DA.GetData(3, ref maxH);
            Curve curve = default;


            SimpleTensorField tf = new SimpleTensorField(vec);
            tf.Factor = factor;
            tf.activationHierarchy = h => (h == -1) || (h >= minH && h <= maxH);
            if (DA.GetData(4, ref curve)) tf.BoundaryCurve = curve;

            DA.SetData(0, tf.gHIOParam);

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
            get { return new Guid("3ab6e8a1-7c67-4a59-9951-ea4ef7d2d8c5"); }
        }
    }
}