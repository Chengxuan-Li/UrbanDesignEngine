using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using UrbanDesignEngine.Utilities;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Components
{
    public class GraphFromCurveNetwork : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GraphFromCurveNetwork class.
        /// </summary>
        public GraphFromCurveNetwork()
          : base("GraphFromCurveNetwork", "GCN",
              "Graph from a network of intersecting curves",
              "UrbanDesignEngine", "Graph")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "Crvs", "Curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("UDEGraph", "G", "Graph generated from the network of curves", GH_ParamAccess.item);
            pManager.AddGenericParameter("GraphEdges", "GCs", "Graph connections", GH_ParamAccess.list);
            pManager.AddGenericParameter("GraphNodes", "GNs", "Graph nodes", GH_ParamAccess.list);
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
            NetworkGraph graph = NetworkCurvesIntersection.NetworkGraphFromCurves(curves);


            graph.SolveFaces();
            DA.SetData(0, graph.GHIOParam);
            List<GHIOCurveParam<NetworkEdge>> edgesGHIOParam = new List<GHIOCurveParam<NetworkEdge>>();
            List<GHIOPointParam<NetworkNode>> nodesGHIOParam = new List<GHIOPointParam<NetworkNode>>();
            graph.Graph.Edges.ToList().ForEach(e => edgesGHIOParam.Add(e.gHIOParam));
            graph.Graph.Vertices.ToList().ForEach(v => nodesGHIOParam.Add(v.gHIOParam));
            DA.SetDataList(1, edgesGHIOParam);
            DA.SetDataList(2, nodesGHIOParam);
            //DA.SetDataList(2, graph.NetworkNodesGeometry);
            //DA.SetDataList(3, graph.NetworkFacesSimpleGeometry);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GraphFromCurve.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c24ef903-8649-42e3-99d8-d7d6e504b0b2"); }
        }
    }
}