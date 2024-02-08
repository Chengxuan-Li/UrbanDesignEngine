using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.IO;

namespace UrbanDesignEngine.Components
{
    public class DualGraph : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DualGraph class.
        /// </summary>
        public DualGraph()
          : base("DualGraph", "DG",
              "Create the dual graph of a planar graph",
              "UrbanDesignEngine", "Graph")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("UDEGraph", "G", "UDEGraph (planar graph) to create dual from", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("DualGraph", "DG", "Created dual graph", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!ScriptVariableGetter<NetworkGraph>.GetScriptVariable(this, DA, 0, out NetworkGraph graph)) return;
            NetworkGraph dualGraph = NetworkGraph.DualGraph(graph);
            DA.SetData(0, dualGraph.GHIOParam);
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
            get { return new Guid("76ae1752-629b-4cd3-8ad1-11b752e49dc6"); }
        }
    }
}