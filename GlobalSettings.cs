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
    }

    public static class PreviewSettings
    {
        public static Color PreviewColor = Color.OrangeRed;
        public static int Thickness = 3;
    }
}
