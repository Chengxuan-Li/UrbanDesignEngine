using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using UrbanDesignEngine.Algorithms;
using UrbanDesignEngine.Components;
using UrbanDesignEngine.Constraints;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.Growth;
using UrbanDesignEngine.IO;
using UrbanDesignEngine.Maths;
using UrbanDesignEngine.Utilities;
using UrbanDesignEngine.Learning;

namespace UrbanDesignEngine
{
    public class TessellationParameters
    {
        public static TessellationParameters DefaultParameters => new TessellationParameters();

        public int MaxIterations = 3;
        public double IntersectOneLineMaxDistance = 50;
        public double MinArea = 100;
        public double MaxArea = 800;
        public double MinAngleDiff = Math.PI / 6.0;
        public double MinGap = 8;
        public double MaxGap = 40;
        public double SnapDistance = 5;
    }
    public class TessellationTest
    {
        public Polyline BoundaryLine;
        public List<double> PrincipleAngles;
        public List<double> PrincipleWeights;
        public List<TessellationTest> Divisions = new List<TessellationTest>();
        public TessellationParameters TessellationParameters = TessellationParameters.DefaultParameters;

        public bool Finalised
        {
            get
            {
                AreaMassProperties amp = AreaMassProperties.Compute(BoundaryLine.ToNurbsCurve());
                // HIJ
                if (amp == null)
                {
                    return false;
                }
                if (amp.Area > TessellationParameters.MaxArea || amp.Area < TessellationParameters.MinArea) // ?
                {
                    return false;
                } else
                {
                    return true;
                }    
                // DD
                double minDist = 8;
                for (int i = 0; i < BoundaryLine.Count - 1; i++)
                {
                    for (int j = i; j < BoundaryLine.Count - 1; j++)
                    {
                        if (i != j)
                        {
                            if (BoundaryLine[i].DistanceTo(BoundaryLine[j]) < minDist)
                            {
                                return false;
                            }
                        }

                    }
                }
                return true;
            }
        }

        public TessellationTest(Polyline pl, List<TessellationTest> divisions)
        {
            BoundaryLine = pl;
            GeometryHelper.LinesPrincipleDirections(pl.GetSegments().ToList(), out PrincipleAngles, out PrincipleWeights);
            //Divisions = divisions; // this is a reference

        }

        public enum SolutionStat
        {
            finished = 0,
            failed = 1,
            continu = 2,
        }

        public SolutionStat Solve()
        {
            if (Finalised)
            {
                Divisions.Add(this);
                return SolutionStat.finished;
            }
            int maxIterations = TessellationParameters.MaxIterations;
            int iterations = 0;
            Random random = new Random();
            while (iterations < maxIterations)
            {
                double p;
                double p2;
                RandomPositionParameters(out p, out p2);
                Vector3d direction;
                bool angleGenerationResult = IncisionDirection(p, out direction);
                if (!angleGenerationResult) return SolutionStat.failed;
                double maxD = IncisionLineMaxLength(p, direction);
                if (maxD <= TessellationParameters.IntersectOneLineMaxDistance && maxD > 0)
                {
                    TessellationTest tt1;
                    TessellationTest tt2;
                    Intersection1Divide(IncisionLine(p, direction, maxD), out tt1, out tt2);
                    if (tt1.Solve() == SolutionStat.finished)
                    {
                        Divisions.AddRange(tt1.Divisions);
                        SolutionStat result = tt2.Solve();
                        if (result == SolutionStat.finished)
                        {
                            Divisions.AddRange(tt2.Divisions);
                            return SolutionStat.finished;
                        } else
                        {
                            return result;
                        }
                    } else if (tt2.Solve() == SolutionStat.finished)
                    {
                        Divisions.AddRange(tt2.Divisions);
                        SolutionStat result = tt1.Solve();
                        if (result == SolutionStat.finished)
                        {
                            Divisions.AddRange(tt1.Divisions);
                            return SolutionStat.finished;
                        }
                        else
                        {
                            return result;
                        }
                    }
                    else
                    {
                        return SolutionStat.failed;
                    }
                } else
                {
                    
                    Vector3d direction2;
                    angleGenerationResult = IncisionDirection(p2, out direction2);
                    if (!angleGenerationResult) return SolutionStat.failed;
                    double maxD2 = IncisionLineMaxLength(p2, direction2);
                    TessellationTest tt1;
                    TessellationTest tt2;
                    List<Line> dv;
                    if (Intersection2Divide(IncisionLine(p, direction, maxD), IncisionLine(p2, direction2, maxD2), out tt1, out tt2, out dv))
                    {
                        if (tt1.Solve() == SolutionStat.finished)
                        {
                            Divisions.AddRange(tt1.Divisions);
                            SolutionStat result = tt2.Solve();
                            if (result == SolutionStat.finished)
                            {
                                Divisions.AddRange(tt2.Divisions);
                                return SolutionStat.finished;
                            }
                            else
                            {
                                return result;
                            }
                        }
                        else if (tt2.Solve() == SolutionStat.finished)
                        {
                            Divisions.AddRange(tt2.Divisions);
                            SolutionStat result = tt1.Solve();
                            if (result == SolutionStat.finished)
                            {
                                Divisions.AddRange(tt1.Divisions);
                                return SolutionStat.finished;
                            }
                            else
                            {
                                return result;
                            }
                        }
                        else
                        {
                            return SolutionStat.failed;
                        }
                    }
                }

                iterations++;
            }
            return SolutionStat.failed;

        }

