using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Triangulation
{
    public class StupidFunnelPathfinding
    {
        
        public static List<Point3d> FindPath(List<Line> lines)
        {
            List<Point3d> portalPts = new List<Point3d>();
            foreach (Line line in lines)
            {
                portalPts.Add(line.From);
                portalPts.Add(line.To);
            }
            return FindPath(portalPts);
        }


        public static List<Point3d> FindPath(List<Point3d> portalPts, int maxPts = 999)
        {
            List<Point3d> Points = new List<Point3d>();
            // Usage example
            double[] portals = new double[portalPts.Count * 2];
            for (int i = 0; i < portalPts.Count; i++)
            {
                portals[i * 2] = portalPts[i].X;
                portals[i * 2 + 1] = portalPts[i].Y;
            }
            int nportals = portalPts.Count / 2;


            double[] pts = new double[maxPts]; // Adjust size as needed
            // Adjust as needed
            int result = StringPull(portals, nportals, ref pts, maxPts);
            //Console.WriteLine("Number of points in path: " + result);
            //Console.WriteLine("Path points:");
            for (int i = 0; i < result; i++)
            {
                Points.Add(new Point3d(pts[i * 2], pts[i * 2 + 1], 0));
                //Console.WriteLine(pts[i * 2] + ", " + pts[i * 2 + 1]);
            }
            return Points;
        }

        static double TriArea2(double[] a, double[] b, double[] c)
        {
            double ax = b[0] - a[0];
            double ay = b[1] - a[1];
            double bx = c[0] - a[0];
            double by = c[1] - a[1];
            return bx * ay - ax * by;
        }

        static bool VEqual(double[] a, double[] b)
        {
            const double eq = 0.001f * 0.001f;
            return VDistSqr(a, b) < eq;
        }

        static double VDistSqr(double[] a, double[] b)
        {
            double dx = b[0] - a[0];
            double dy = b[1] - a[1];
            return dx * dx + dy * dy;
        }

        static void VCopy(double[] dest, double[] source)
        {
            Array.Copy(source, dest, Math.Min(dest.Length, source.Length));
        }

        static int StringPull(double[] portals, int nportals, ref double[] pts, int maxPts)
        {
            int npts = 0;
            double[] portalApex = new double[2];
            double[] portalLeft = new double[2];
            double[] portalRight = new double[2];
            int apexIndex = 0, leftIndex = 0, rightIndex = 0;
            VCopy(portalApex, new double[] { portals[0], portals[1] });
            VCopy(portalLeft, new double[] { portals[0], portals[1] });
            VCopy(portalRight, new double[] { portals[2], portals[3] });

            VCopy(pts, portalApex);
            npts++;

            for (int i = 1; i < nportals && npts < maxPts; ++i)
            {
                double[] left = { portals[i * 4], portals[i * 4 + 1] };
                double[] right = { portals[i * 4 + 2], portals[i * 4 + 3] };

                if (TriArea2(portalApex, portalRight, right) <= 0.0f)
                {
                    if (VEqual(portalApex, portalRight) || TriArea2(portalApex, portalLeft, right) > 0.0f)
                    {
                        VCopy(portalRight, right);
                        rightIndex = i;
                    }
                    else
                    {
                        pts[npts * 2] = portalLeft[0];
                        pts[npts * 2 + 1] = portalLeft[1];
                        npts++;
                        VCopy(portalApex, portalLeft);
                        apexIndex = leftIndex;
                        VCopy(portalLeft, portalApex);
                        VCopy(portalRight, portalApex);
                        leftIndex = apexIndex;
                        rightIndex = apexIndex;
                        i = apexIndex;
                        continue;
                    }
                }

                if (TriArea2(portalApex, portalLeft, left) >= 0.0f)
                {
                    if (VEqual(portalApex, portalLeft) || TriArea2(portalApex, portalRight, left) < 0.0f)
                    {
                        VCopy(portalLeft, left);
                        leftIndex = i;
                    }
                    else
                    {
                        pts[npts * 2] = portalRight[0];
                        pts[npts * 2 + 1] = portalRight[1];
                        //VCopy(pts, portalRight);
                        npts++;
                        VCopy(portalApex, portalRight);
                        apexIndex = rightIndex;
                        VCopy(portalLeft, portalApex);
                        VCopy(portalRight, portalApex);
                        leftIndex = apexIndex;
                        rightIndex = apexIndex;
                        i = apexIndex;
                        continue;
                    }
                }
            }

            if (npts < maxPts)
            {
                pts[npts * 2] = portals[(nportals - 1) * 4];
                pts[npts * 2 + 1] = portals[(nportals - 1) * 4 + 1];
                npts++;
            }

            return npts;
        }
    }
}
