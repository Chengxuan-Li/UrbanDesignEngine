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
                for (int j = 0; i < side.PlotsBoundarySegments.Count; j++)
                {
                    Line segment = side.PlotsBoundarySegments[j];
                    double a;
                    bool intersecting = Intersection.LineLine(line, segment, out a, out _, GlobalSettings.AbsoluteTolerance, true);
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
            List<double> pWeights = ParentBlock.PrincipleAngles.ToList();

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