        public void RandomPositionParameters(out double lengthParameter1, out double lengthParameter2)
        {
            Random random = new Random();
            List<double> paramsToAvoid = ProjectedLengthsCumulated.ToList();
            lengthParameter1 = RandomPositionParametersWithGap(paramsToAvoid);
            paramsToAvoid.Add(lengthParameter1);
            lengthParameter2 = RandomPositionParametersWithGap(paramsToAvoid);
        }

        public double RandomPositionParametersWithGap(List<double> parametersToAvoid)
        {
            Random random = new Random();
            parametersToAvoid.Sort();
            List<double> intervalMins = new List<double>();
            List<double> intervalMaxes = new List<double>();
            List<double> intervalVals = new List<double>();
            for (int i = 0; i < parametersToAvoid.Count - 1; i++)
            {
                double interval = parametersToAvoid[i + 1] - parametersToAvoid[i];
                if (interval <= 2 * TessellationParameters.MinGap)
                {
                    intervalMins.Add(parametersToAvoid[i]);
                    intervalMaxes.Add(parametersToAvoid[i + 1]);
                    intervalVals.Add(random.NextDouble() >= 0.5 ? parametersToAvoid[i + 1] : parametersToAvoid[i]);
                } else if (interval <= 2 * TessellationParameters.MaxGap)
                {
                    intervalMins.Add(parametersToAvoid[i]);
                    intervalMaxes.Add(parametersToAvoid[i + 1]);
                    intervalVals.Add(random.NextDouble() * (parametersToAvoid[i + 1] - parametersToAvoid[i] - 2 * TessellationParameters.MinGap) + parametersToAvoid[i] + TessellationParameters.MinGap);
                } else
                {
                    intervalMins.Add(parametersToAvoid[i]);
                    intervalMaxes.Add(parametersToAvoid[i + 1]);
                    double b = random.NextDouble() > 0.5 ? parametersToAvoid[i] + TessellationParameters.MinGap : parametersToAvoid[i + 1] - TessellationParameters.MaxGap;
                    intervalVals.Add(random.NextDouble() * (TessellationParameters.MaxGap - TessellationParameters.MinGap) + b);
                }
            }
            double p = random.NextDouble() * ProjectedLengthSum;
            double v = default;
            for (int i = 0; i < intervalMins.Count; i++)
            {
                if (p <= intervalMaxes[i] && p >= intervalMins[i])
                {
                    v = intervalVals[i];
                }
            }

            return v;
        }

        public List<Line> PlotsBoundarySegments => BoundaryLine.GetSegments().ToList();

        public List<double> ProjectedLengths
        {
            get
            {
                List<double> lengths = new List<double>();
                //Vector3d vec = new Vector3d(LengthProjectionVector);
                foreach (Line line in PlotsBoundarySegments)
                {
                    //double angle = Vector3d.VectorAngle(vec, new Vector3d(-line.From + line.To));
                    lengths.Add(line.Length * 1.0); // Math.Cos(angle)
                }
                return lengths;
            }
        }

        public double ProjectedLengthSum => ProjectedLengths.Sum();

        public List<double> ProjectedLengthsCumulated
        {
            get
            {
                List<double> lengths = new List<double>();
                double sum = 0;
                foreach (double length in ProjectedLengths)
                {
                    lengths.Add(length + sum);
                    sum = sum + length;
                }
                return lengths;
            }
        }

