using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Utilities
{
    public static class Trigonometry
    {
        public static double Angle(NetworkNode source, NetworkNode target)
        {
            double angle = Math.Atan2(target.Point.Y - source.Point.Y, target.Point.X - source.Point.X);
            if (angle < 0)
            {
                angle = angle + Math.PI * 2;
            }
            return angle;
        }

        public static double Angle(double angleA, double angleB)
        {
            angleA = angleA % (2 * Math.PI);
            angleB = angleB % (2 * Math.PI);
            double difference = Math.Abs(angleA - angleB);
            return (difference > Math.PI) ? 2 * Math.PI - difference : difference;
        }
    }
}
