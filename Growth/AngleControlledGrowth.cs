﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.Utilities;
using Rhino.Geometry;

namespace UrbanDesignEngine.Growth
{
    public class AngleControlledGrowth
    {
        public NetworkNode Node;
        public List<double> Angles = new List<double>();
        public double MinimumAngle;
        public double MaximumAngle;
        public Random random = new Random();

        public AngleControlledGrowth(NetworkNode node, double minimumAngle, double maximumAngle)
        {
            Node = node;
            MinimumAngle = minimumAngle;
            MaximumAngle = maximumAngle;
            
        }

        public bool Next(double distance, out Point3d result)
        {
            
            double nextAngle = random.NextDouble() * 2 * Math.PI;
            bool valid = true;
            Angles = new List<double>();
            Node.Graph.Graph.AdjacentVertices(Node).ToList().ForEach(n => Angles.Add(Trigonometry.Angle(Node, n)));



            foreach (double angle in Angles)
            {
                if (Trigonometry.AngleDifference(angle, nextAngle) < MinimumAngle)
                {
                    valid = false;
                    break;
                }
            }

            


            if (Angles.Count > 0 && Angles.Min<double, double>(x => Trigonometry.AngleDifference(x, nextAngle)) > MaximumAngle)
            {
                valid = false;
            }

            result = new Point3d(
                    Node.Point.X + distance * Math.Cos(nextAngle),
                    Node.Point.Y + distance * Math.Sin(nextAngle),
                    0);
            return valid;
        }
    }
}