        public Point3d LengthToPoint(double lengthParameter)
        {
            int index = ProjectedLengthsCumulated.FindIndex(x => x >= lengthParameter);
            double diff = lengthParameter - (ProjectedLengthsCumulated[index] - ProjectedLengths[index]);
            Vector3d direction = PlotsBoundarySegments[index].Direction;
            direction.Unitize();
            return PlotsBoundarySegments[index].From + direction * diff;
        }

        public int LengthToSegment(double lengthParameter)
        {
            return ProjectedLengthsCumulated.FindIndex(x => x >= lengthParameter);
        }

        public bool IncisionDirection(double lengthParameter, out Vector3d direction)
        {
            direction = default;
            
            double avoidAngleDiff = TessellationParameters.MinAngleDiff;
            int index = ProjectedLengthsCumulated.FindIndex(x => x >= lengthParameter);
            double diff = lengthParameter - (ProjectedLengthsCumulated[index] - ProjectedLengths[index]);
            List<double> anglesToAvoid = new List<double>();
            double currentDirection = Trigonometry.Angle(PlotsBoundarySegments[index].From, PlotsBoundarySegments[index].To);
            if (diff <= GlobalSettings.AbsoluteTolerance)
            {
                // point is on the starting end of the segment - the direction of this and the prev segment should be avoided
                anglesToAvoid.Add(currentDirection);
                //anglesToAvoid.Add(currentDirection < Math.PI ? currentDirection + Math.PI : currentDirection - Math.PI);
                if (index >= 1) // check if this is NOT the first segment
                {
                    var ang = Trigonometry.Angle(PlotsBoundarySegments[index - 1].From, PlotsBoundarySegments[index - 1].To);
                    //anglesToAvoid.Add(ang);
                    anglesToAvoid.Add(ang < Math.PI ? ang + Math.PI : ang - Math.PI);
                } // TODO add treatment if this is the first segment
            }
            else if ((ProjectedLengthsCumulated[index] - lengthParameter) <= GlobalSettings.AbsoluteTolerance)
            { // point is on the end of the segment - the direction of this and the next segment should be avoided
                //anglesToAvoid.Add(currentDirection);
                anglesToAvoid.Add(currentDirection < Math.PI ? currentDirection + Math.PI : currentDirection - Math.PI);
                if (index < PlotsBoundarySegments.Count - 1)
                {
                    var ang = Trigonometry.Angle(PlotsBoundarySegments[index + 1].From, PlotsBoundarySegments[index + 1].To);
                    anglesToAvoid.Add(ang);
                    //anglesToAvoid.Add(ang < Math.PI ? ang + Math.PI : ang - Math.PI);
                } // TODO add treatement if this is the last segment
            } else
            {
                anglesToAvoid.Add(currentDirection);
                anglesToAvoid.Add(currentDirection < Math.PI ? currentDirection + Math.PI : currentDirection - Math.PI);
            }

            List<double> pAngles = PrincipleAngles.ToList();
            List<double> pWeights = PrincipleWeights.ToList(); // !!!

            for (int i = 0; i < pAngles.Count; i++)
            {
                foreach (double angleToAvoid in anglesToAvoid)
                {
                    if (Math.Abs(pAngles[i] - angleToAvoid) <= avoidAngleDiff)
                    {
                        pWeights[i] = 0.0;
                    }
                }
            }
            double resultAngle = currentDirection;
            if (pWeights.Sum() <= GlobalSettings.AbsoluteTolerance)
            {
                resultAngle = (currentDirection < Math.PI * 1.5) ? currentDirection + Math.PI / 2 : currentDirection - 1.5 * Math.PI;
                direction = new Vector3d(Math.Cos(resultAngle), Math.Sin(resultAngle), 0);
                return false;
            }
            var wrpk = Maths.MathsHelper.WeightedRandomPick<double>(pAngles, pWeights);
            int trylefttimes = 0;
            while (trylefttimes < 50 && Trigonometry.OnLeftSideOf(currentDirection, resultAngle) != 1) // handle the condition wher
            {
                resultAngle = wrpk.Invoke();
                trylefttimes += 1;
            }
            direction = new Vector3d(Math.Cos(resultAngle), Math.Sin(resultAngle), 0);
            return true;

        }

        public int OnSegmentId(Point3d pt)
        {
            for (int i = 0; i < PlotsBoundarySegments.Count; i++)
            {
                if (PlotsBoundarySegments[i].DistanceTo(pt, true) < GlobalSettings.AbsoluteTolerance) return i;
            }
            return -1;
        }

        public Line IncisionLine(double lengthParameter, Vector3d direction, double length)
        {
            return new Line(LengthToPoint(lengthParameter), LengthToPoint(lengthParameter) + direction * length);
        }

