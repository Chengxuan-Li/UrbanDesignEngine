using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.IO;

namespace UrbanDesignEngine.Components
{
    public class LSystemFromGraph : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LSystemFromGraph class.
        /// </summary>
        public LSystemFromGraph()
          : base("LSystemFromGraph", "LSys",
              "L System Urban Modeler",
              "UrbanDesignEngine", "Models")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddScriptVariableParameter("UDEGraph", "UDEGraph", "UDEGraph", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Iterations", "Iterations", "Iterations", GH_ParamAccess.item);
            pManager.AddNumberParameter("MinDistance", "MinDist", "Minimum Distance", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaxDistance", "MaxDist", "Maximum Distance", GH_ParamAccess.item);
            pManager.AddNumberParameter("SnapDistance", "SnapDist", "Snap Distance", GH_ParamAccess.item);
            pManager.AddNumberParameter("MinimumAngle", "MinAngle", "Minimum Angle", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaximumAngle", "MaxAngle", "Maximum Angle", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumGenAttempt", "NumGenAttempt", "Number of attempts for generation", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumPossibleGrowths", "NumPossibleGrowths", "NumPossibleGrowths", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Seed", "Seed", "Seed", GH_ParamAccess.item);
            pManager.AddIntegerParameter("CalculationTimeLimit", "CalcTimeLim", "Calculation Time Limit in Milliseconds", GH_ParamAccess.item);

            
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;
            pManager[10].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "Lines", "Lines", GH_ParamAccess.list);
            pManager.AddCurveParameter("Faces", "Faces", "Faces", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (ScriptVariableGetter.GetScriptVariable<NetworkGraph>(this, DA, 0, true, out NetworkGraph graph) != VariableGetterStatus.Success) return;

            int iterations = 1; ;
            if (!DA.GetData(1, ref iterations)) return;
            double minDistance = 1;
            DA.GetData(2, ref minDistance);
            double maxDistance = 20;
            DA.GetData(3, ref maxDistance);
            double snapDistance = 5;
            DA.GetData(4, ref snapDistance);
            double minimumAngle = 2.8 / 6.0 * Math.PI;
            DA.GetData(5, ref minimumAngle);
            double maximumAngle = Math.PI;
            DA.GetData(6, ref maximumAngle);
            int numAttempt = 5;
            DA.GetData(7, ref numAttempt);
            int numPossibleGrowth = 2;
            DA.GetData(8, ref numPossibleGrowth);
            int seed = 0;
            DA.GetData(9, ref seed);
            int timeLimit = 3000;
            DA.GetData(10, ref timeLimit);

            LSystem lSystem = new LSystem(graph.Duplicate());
            lSystem.Iterations = iterations;
            lSystem.MinDistance = minDistance;
            lSystem.MaxDistance = maxDistance;
            lSystem.SnapDistance = snapDistance;
            lSystem.MinimumAngle = minimumAngle;
            lSystem.MaximumAngle = maximumAngle;
            lSystem.NumAttempt = numAttempt;
            lSystem.NumPossibleGrowth = numPossibleGrowth;
            lSystem.SetSeed(seed);
            lSystem.CalculationTimeLimit = timeLimit;

            lSystem.Solve();
            List<Line> lines = new List<Line>();
            lSystem.Graph.Graph.Edges.ToList().ForEach(e => lines.Add(new Line(e.Source.Point, e.Target.Point)));

            // Finally assign the spiral to the output parameter.
            DA.SetDataList(0, lines);
            //DA.SetDataList(1, lSystem.FaceCurves);

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
            get { return new Guid("6d5f239f-cd06-4147-b06b-9c23932a8bd0"); }
        }
    }
}