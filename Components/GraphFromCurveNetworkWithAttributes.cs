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
            pManager.AddScriptVariableParameter("UDECurveReferences", "CrvRefs", "UDECurveReferences", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("UDEGraph", "G", "Graph generated from the network of curves", GH_ParamAccess.item);
            pManager.AddGenericParameter("GraphConnections", "GCs", "Graph connections", GH_ParamAccess.list);
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

            ScriptVariableGetter.GetScriptVariableList<ReferenceCurveGeometry>(this, DA, 0, true, out List<ReferenceCurveGeometry> svs);

            List<Attributes> attributesList = new List<Attributes>();
            svs.ForEach(s => curves.Add(s.PreviewGeometry));
            svs.ForEach(s => attributesList.Add(s.AttributesInstance));
            

            NetworkGraph graph = NetworkCurvesIntersection.NetworkGraphFromCurves(curves, attributesList);
            graph.SolveFaces();
            DA.SetData(0, graph.GHIOParam);
            List<GHIOCurveParam<NetworkEdge>> edgesGHIOParam = new List<GHIOCurveParam<NetworkEdge>>();
            List<GHIOPointParam<NetworkNode>> nodesGHIOParam = new List<GHIOPointParam<NetworkNode>>();
            graph.Graph.Edges.ToList().ForEach(e => edgesGHIOParam.Add(e.gHIOParam));
            graph.Graph.Vertices.ToList().ForEach(v => nodesGHIOParam.Add(v.gHIOParam));
            DA.SetDataList(1, edgesGHIOParam);
            DA.SetDataList(2, nodesGHIOParam);
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