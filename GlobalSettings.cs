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
        public static Color GraphPreviewColor = Color.OrangeRed;
        public static Color TensorFieldPreviewColor = Color.MidnightBlue;
        public static int Thickness = 3;
        public static double ArrowSize = 1;
        public static int PointRadius = 4;
    }

    public static class TensorFieldSettings
    {
        public static double EvaluationNeighbourDistance = 0.1;
        public static double EvaluationNeighbourWeight = 0.25;
        public static double PreviewGeometryInterval = 25.0;
    }
}