        public double IncisionLineMaxLength(double lengthParameter, Vector3d direction)
        {
            Line maximumLine = new Line(LengthToPoint(lengthParameter), LengthToPoint(lengthParameter) + direction * 99999);
            List<double> parameters;
            if (IntersectComponents(maximumLine, out var _, out parameters) >= 2)
            {
                return maximumLine.PointAt(parameters[1]).DistanceTo(maximumLine.From);
            }
            return -1;
        }

        public int IntersectComponents(Line line, out List<int> segsId, out List<double> paramsSorted)
        {
            List<int> intSegs = new List<int>();
            List<double> intParams = new List<double>();

            for (int j = 0; j < PlotsBoundarySegments.Count; j++)
            {
                Line segment = PlotsBoundarySegments[j];
                double a;
                bool intersecting = Rhino.Geometry.Intersect.Intersection.LineLine(line, segment, out a, out _, GlobalSettings.AbsoluteTolerance, true);
                if (intersecting)
                {
                    intParams.Add(a);
                    intSegs.Add(j);
                }
            }

            List<int> segsSorted;
            DataManagement.SortByKeys<double, int>(intParams, intSegs, out paramsSorted, out segsSorted);
            segsId = segsSorted;
            return intParams.Count;
        }

        

        public bool Intersection2Divide(Line lineA, Line lineB, out TessellationTest tb1, out TessellationTest tb2, out List<Line> dv)
        {
            tb1 = default;
            tb2 = default;
            dv = default;
            double a;
            double b;
            bool result = Rhino.Geometry.Intersect.Intersection.LineLine(lineA, lineB, out a, out b, GlobalSettings.AbsoluteTolerance, true);
            if (!result)
            {
                return false;
            }
            dv = new List<Line>() { new Line(lineA.From, lineA.PointAt(a)), new Line(lineA.PointAt(a), lineB.From) };
            int fromId = OnSegmentId(BoundaryLine.ClosestPoint(lineA.From));
            int toId = OnSegmentId(BoundaryLine.ClosestPoint(lineB.From));
            List<Point3d> ptsA = new List<Point3d>();
            List<Point3d> ptsB = new List<Point3d>();

            for (int i = 0; i < BoundaryLine.Count; i++)
            {
                if (i < (fromId < toId ? fromId : toId))
                {
                    ptsA.Add(BoundaryLine[i]);
                }
                else if (i == (fromId < toId ? fromId : toId))
                {
                    ptsA.Add(BoundaryLine[i]);
                    ptsA.Add(BoundaryLine.ClosestPoint(fromId < toId ? lineA.From : lineB.From));
                    ptsA.Add(lineA.PointAt(a));
                    ptsA.Add(BoundaryLine.ClosestPoint(fromId < toId ? lineB.From : lineA.From));

                    ptsB.Add(BoundaryLine.ClosestPoint(fromId < toId ? lineB.From : lineA.From));
                    ptsB.Add(lineA.PointAt(a));
                    ptsB.Add(BoundaryLine.ClosestPoint(fromId < toId ? lineA.From : lineB.From));
                }
                else if (i < (fromId < toId ? toId : fromId))
                {
                    ptsB.Add(BoundaryLine[i]);
                }
                else if (i == (fromId < toId ? toId : fromId))
                {
                    ptsB.Add(BoundaryLine[i]);
                    ptsB.Add(BoundaryLine.ClosestPoint(fromId < toId ? lineB.From : lineA.From));
                }
                else
                {
                    ptsA.Add(BoundaryLine[i]);
                }
            }

            tb1 = new TessellationTest(new Polyline(ptsA), Divisions);
            tb2 = new TessellationTest(new Polyline(ptsB), Divisions);
            return true;
        }

