using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.Utilities;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.IO;

namespace UrbanDesignEngine.Components
{
    public class GraphFromCurveNetworkWithAttributes : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GraphFromCurveNetwork class.
        /// </summary>
        public GraphFromCurveNetworkWithAttributes()
          : base("GraphFromCurveNetworkWithAttributes", "GCNAttr",
              "Graph from a network of intersecting curves, with Attributes",
              "UrbanDesignEngine", "Graph")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "Crvs", "Curves", GH_ParamAccess.list);
            pManager.AddScriptVariableParameter("UDEAttributes", "Attr", "UDE Attribute class definitions", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("UDEGraph", "G", "Graph generated from the network of curves", GH_ParamAccess.item);
            pManager.AddLineParameter("GraphConnections", "GCs", "Graph connections", GH_ParamAccess.list);
            pManager.AddPointParameter("GraphNodes", "GNs", "Graph nodes", GH_ParamAccess.list);
            pManager.AddCurveParameter("GraphPlanarFaces", "PFs", "Graph planar faces", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> curves = new List<Curve>();
            if (!DA.GetDataList(0, curves)) return;
            List<Attributes> attributesList;
            ScriptVariableGetter<Attributes>.GetScriptVariableList(this, DA, 1, true, out attributesList);
            

            NetworkGraph graph = NetworkCurvesIntersection.NetworkGraphFromCurves(curves);
            graph.SolveFaces();
            DA.SetData(0, graph.GHIOParam);
            DA.SetDataList(1, graph.NetworkEdgesSimpleGeometry);
            DA.SetDataList(2, graph.NetworkNodesGeometry);
            DA.SetDataList(3, graph.NetworkFacesSimpleGeometry);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GraphFromCurveAttr.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("98347f9a-3f49-4a29-ae7a-c0f78dc50e7f"); }
        }
    }
}