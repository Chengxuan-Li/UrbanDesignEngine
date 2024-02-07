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
        /// <summary>
        /// Azimuth angle from source to target. Resultant angle value is between 0 (inclusive) and 2*pi (exclusive)
        /// </summary>
        /// <param name="source">From node</param>
        /// <param name="target">To node</param>
        /// <returns>Azimuth angle from source to target</returns>
        public static double Angle(NetworkNode source, NetworkNode target)
        {
            double angle = Math.Atan2(target.Point.Y - source.Point.Y, target.Point.X - source.Point.X);
            if (angle < 0)
            {
                angle = angle + Math.PI * 2;
            }
            return angle;
        }

        /// <summary>
        /// Angle difference between 2 angles
        /// </summary>
        /// <param name="angleA"></param>
        /// <param name="angleB"></param>
        /// <returns></returns>
        public static double AngleDifference(double angleA, double angleB)
        {
            angleA = angleA % (2 * Math.PI);
            angleB = angleB % (2 * Math.PI);
            double difference = Math.Abs(angleA - angleB);
            return (difference > Math.PI) ? 2 * Math.PI - difference : difference;
        }
    }
}