        public void Intersection1Divide(Line line, out TessellationTest tb1, out TessellationTest tb2)
        {
            tb1 = default;
            tb2 = default;


            int fromId = OnSegmentId(BoundaryLine.ClosestPoint(line.From));
            int toId = OnSegmentId(BoundaryLine.ClosestPoint(line.To));
            List<Point3d> ptsA = new List<Point3d>();
            List<Point3d> ptsB = new List<Point3d>();
            for (int i = 0; i < BoundaryLine.Count; i++)
            {
                if (i < (fromId < toId ? fromId : toId))
                {
                    ptsA.Add(BoundaryLine[i]);
                } else if (i == (fromId < toId ? fromId : toId))
                {
                    ptsA.Add(BoundaryLine[i]);
                    ptsA.Add(BoundaryLine.ClosestPoint(fromId < toId ? line.From : line.To));
                    ptsA.Add(BoundaryLine.ClosestPoint(fromId < toId ? line.To :  line.From));

                    ptsB.Add(BoundaryLine.ClosestPoint(fromId < toId ? line.To : line.From));
                    ptsB.Add(BoundaryLine.ClosestPoint(fromId < toId ? line.From : line.To));
                } else if (i < (fromId < toId ? toId : fromId))
                {
                    ptsB.Add(BoundaryLine[i]);
                } else if (i == (fromId < toId ? toId : fromId))
                {
                    ptsB.Add(BoundaryLine[i]);
                    ptsB.Add(BoundaryLine.ClosestPoint(fromId < toId ? line.To : line.From));
                } else
                {
                    ptsA.Add(BoundaryLine[i]);
                }
            }

            tb1 = new TessellationTest(new Polyline(ptsA), Divisions);
            tb2 = new TessellationTest(new Polyline(ptsB), Divisions);
        }

    }


    public class TessellateBlockSolution
    {

        public TessellateBlockSolution(List<Polyline> pls)
        {
            TessellateBlock tsb = new TessellateBlock(pls);
            

        }
    }



    /// <summary>
    /// The generic type input for SolutionStatus to solve;
    /// this represents an intermediate state of block tessellation,
    /// including the initial state.
    /// The idea of tessellation is as follows:
    /// 1.  Select points on the boundary, subject to conditions such as
    ///     Mininum gap (~10m)
    ///     Maximum gap (30+m)
    /// 2.  Draw lines following the principle directions determined by
    /// the original input of the block, avoiding overlap lines
    /// 3.  Solve for the occasions where lines in step (2) do intersect,
    /// generating plots of appropriate size, either:
    ///     a)  results in an appropriate block + a larger block, latter
    ///         to be subdivided
    ///     b)  results in two larger blocks of appropriate shape and size
    ///         both of which are to be subdivided later
    /// 4.  Loop until all divided areas having appropriate shape and size
    /// </summary>
    public class TessellateBlock : ISolvable<TessellateBlock>
    {
        #region InputParamsHolder
        public List<PlotsBoundarySide> OriginalSides = new List<PlotsBoundarySide>();
        public List<double> PrincipleAngles = new List<double>();
        public List<double> PrincipileAngleWeights = new List<double>();

        public TessellateBlock(List<Polyline> sides)
        {
            List<Line> lines = new List<Line>();
            foreach (Polyline side in sides)
            {
                PlotsBoundarySide pside = new PlotsBoundarySide(side);
                pside.ParentBlock = this;
                OriginalSides.Add(pside);
                lines.AddRange(side.GetSegments());
            }
            GeometryHelper.LinesPrincipleDirections(lines, out PrincipleAngles, out PrincipileAngleWeights);
        }
        #endregion

        public int IntersectComponents(Line line, out List<int> sidesId, out List<int> segsId, out List<double> paramsSorted)
        {
            List<int> intSides = new List<int>();
            List<int> intSegs = new List<int>();
            List<double> intParams = new List<double>();
            for (int i = 0; i < RuntimeSides.Count; i++)
            {
                PlotsBoundarySide side = RuntimeSides[i];
                for (int j = 0; j < side.PlotsBoundarySegments.Count; j++)
                {
                    Line segment = side.PlotsBoundarySegments[j];
                    double a;
                    bool intersecting = Rhino.Geometry.Intersect.Intersection.LineLine(line, segment, out a, out _, GlobalSettings.AbsoluteTolerance, true);
                    if (intersecting)
                    {
                        intParams.Add(a);
                        intSides.Add(i);
                        intSegs.Add(j);
                    }
                }
            }

            List<int> sidesSorted;
            List<int> segsSorted;
            DataManagement.SortByKeys<double, int>(intParams, intSides, out paramsSorted, out sidesSorted);
            DataManagement.SortByKeys<double, int>(intParams, intSegs, out paramsSorted, out segsSorted);
            sidesId = sidesSorted;
            segsId = segsSorted;
            return intParams.Count;
        }







        #region SolvableParameters
        public List<PlotsBoundarySide> RuntimeSides = new List<PlotsBoundarySide>();


        #endregion

        #region PredicatesActionsFuncs
        public static Action<TessellateBlock> P1Propagate_GenBreakParam => tb =>
        {
            tb.RuntimeSides.ForEach(s => PlotsBoundarySide.GenerateBreakParameter.Invoke(s));
        };

