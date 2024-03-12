using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Triangulation
{
    public class MeshNav
    {
        public Mesh Mesh => mesh;
        protected Mesh mesh;
        NetworkGraph graph;
        public MeshNav(Mesh mesh)
        {
            this.mesh = mesh;
            graph = new NetworkGraph();
            mesh.Faces.ToList().ForEach(f => graph.AddNetworkNode(
                new Point3d((
                new Point3d(mesh.Vertices[f.A])
                + new Point3d(mesh.Vertices[f.B])
                + new Point3d(mesh.Vertices[f.C])
                ) / 3
                )));
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                int[] adjs = mesh.Faces.AdjacentFaces(i);
                adjs.ToList().ForEach(j => graph.AddNetworkEdge(new NetworkEdge(graph.Graph.Vertices.ToList()[i], graph.Graph.Vertices.ToList()[j], graph, -1)));
            }
        }


        public List<FaceChain> FaceChains;

        public void SolveFaceChain(Point3d start, List<Point3d> endPoints)
        {
            var sp = new Algorithms.ShortestPath(graph);
            sp.SolveShortestPathForPoints(start, endPoints, out List<double> distances, out var edgePaths, out var crvs);

            FaceChains = new List<FaceChain>();

            foreach (var es in edgePaths)
            {
                List<MeshFace> mfs = new List<MeshFace>();
                for (int i = 0; i < es.Count; i++)
                {
                    var e = es[i];
                    if (!mfs.Contains(mesh.Faces[e.NodeA.Id])) mfs.Add(mesh.Faces[e.NodeA.Id]);
                    if (!mfs.Contains(mesh.Faces[e.NodeB.Id])) mfs.Add(mesh.Faces[e.NodeB.Id]);
                }
                
                
                Point3d spp = default;
                Point3d epp = default;

                if        (mfs[0].A != mfs[1].A && mfs[0].A != mfs[1].B && mfs[0].A != mfs[1].C && mfs[0].A != mfs[1].D)
                {
                    spp = mesh.Vertices[mfs[0].A];
                } else if (mfs[0].B != mfs[1].A && mfs[0].B != mfs[1].B && mfs[0].B != mfs[1].C && mfs[0].B != mfs[1].D)
                {
                    spp = mesh.Vertices[mfs[0].B];
                } else if (mfs[0].C != mfs[1].A && mfs[0].C != mfs[1].B && mfs[0].C != mfs[1].C && mfs[0].C != mfs[1].D)
                {
                    spp = mesh.Vertices[mfs[0].C];
                } else // (mfs[0].D != mfs[1].A && mfs[0].D != mfs[1].B && mfs[0].D != mfs[1].C && mfs[0].D != mfs[1].D)
                {
                    spp = mesh.Vertices[mfs[0].D];
                }

                if      (mfs.Last().A != mfs[mfs.Count - 2].A && mfs.Last().A != mfs[mfs.Count - 2].B && mfs.Last().A != mfs[mfs.Count - 2].C && mfs.Last().A != mfs[mfs.Count - 2].D)
                {
                    epp = mesh.Vertices[mfs.Last().A];
                }
                else if (mfs.Last().B != mfs[mfs.Count - 2].A && mfs.Last().B != mfs[mfs.Count - 2].B && mfs.Last().B != mfs[mfs.Count - 2].C && mfs.Last().B != mfs[mfs.Count - 2].D)
                {
                    epp = mesh.Vertices[mfs.Last().B];
                }
                else if (mfs.Last().C != mfs[mfs.Count - 2].A && mfs.Last().C != mfs[mfs.Count - 2].B && mfs.Last().C != mfs[mfs.Count - 2].C && mfs.Last().C != mfs[mfs.Count - 2].D)
                {
                    epp = mesh.Vertices[mfs.Last().C];
                }
                else // 
                {
                    epp = mesh.Vertices[mfs.Last().D];
                }

                FaceChain fc = new FaceChain(Mesh.Vertices, mfs, spp, epp);

                FaceChains.Add(fc);
                
            }
            return;
        }

        public List<Polyline> SolveStraightPaths()
        {
            List<Polyline> pls = new List<Polyline>();
            foreach (FaceChain fc in FaceChains)
            {
                pls.Add(new Polyline(fc.SolveFunnel()));
            }
            return pls;
        }
        
    }


    public class FaceChain
    {
        public List<MeshFace> Faces;

        public Mesh Mesh;

        public Point3d Start;

        public Point3d End;

        public List<Point3d> LeftPoints;

        

        public List<Point3d> RightPoints;

        
        public FaceChain(Rhino.Geometry.Collections.MeshVertexList vertices, List<MeshFace> faces, Point3d start, Point3d end)
        {
            this.Faces = faces;
            Start = start;
            End = end;
            Mesh = new Mesh();
            Mesh.Vertices.AddVertices(vertices);
            Mesh.Faces.AddFaces(faces);
            Mesh.DestroyTopology();
            
            Setup();
            
        }

        public List<Point3d> SolveFunnel()
        {
            shortcutPoints.Add(Start); // test
            Solve(LeftPoints, RightPoints);
            return shortcutPoints;

        }

        void Setup()
        {
            Polyline pl = Mesh.GetOutlines(Plane.WorldXY)[0];
            List<Point3d> plpts = pl.ToList();
            plpts.RemoveAt(plpts.Count - 1);
            LeftPoints = new List<Point3d>();
            RightPoints = new List<Point3d>();

            int startId = plpts.FindIndex(pt => pt.EpsilonEquals(Start, GlobalSettings.AbsoluteTolerance));
            int endId = plpts.FindIndex(pt => pt.EpsilonEquals(End, GlobalSettings.AbsoluteTolerance));

            if (startId < endId)
            {
                for (int i = startId + 1; i < endId; i++)
                {
                    RightPoints.Add(plpts[i]);
                }
                for (int i = 0; i < startId; i++)
                {
                    LeftPoints.Insert(0, plpts[i]);
                }
                int llp = LeftPoints.Count;
                for (int i = endId + 1; i < plpts.Count; i++)
                {
                    LeftPoints.Insert(llp, plpts[i]);
                }
            } else
            {
                for (int i = startId + 1; i < plpts.Count; i++)
                {
                    RightPoints.Add(plpts[i]);
                }
                for (int i = 0; i < endId; i++)
                {
                    RightPoints.Add(plpts[i]);
                }
                for (int i = endId + 1; i < startId; i++)
                {
                    LeftPoints.Insert(0, plpts[i]);
                }
            }

            RightPoints.Insert(0, plpts[startId]);
            RightPoints.Add(plpts[endId]);
            LeftPoints.Insert(0, plpts[startId]);
            LeftPoints.Add(plpts[endId]);
        }

        bool IsShortCutValid(int sid, int eid, List<Point3d> pts)
        {
            if (eid < sid)
            {
                int pid = sid;
                sid = eid;
                eid = sid;
            }
            Point3d midPt = (pts[sid] + pts[eid]) / 2;
            if (Mesh.ClosestPoint(midPt).EpsilonEquals(midPt, GlobalSettings.AbsoluteTolerance))
            {
                if (eid - sid <= 2)
                {
                    return true;
                } else
                {
                    Line intendedLine = new Line(pts[sid], pts[eid]);
                    for (int i = sid + 1; i < eid - 1; i++)
                    {
                        Line line = new Line(pts[i], pts[i + 1]);
                        Rhino.Geometry.Intersect.Intersection.LineLine(intendedLine, line, out double a, out double b, GlobalSettings.AbsoluteTolerance, true);
                        if (intendedLine.PointAt(a).EpsilonEquals(line.PointAt(b), GlobalSettings.AbsoluteTolerance))
                        {
                            return false;
                        } else
                        {
                            continue;
                        }    
                    }
                    return true;
                }

            } else
            {
                return false;
            }
        }

        List<Point3d> shortcutPoints = new List<Point3d>() ;

        bool Solve(List<Point3d> lefts, List<Point3d> rights)
        {
            if (lefts.Count <= 3)
            {
                shortcutPoints.Add(lefts.Last());
                return true;
            }
            if (rights.Count <= 3)
            {
                shortcutPoints.Add(rights.Last());
                return true;
            }
            if (IsShortCutValid(0, lefts.Count - 1, lefts))
            {
                shortcutPoints.Add(lefts.Last());
                return true;
            }
            if (IsShortCutValid(0, rights.Count - 1, rights))
            {
                shortcutPoints.Add(rights.Last());
                return true;
            }


            Point3d leftSCP = default;
            int leftSCPId = 1;
            Point3d rightSCP = default;
            int rightSCPId = 1;

            for (int i = lefts.Count - 1; i >= 0; i--)
            {
                if (IsShortCutValid(0, i, lefts))
                {
                    leftSCP = lefts[i];
                    leftSCPId = i;
                    break;
                }
            }

            for (int i = rights.Count - 1; i >= 0; i--)
            {
                if (IsShortCutValid(0, i, rights))
                {
                    rightSCP = rights[i];
                    rightSCPId = i;
                    break;
                }
            }

            if (leftSCP.DistanceTo(lefts[0]) >= rightSCP.DistanceTo(rights[0]))
            {
                shortcutPoints.Add(rightSCP);
                List<Point3d> newRights = new List<Point3d>();
                List<Point3d> newLefts = new List<Point3d>();
                for (int i = rightSCPId; i < rights.Count; i++)
                {
                    newRights.Add(rights[i]);
                }
                newLefts.Add(rightSCP);
                for (int i = leftSCPId; i < lefts.Count; i++)
                {
                    newLefts.Add(lefts[i]);
                }
                return Solve(newLefts, newRights);
            } else
            {
                shortcutPoints.Add(leftSCP);
                List<Point3d> newRights = new List<Point3d>();
                List<Point3d> newLefts = new List<Point3d>();
                for (int i = leftSCPId; i < lefts.Count; i++)
                {
                    newLefts.Add(lefts[i]);
                }
                newRights.Add(leftSCP);
                for (int i = rightSCPId; i < rights.Count; i++)
                {
                    newRights.Add(rights[i]);
                }
                return Solve(newLefts, newRights);
            }



        }
    }
}
