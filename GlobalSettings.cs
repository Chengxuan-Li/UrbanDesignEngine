using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace UrbanDesignEngine
{
    public static class GlobalSettings
    {
        public static double AbsoluteTolerance = 0.0001;
        public static string SCPrefix = "UDE";
        public static double DefaultOffsetDitance = 4.0;
        public static double DefaultOffsetExtensionDistance = 300;
    }

    public static class PreviewSettings
    {
        public static Color PreviewColor = Color.OrangeRed;
        public static int Thickness = 3;
        public static int PointRadius = 4;
    }

    public static class TensorFieldSettings
    {
        public static double EvaluationNeighbourDistance = 0.1;
        public static double EvaluationNeighbourWeight = 0.25;
    }
}