        public static Predicate<TessellateBlock> P1Runtime_MinGap => tb =>
        {
            foreach (var side in tb.RuntimeSides)
            {
                if (!PlotsBoundarySide.BPRuntimeMinimumGap(side))
                {
                    return false;
                }
            }
            return true;
        };

        public static Predicate<TessellateBlock> P1Final_MaxGap => tb =>
        {
            foreach (var side in tb.RuntimeSides)
            {
                if (!PlotsBoundarySide.BPFinal(side))
                {
                    return false;
                }
            }
            return true;
        };

        public static Action<TessellateBlock> P2Propagate_GenLineDivide=> tb =>
        {

        };

        public static Predicate<TessellateBlock> P2Runtime_DivGeoValidity => tb =>
        {
            return false; // TODO
        };

        public static Predicate<TessellateBlock> P2Final_DivAreaValidity => tb =>
        {
            return false; // TODO
        };

        public static Predicate<TessellateBlock> PFFinal_AllAreaQualified => tb =>
        {
            return false; // TODO
        };

        #endregion

        #region ISolvable
        public TessellateBlock Duplicate()
        {
            throw new NotImplementedException();
        }

        public TessellateBlock StateParametersInitialise()
        {
            throw new NotImplementedException();
        }
        #endregion
    }


    public class PlotsBoundarySide
    {
        public Polyline BoundaryLine;
        public Vector3d LengthProjectionVector => new Vector3d(-BoundaryLine.First + BoundaryLine.Last);
        public TessellateBlock ParentBlock;
        
        public PlotsBoundarySide(Polyline sidePl)
        {
            BoundaryLine = sidePl;
        }

        public List<Line> PlotsBoundarySegments => BoundaryLine.GetSegments().ToList();

        public List<double> ProjectedLengths
        {
            get
            {
                List<double> lengths = new List<double>();
                Vector3d vec = new Vector3d(LengthProjectionVector);
                foreach(Line line in PlotsBoundarySegments)
                {
                    double angle = Vector3d.VectorAngle(vec, new Vector3d(-line.From + line.To));
                    lengths.Add(line.Length * Math.Cos(angle));
                }
                return lengths;
            }
        }

        public double ProjectedLengthSum => ProjectedLengths.Sum();

        public List<double> ProjectedLengthsCumulated
        {
            get
            {
                List<double> lengths = new List<double>();
                double sum = 0;
                foreach(double length in ProjectedLengths)
                {
                    lengths.Add(length + sum);
                    sum = sum + length;
                }
                return lengths;
            }
        }

        public Point3d LengthToPoint(double lengthParameter)
        {
            int index = ProjectedLengthsCumulated.FindIndex(x => x >= lengthParameter);
            double diff = lengthParameter - (ProjectedLengthsCumulated[index] - ProjectedLengths[index]);
            Vector3d direction = PlotsBoundarySegments[index].Direction;
            direction.Unitize();
            return PlotsBoundarySegments[index].From + direction * diff;
        }
        
        public int LengthToSegment(double lengthParameter)
        {
            return ProjectedLengthsCumulated.FindIndex(x => x >= lengthParameter);
        }

        public  Vector3d IncisionDirection(double lengthParameter)
        {
            double avoidAngleDiff = Math.PI / 3.0;
            int index = ProjectedLengthsCumulated.FindIndex(x => x >= lengthParameter);
            double diff = lengthParameter - (ProjectedLengthsCumulated[index] - ProjectedLengths[index]);
            List<double> anglesToAvoid = new List<double>();
            double currentDirection = Trigonometry.Angle(PlotsBoundarySegments[index].From, PlotsBoundarySegments[index].To);
            if (diff <= GlobalSettings.AbsoluteTolerance)
            {
                // point is on the starting end of the segment - the direction of this and the prev segment should be avoided
                anglesToAvoid.Add(currentDirection);
                anglesToAvoid.Add(currentDirection < Math.PI ? currentDirection + Math.PI : currentDirection - Math.PI);
                if (index >= 1)
                {
                    var ang = Trigonometry.Angle(PlotsBoundarySegments[index - 1].From, PlotsBoundarySegments[index - 1].To);
                    anglesToAvoid.Add(ang);
                    anglesToAvoid.Add(ang < Math.PI ? ang + Math.PI : ang - Math.PI);
                }
            } else if ((ProjectedLengthsCumulated[index] - lengthParameter) <= GlobalSettings.AbsoluteTolerance)
            {
                anglesToAvoid.Add(currentDirection);
                if (index < PlotsBoundarySegments.Count - 1)
                {
                    var ang = Trigonometry.Angle(PlotsBoundarySegments[index + 1].From, PlotsBoundarySegments[index + 1].To);
                    anglesToAvoid.Add(ang);
                    anglesToAvoid.Add(ang < Math.PI ? ang + Math.PI : ang - Math.PI);
                }
            }

            List<double> pAngles = ParentBlock.PrincipleAngles.ToList();
            List<double> pWeights = ParentBlock.PrincipileAngleWeights.ToList();

            for (int i = 0; i < pAngles.Count; i++)
            {
                foreach (double angleToAvoid in anglesToAvoid)
                {
                    if (Math.Abs(pAngles[i] - angleToAvoid) <= avoidAngleDiff)
                    {
                        pWeights[i] = 0.0;
                    }
                }
            }

            var wrpk = Maths.MathsHelper.WeightedRandomPick<double>(pAngles, pWeights);
            double resultAngle = currentDirection;
            while (Trigonometry.OnLeftSideOf(currentDirection, resultAngle) == 1)
            {
                resultAngle = wrpk.Invoke();
            }
            return new Vector3d(Math.Cos(resultAngle), Math.Sin(resultAngle), 0);

        }

