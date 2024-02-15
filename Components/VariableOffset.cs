using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using UrbanDesignEngine.Utilities;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.IO;

namespace UrbanDesignEngine.Components
{
    public class VariableOffset : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the VariableOffsetGraphEdges class.
        /// </summary>
        public VariableOffset()
          : base("VariableOffset", "VOffset",
              "VariableOffset",
              "UrbanDesignEngine", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("UDEGraph", "G", "UDEGraph instance", GH_ParamAccess.item);
            pManager.AddTextParameter("DistanceFieldName", "DistF", "Optional specification of the name of the field used as offset distance for each edge", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("ResultantOffsetRegions", "OReg", "Resultatn offset regions", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!(ScriptVariableGetter.GetScriptVariable<NetworkGraph>(this, DA, 0, true, out NetworkGraph graph) == VariableGetterStatus.Success)) return;
            string fieldName = "OffsetDistance";
            DA.GetData(1, ref fieldName);
            DA.SetDataList(0, OffsetCurve.OffsetGraphFaces(graph, fieldName));

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
            get { return new Guid("f03c14ee-9200-40a1-9123-0a6fc40626d9"); }
        }
    }
}