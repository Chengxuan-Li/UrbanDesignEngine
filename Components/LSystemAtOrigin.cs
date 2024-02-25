using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using UrbanDesignEngine.Algorithms;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace UrbanDesignEngine
{
    public class LSystemAtOrigin : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public LSystemAtOrigin()
          : base("LSystemAtOrigin", "LSys",
              "L System Urban Modeler",
              "UrbanDesignEngine", "Models")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.
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

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;

            // If you want to change properties of certain parameters, 
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.
            pManager.AddLineParameter("Lines", "Lines", "Lines", GH_ParamAccess.list);
            pManager.AddCurveParameter("Faces", "Faces", "Faces", GH_ParamAccess.list);

            // Sometimes you want to hide a specific parameter from the Rhino preview.
            // You can use the HideParameter() method as a quick way:
            //pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            int iterations = 1; ;
            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref iterations)) return;

            double minDistance = 1;
            DA.GetData(1, ref minDistance);
            double maxDistance = 20;
            DA.GetData(2, ref maxDistance);
            double snapDistance = 5;
            DA.GetData(3, ref snapDistance);
            double minimumAngle = 2.8 / 6.0 * Math.PI;
            DA.GetData(4, ref minimumAngle);
            double maximumAngle = Math.PI;
            DA.GetData(5, ref maximumAngle);
            int numAttempt = 5;
            DA.GetData(6, ref numAttempt);
            int numPossibleGrowth = 2;
            DA.GetData(7, ref numPossibleGrowth);
            int seed = 0;
            DA.GetData(8, ref seed);
            int timeLimit = 3000;
            DA.GetData(9, ref timeLimit);


            LSystem lSystem = new LSystem(new Point3d(0, 0, 0));
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

        private Curve CreateSpiral(Plane plane, double r0, double r1, Int32 turns)
        {
            Line l0 = new Line(plane.Origin + r0 * plane.XAxis, plane.Origin + r1 * plane.XAxis);
            Line l1 = new Line(plane.Origin - r0 * plane.XAxis, plane.Origin - r1 * plane.XAxis);

            Point3d[] p0;
            Point3d[] p1;

            l0.ToNurbsCurve().DivideByCount(turns, true, out p0);
            l1.ToNurbsCurve().DivideByCount(turns, true, out p1);

            PolyCurve spiral = new PolyCurve();

            for (int i = 0; i < p0.Length - 1; i++)
            {
                Arc arc0 = new Arc(p0[i], plane.YAxis, p1[i + 1]);
                Arc arc1 = new Arc(p1[i + 1], -plane.YAxis, p0[i + 1]);

                spiral.Append(arc0);
                spiral.Append(arc1);
            }

            return spiral;
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5d64c17e-1879-48e6-8f7c-3c1a44ab0448"); }
        }
    }
}