        public Line IncisionLine(double lengthParameter, Vector3d direction, double length)
        {
            return new Line(LengthToPoint(lengthParameter), LengthToPoint(lengthParameter) + direction * length);
        }

        public double IncisionLineMaxLength(double lengthParameter, Vector3d direction)
        {
            Line maximumLine = new Line(LengthToPoint(lengthParameter), LengthToPoint(lengthParameter) + direction * 99999);
            List<double> parameters;
            if (ParentBlock.IntersectComponents(maximumLine, out var _, out var __, out parameters) >= 2)
            {
                return maximumLine.PointAt(parameters[1]).DistanceTo(maximumLine.From);
            }
            return -1;
        }

        #region BreakParams

        public List<double> BreakParameters = new List<double>();
        public List<double> BracketedSortedParameters
        {
            get
            {
                List<double> parameters = BreakParameters.ToList();
                parameters.Add(parameterStart);
                parameters.Add(parameterEnd);
                parameters.Sort();
                return parameters;
            }
        }
        public double parameterStart => 0.0;

        public double parameterEnd => ProjectedLengthSum;

        public static Action<PlotsBoundarySide> GenerateBreakParameter => pbs =>
        {
            double snapDistance = 5.0;
            double p = new Random().NextDouble() * pbs.parameterEnd;
            int index = pbs.ProjectedLengthsCumulated.FindIndex(x => Math.Abs(p - x) <= snapDistance);
            if (index < 0)
            {
                pbs.BreakParameters.Add(p);
            }
            else
            {
                if (!pbs.BreakParameters.Contains(pbs.ProjectedLengthsCumulated[index]))
                {
                    pbs.BreakParameters.Add(pbs.ProjectedLengthsCumulated[index]);
                }
            }
        };

        public static Predicate<PlotsBoundarySide> BPRuntimeMinimumGap => pbs =>
        {
            double minGap = 12; 
           
            for (int i = 0; i < pbs.BracketedSortedParameters.Count - 1; i++)
            {
                if (pbs.BracketedSortedParameters[i + 1] - pbs.BracketedSortedParameters[i] < minGap)
                {
                    return false;
                }
            }
            return true;
        };

        public static Predicate<PlotsBoundarySide> BPFinal => pbs =>
        {
            double maxGap = 45;

            for (int i = 0; i < pbs.BracketedSortedParameters.Count - 1; i++)
            {
                if (pbs.BracketedSortedParameters[i + 1] - pbs.BracketedSortedParameters[i] > maxGap)
                {
                    return false;
                }
            }
            return true;
        };

