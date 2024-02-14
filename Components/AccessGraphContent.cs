using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.IO;

namespace UrbanDesignEngine.Components
{
    public class AccessGraphContent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AccessGraphContent class.
        /// </summary>
        public AccessGraphContent()
          : base("AccessGraphContent", "AGC",
              "Access the content of a graph, including its nodes (vertices), edges, faces, topology/connectivity etc.",
              "UrbanDesignEngine", "Graph")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("UDEGraph", "G", "UDEGraph type variable representing the graph of which the content is to be accessed", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nodes", "N", "Nodes (vertices) of the graph", GH_ParamAccess.list);
            pManager.AddLineParameter("Edges", "E", "Edges of the graph, in simple lines denoting connectivity", GH_ParamAccess.list);
            pManager.AddCurveParameter("EdgeCurves", "EC", "Corresponding curves for each edge, resulting from the actual underlying geometry for each connection", GH_ParamAccess.list);
            pManager.AddCurveParameter("Faces", "F", "Faces of the graph, in simple enclosed polyline curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("FaceCurves", "FC", "Corresponding curves for each face, resulting from the actual underlying geometry for each face loop", GH_ParamAccess.list);
            pManager.AddIntegerParameter("NodeAdjacentNode", "NAN", "Adjacent node of each node", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("NodeAdjacentEdge", "NAE", "Adjacent edge of each node", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("NodeAdjacentFace", "NAF", "Adjacent face of each node", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("EdgeSourceNode", "ESN", "Source node of each edge", GH_ParamAccess.list);
            pManager.AddIntegerParameter("EdgeTargetNode", "ETN", "Target node of each edge", GH_ParamAccess.list);
            pManager.AddIntegerParameter("EdgeLeftFace", "ELF", "Left face of each edge", GH_ParamAccess.list);
            pManager.AddIntegerParameter("EdgeRightFace", "ELF", "Right face of each edge", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!(ScriptVariableGetter<NetworkGraph>.GetScriptVariable(this, DA, 0, true, out NetworkGraph graph) == VariableGetterStatus.Success)) return;
            //graph.NetworkFaces.RemoveAll(f => !f.IsAntiClockWise); // temporary TODO
            DA.SetDataList(0, graph.NetworkNodesGeometry);
            DA.SetDataList(1, graph.NetworkEdgesSimpleGeometry);
            DA.SetDataList(2, graph.NetworkEdgeUnderglyingGeometry);
            if(graph.SolvedPlanarFaces)
            {
                DA.SetDataList(3, graph.NetworkFacesSimpleGeometry);
                DA.SetDataList(4, graph.NetworkFacesUnderlyingGeometry);
                DA.SetDataList(10, graph.EdgesLeftFaces);
                DA.SetDataList(11, graph.EdgesRightFaces);
            } else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Warning: dual graph not resolved - no face topology exists");   
            }

            DA.SetDataTree(5, graph.NodesAdjacentNodes);
            DA.SetDataTree(6, graph.NodesAdjacentEdges);
            DA.SetDataTree(7, graph.NodesAdjacentFaces);
            DA.SetDataList(8, graph.EdgesSourceNodes);
            DA.SetDataList(9, graph.EdgesTargetNodes);

            
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
            get { return new Guid("b28a13aa-1508-4906-80fc-aa3fa43e7c7f"); }
        }
    }
}