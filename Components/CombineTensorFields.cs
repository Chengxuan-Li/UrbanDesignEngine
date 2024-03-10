using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.Tensor;

namespace UrbanDesignEngine.Components
{
    public class CombineTensorFields : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CombineTensorFields class.
        /// </summary>
        public CombineTensorFields()
          : base("CombineTensorFields", "CTF",
              "Combine several tensor fields",
              "UrbanDesignEngine", "TensorField")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddScriptVariableParameter("UDETensorField1", "UTF1", "UDETensorField instance 1", GH_ParamAccess.item);
            pManager.AddScriptVariableParameter("UDETensorField2", "UTF2", "UDETensorField instance 2", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("UDETensorField", "UTF", "UDETensorField after combination", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ScriptVariableGetter svg0 = ScriptVariableGetter.AllAttributableScriptVariableClassesGetter(this, DA, 0, true);
            MultipleTensorFields tfm0 = default;
            VariableGetterStatus result = svg0.GetVariableFromAllSimpleTensorFieldTypes(out SimpleTensorField tf0);
            if (result == VariableGetterStatus.TypeError)
            {
                result = ScriptVariableGetter.GetScriptVariable<MultipleTensorFields>(this, DA, 0, true, out tfm0);
                if (result != VariableGetterStatus.Success) return;
            } else
            {
                tfm0 = new MultipleTensorFields(tf0);
            }

            ScriptVariableGetter svg1 = ScriptVariableGetter.AllAttributableScriptVariableClassesGetter(this, DA, 1, true);
            MultipleTensorFields tfm1 = default;
            result = svg1.GetVariableFromAllSimpleTensorFieldTypes(out SimpleTensorField tf1);
            if (result == VariableGetterStatus.TypeError)
            {
                result = ScriptVariableGetter.GetScriptVariable<MultipleTensorFields>(this, DA, 1, true, out tfm1);
                if (result != VariableGetterStatus.Success) return;
            }
            else
            {
                tfm1 = new MultipleTensorFields(tf1);
            }

            //tfm0 = tfm0.Duplicate();

            //tfm1 = tfm1.Duplicate();

            tfm0.Multiply(tfm1);

            DA.SetData(0, tfm0.gHIOParam);

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
            get { return new Guid("5b866484-8a1a-4e7e-87fa-dead673f25c5"); }
        }
    }
}