        #endregion
        /*
        //temporary
        public List<double> GenerateBreakParams(List<double> lengthsCumSum)
        {
            List<double> parameters = new List<double>();
            double minSep = 15.0;
            double maxSep = 50;
            double snapDist = 5.0;
            Random random = new Random();
            bool pass = false;
            int iterations = 0;
            int maxIterations = 30;
            Predicate<List<double>> minSepCheck = (dd) =>
            {
                var d = dd.ToList();
                d.Sort();
                if (d.Count >= 1)
                {
                    List<double> dplus = new List<double>();
                    dplus.Add(0);
                    dplus.AddRange(d);
                    dplus.Add(lengthsCumSum[lengthsCumSum.Count - 1]);

                    for (int i = 0; i < dplus.Count - 1; i++)
                    {
                        if (dplus[i + 1] - dplus[i] < minSep)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            };
            Predicate<List<double>> maxSepCheck = (dd) =>
            {
                var d = dd.ToList();
                d.Sort();
                if (d.Count >= 1)
                {
                    List<double> dplus = new List<double>();
                    dplus.Add(0);
                    dplus.AddRange(d);
                    dplus.Add(lengthsCumSum[lengthsCumSum.Count - 1]);

                    for (int i = 0; i < dplus.Count - 1; i++)
                    {
                        if (dplus[i + 1] - dplus[i] > maxSep)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            };
            Func<double> prop = () =>
            {
                double p = random.NextDouble() * lengthsCumSum[lengthsCumSum.Count - 1];
                int index = lengthsCumSum.FindIndex(l => Math.Abs(l - p) <= snapDist);
                return (index == -1) ? p : lengthsCumSum[index];
            };
            List<double> states = new List<double>();
            Exploration<double>(states, prop, minSepCheck, maxSepCheck);
            states.Sort();
            return states;
        }
        
        public static bool Exploration<T>(List<T> states, Func<T> propagation , Predicate<List<T>> runtimeCompliance, Predicate<List<T>> resultCompliance)
        {
            int maxAttempts = 20;
            int attempt = 0;
            while (attempt < maxAttempts)
            {
                states.Add(propagation.Invoke());
                if (runtimeCompliance.Invoke(states))
                {
                    if (resultCompliance.Invoke(states))
                    {
                        return true;
                    }
                    if (Exploration<T>(states, propagation, runtimeCompliance, resultCompliance))
                    {
                        return true;
                    }
                }
                states.RemoveAt(states.Count - 1);
                attempt++;
            }
            return false;
        }
        */

    }

    public static class GeometryHelper
    {
        public static void LinesPrincipleDirections(List<Line> lines, out List<double> Angles, out List<double> Weights)
        {
            // make continuous later (aggregation of probability distribution functions)
            List<double> originalLengths = new List<double>();
            List<double> allLengthsDuplicated = new List<double>();
            lines.ForEach(l => originalLengths.Add(l.From.DistanceTo(l.To) / 4));

            for (int i = 0; i < 4; i ++)
            {
                allLengthsDuplicated.AddRange(originalLengths.ToList());
            }

            List<double> originalAngles = new List<double>();
            List<double> allAnglesDuplicated = new List<double>();
            lines.ForEach(l => originalAngles.Add(Trigonometry.Angle(l.From, l.To)));

            allAnglesDuplicated.AddRange(originalAngles.ToList());
            allAnglesDuplicated.AddRange(originalAngles.ConvertAll(x => x >= Math.PI ? x - Math.PI : x + Math.PI)); // + pi
            allAnglesDuplicated.AddRange(originalAngles.ConvertAll(x => x >= Math.PI / 2 ? x - Math.PI / 2 : x + Math.PI * 3 / 2)); // - pi/2
            allAnglesDuplicated.AddRange(originalAngles.ConvertAll(x => x >= Math.PI * 3 / 2 ? x - Math.PI * 3 / 2 : x + Math.PI / 2)); // + pi/2

            List<double> angles = new List<double>();
            List<double> weights = new List<double>();
            double identicalAngleTolerance = 0.1 / 180 * Math.PI;
            double combineAngleTolerance = 15.0 / 180 * Math.PI;
            for(int i = 0; i < allAnglesDuplicated.Count; i++)
            {
                double angle = allAnglesDuplicated[i];
                int index = angles.FindIndex(a => Trigonometry.AngleDifference(angle, a) <= identicalAngleTolerance);
                if (index == -1)
                {
                    angles.Add(angle);
                    weights.Add(allLengthsDuplicated[i]);
                } else
                {
                    weights[index] = weights[index] + allLengthsDuplicated[i];
                }
            }
            int pos = 0;
            while (pos < angles.Count)
            {
                int index = angles.FindIndex(a => pos != angles.IndexOf(a) && Trigonometry.AngleDifference(angles[pos], a) <= combineAngleTolerance);
                if (index == -1)
                {
                    pos++;
                } else
                {
                    if (weights[pos] >= weights[index])
                    {
                        weights[pos] = weights[pos] + weights[index];
                        weights.RemoveAt(index);
                        angles.RemoveAt(index);
                        pos++;
                    } else
                    {
                        weights[index] = weights[index] + weights[pos];
                        weights.RemoveAt(pos);
                        angles.RemoveAt(pos);
                    }
                }
            }
            Angles = angles;
            Weights = weights;
        }
    }



}
