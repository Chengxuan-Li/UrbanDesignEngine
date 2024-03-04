using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace UrbanDesignEngine.Tensor
{
    public class StreamlineIntegration
    {
        public Vector3d Seed { get; set; }
        public Vector3d OriginalDir { get; set; }
        public List<Vector3d> Streamline { get; set; }
        public Vector3d PreviousDirection { get; set; }
        public Vector3d PreviousPoint { get; set; }
        public bool Valid { get; set; }
    }

    public class StreamlineParams
    {
        public double Dsep { get; set; }  // Streamline seed separating distance
        public double Dtest { get; set; }  // Streamline integration separating distance
        public double Dstep { get; set; }  // Step size
        public double Dcirclejoin { get; set; }  // How far to look to join circles - (e.g. 2 x dstep)
        public double Dlookahead { get; set; }  // How far to look ahead to join up dangling
        public double Joinangle { get; set; }  // Angle to join roads in radians
        public int PathIterations { get; set; }  // Path integration iteration limit
        public int SeedTries { get; set; }  // Max failed seeds
        public double SimplifyTolerance { get; set; }
        public double CollideEarly { get; set; }  // Chance of early collision 0-1
    }


    public class StreamlineGenerator
    {
        protected readonly bool SEED_AT_ENDPOINTS = false;
        protected readonly int NEAR_EDGE = 3;

        protected GridStorage majorGrid;
        protected GridStorage minorGrid;
        protected StreamlineParams paramsSq;

        protected int nStreamlineStep;
        protected int nStreamlineLookBack;
        protected double dcollideselfSq;

        protected List<Vector3d> candidateSeedsMajor = new List<Vector3d>();
        protected List<Vector3d> candidateSeedsMinor = new List<Vector3d>();

        protected bool streamlinesDone = true;
        protected Action resolve;
        protected bool lastStreamlineMajor = true;

        public List<List<Vector3d>> allStreamlines = new List<List<Vector3d>>();
        public List<List<Vector3d>> streamlinesMajor = new List<List<Vector3d>>();
        public List<List<Vector3d>> streamlinesMinor = new List<List<Vector3d>>();
        public List<List<Vector3d>> allStreamlinesSimple = new List<List<Vector3d>>();


        public StreamlineGenerator(FieldIntegrator integrator, Vector3d origin, Vector3d worldDimensions, StreamlineParams parameters)
        {
            this.integrator = integrator;
            this.origin = origin;
            this.worldDimensions = worldDimensions;
            this.params = parameters;

            if (parameters.Dstep > parameters.Dsep)
            {
                Console.Error.WriteLine("STREAMLINE SAMPLE DISTANCE BIGGER THAN DSEP");
            }

            // Enforce test < sep
            parameters.Dtest = Math.Min(parameters.Dtest, parameters.Dsep);

            // Needs to be less than circlejoin
            dcollideselfSq = Math.Pow(parameters.Dcirclejoin / 2, 2);
            nStreamlineStep = (int)Math.Floor(parameters.Dcirclejoin / parameters.Dstep);
            nStreamlineLookBack = 2 * nStreamlineStep;

            majorGrid = new GridStorage(worldDimensions, origin, parameters.Dsep);
            minorGrid = new GridStorage(worldDimensions, origin, parameters.Dsep);

            SetParamsSq();
        }

        public void ClearStreamlines()
        {
            allStreamlinesSimple.Clear();
            streamlinesMajor.Clear();
            streamlinesMinor.Clear();
            allStreamlines.Clear();
        }

        public void JoinDanglingStreamlines()
        {
            // TODO do in update method
            foreach (var major in new bool[] { true, false })
            {
                foreach (var streamline in Streamlines(major))
                {
                    // Ignore circles
                    if (streamline[0].Equals(streamline[streamline.Length - 1]))
                    {
                        continue;
                    }

                    var newStart = GetBestNextPoint(streamline[0], streamline[4], streamline);
                    if (newStart != null)
                    {
                        foreach (var p in PointsBetween(streamline[0], newStart, params.Dstep))
                        {
                            streamline.Insert(0, p);
                            Grid(major).AddSample(p);
                        }
                    }

                    var newEnd = GetBestNextPoint(streamline[streamline.Length - 1], streamline[streamline.Length - 4], streamline);
                    if (newEnd != null)
                    {
                        foreach (var p in PointsBetween(streamline[streamline.Length - 1], newEnd, params.Dstep))
                        {
                            streamline.Add(p);
                            Grid(major).AddSample(p);
                        }
                    }
                }
            }

            // Reset simplified streamlines
            allStreamlinesSimple.Clear();
            foreach (var s in allStreamlines)
            {
                allStreamlinesSimple.Add(SimplifyStreamline(s));
            }
        }

        public List<Vector3d> PointsBetween(Vector3d v1, Vector3d v2, double dstep)
        {
            double d = v1.DistanceTo(v2);
            int nPoints = (int)Math.Floor(d / dstep);
            if (nPoints == 0) return new List<Vector3d>();

            Vector3d stepVector = v2.Sub(v1);

            List<Vector3d> outPoints = new List<Vector3d>();
            for (int i = 1; i <= nPoints; i++)
            {
                Vector3d next = v1.Clone().Add(stepVector.Clone().MultiplyScalar(i / (double)nPoints));
                if (integrator.Integrate(next, true).LengthSquared() > 0.001) // Test for degenerate point
                {
                    outPoints.Add(next);
                }
                else
                {
                    return outPoints;
                }
            }
            return outPoints;
        }

        public Vector3d GetBestNextPoint(Vector3d point, Vector3d previousPoint, Vector3d[] streamline)
        {
            List<Vector3d> nearbyPoints = majorGrid.GetNearbyPoints(point, params.Dlookahead);
            nearbyPoints.AddRange(minorGrid.GetNearbyPoints(point, params.Dlookahead));
            Vector3d direction = point.Clone().Sub(previousPoint);

            Vector3d closestSample = null;
            double closestDistance = double.PositiveInfinity;

            foreach (Vector3d sample in nearbyPoints)
            {
                if (!sample.Equals(point) && !sample.Equals(previousPoint))
                {
                    Vector3d differenceVector = sample.Clone().Sub(point);
                    if (differenceVector.Dot(direction) < 0)
                    {
                        // Backwards
                        continue;
                    }

                    double distanceToSample = point.DistanceToSquared(sample);
                    if (distanceToSample < 2 * paramsSq.Dstep)
                    {
                        closestSample = sample;
                        break;
                    }

                    double angleBetween = Math.Abs(Vector3d.AngleBetween(direction, differenceVector));

                    if (angleBetween < params.Joinangle && distanceToSample < closestDistance)
            {
                        closestDistance = distanceToSample;
                        closestSample = sample;
                    }
                }
            }

            // TODO is reimplement simplify-js to preserve intersection points
            //  - this is the primary reason polygons aren't found
            // If trying to find intersections in the simplified graph
            // prevent ends getting pulled away from simplified lines
            if (closestSample != null)
            {
                closestSample = closestSample.Clone().Add(direction.SetLength(params.SimplifyTolerance * 4));
            }

            return closestSample;
        }

        public void AddExistingStreamlines(StreamlineGenerator s)
        {
            majorGrid.AddAll(s.MajorGrid);
            minorGrid.AddAll(s.MinorGrid);
        }

        public void SetGrid(StreamlineGenerator s)
        {
            majorGrid = s.MajorGrid;
            minorGrid = s.MinorGrid;
        }

        public bool Update()
        {
            if (!streamlinesDone)
            {
                lastStreamlineMajor = !lastStreamlineMajor;
                if (!CreateStreamline(lastStreamlineMajor))
                {
                    streamlinesDone = true;
                    resolve();
                }
                return true;
            }

            return false;
        }

        public async Task CreateAllStreamlines(bool animate = false)
        {
            await Task.Run(() =>
            {
                streamlinesDone = false;

                if (!animate)
                {
                    bool major = true;
                    while (CreateStreamline(major))
                    {
                        major = !major;
                    }
                }
            });

            JoinDanglingStreamlines();
        }

        protected Vector3d[] SimplifyStreamline(Vector3d[] streamline)
        {
            List<Vector3d> simplified = new List<Vector3d>();
            foreach (Vector3d point in Simplify(streamline, params.SimplifyTolerance))
            {
                simplified.Add(new Vector3d(point.X, point.Y));
            }
            return simplified.ToArray();
        }

        protected bool CreateStreamline(bool major)
        {
            Vector3d seed = GetSeed(major);
            if (seed == null)
            {
                return false;
            }
            Vector3d[] streamline = IntegrateStreamline(seed, major);
            if (ValidStreamline(streamline))
            {
                grid(major).AddPolyline(streamline);
                streamlines(major).Add(streamline);
                allStreamlines.Add(streamline);

                allStreamlinesSimple.Add(SimplifyStreamline(streamline));

                // Add candidate seeds
                if (!streamline[0].Equals(streamline[streamline.Length - 1]))
                {
                    candidateSeeds(!major).Add(streamline[0]);
                    candidateSeeds(!major).Add(streamline[streamline.Length - 1]);
                }
            }

            return true;
        }

        protected bool ValidStreamline(Vector3d[] s)
        {
            return s.Length > 5;
        }

        protected void SetParamsSq()
        {
            paramsSq = new Dictionary<string, object>(params);
            foreach (string p in paramsSq.Keys.ToList())
            {
                if (paramsSq[p] is double)
                {
                    paramsSq[p] = (double)paramsSq[p] * (double)paramsSq[p];
                }
            }
        }

        protected Vector3d SamplePoint()
        {
            // TODO better seeding scheme
            return new Vector3d(
                Random.NextDouble() * worldDimensions.X,
                Random.NextDouble() * worldDimensions.Y)
                .Add(origin);
        }

        protected Vector3d GetSeed(bool major)
        {
            // Candidate seeds first
            if (SEED_AT_ENDPOINTS && candidateSeeds(major).Count > 0)
            {
                while (candidateSeeds(major).Count > 0)
                {
                    Vector3d seed = candidateSeeds(major).Pop();
                    if (IsValidSample(major, seed, paramsSq["dsep"]))
                    {
                        return seed;
                    }
                }
            }

            Vector3d seed = SamplePoint();
            int i = 0;
            while (!IsValidSample(major, seed, (double)paramsSq["dsep"]))
            {
                if (i >= (int)params["seedTries"])
        {
                    return null;
                }
                seed = SamplePoint();
                i++;
            }

            return seed;
        }

        protected bool IsValidSample(bool major, Vector3d point, double dSq, bool bothGrids = false)
        {
            bool gridValid = grid(major).IsValidSample(point, dSq);
            if (bothGrids)
            {
                gridValid = gridValid && grid(!major).IsValidSample(point, dSq);
            }
            return integrator.OnLand(point) && gridValid;
        }

        protected Stack<Vector3d> CandidateSeeds(bool major)
        {
            return major ? candidateSeedsMajor : candidateSeedsMinor;
        }

        protected Vector3d[][] Streamlines(bool major)
        {
            return major ? streamlinesMajor : streamlinesMinor;
        }

        protected GridStorage Grid(bool major)
        {
            return major ? majorGrid : minorGrid;
        }

        protected bool PointInBounds(Vector3d v)
        {
            return (v.X >= origin.X
                && v.Y >= origin.Y
                && v.X < worldDimensions.X + origin.X
                && v.Y < worldDimensions.Y + origin.Y
            );
        }

        
        // did not end up using
        protected bool DoesStreamlineCollideSelf(Vector3d testSample, List<Vector3d> streamlineForwards, List<Vector3d> streamlineBackwards)
        {
            // Streamline long enough
            if (streamlineForwards.Count > nStreamlineLookBack)
            {
                // Forwards check
                for (int i = 0; i < streamlineForwards.Count - nStreamlineLookBack; i += nStreamlineStep)
                {
                    if (testSample.DistanceToSquared(streamlineForwards[i]) < dcollideselfSq)
                    {
                        return true;
                    }
                }

                // Backwards check
                for (int i = 0; i < streamlineBackwards.Count; i += nStreamlineStep)
                {
                    if (testSample.DistanceToSquared(streamlineBackwards[i]) < dcollideselfSq)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool StreamlineTurned(Vector3d seed, Vector3d originalDir, Vector3d point, Vector3d direction)
        {
            if (originalDir.Dot(direction) < 0)
            {
                // TODO optimize
                Vector3d perpendicularVector = new Vector3d(originalDir.Y, -originalDir.X);
                bool isLeft = point.Clone().Subtract(seed).Dot(perpendicularVector) < 0;
                bool directionUp = direction.Dot(perpendicularVector) > 0;
                return isLeft == directionUp;
            }

            return false;
        }

        protected void StreamlineIntegrationStep(StreamlineIntegration parameters, bool major, bool collideBoth)
        {
            if (parameters.Valid)
            {
                parameters.Streamline.Add(parameters.PreviousPoint);
                Vector3d nextDirection = this.Integrator.Integrate(parameters.PreviousPoint, major);

                // Stop at degenerate point
                if (nextDirection.LengthSquared() < 0.01)
                {
                    parameters.Valid = false;
                    return;
                }

                // Make sure we travel in the same direction
                if (nextDirection.Dot(parameters.PreviousDirection) < 0)
                {
                    nextDirection.Negate();
                }

                Vector3d nextPoint = parameters.PreviousPoint.Clone().Add(nextDirection);

                // Visualize stopping points
                // if (this.StreamlineTurned(parameters.Seed, parameters.OriginalDir, nextPoint, nextDirection)) {
                //     parameters.Valid = false;
                //     parameters.Streamline.Add(Vector3d.ZeroVector());
                // }

                if (this.PointInBounds(nextPoint) && this.IsValidSample(major, nextPoint, this.ParamsSq.Dtest, collideBoth)
                    && !this.StreamlineTurned(parameters.Seed, parameters.OriginalDir, nextPoint, nextDirection))
                {
                    parameters.PreviousPoint = nextPoint;
                    parameters.PreviousDirection = nextDirection;
                }
                else
                {
                    // One more step
                    parameters.Streamline.Add(nextPoint);
                    parameters.Valid = false;
                }
            }
        }

        protected Vector3d[] IntegrateStreamline(Vector3d seed, bool major)
        {
            int count = 0;
            bool pointsEscaped = false;  // True once two integration fronts have moved dlookahead away

            // Whether or not to test validity using both grid storages
            // (Collide with both major and minor)
            bool collideBoth = new Random().NextDouble() < this.Params.CollideEarly;

            Vector3d d = this.Integrator.Integrate(seed, major);

            StreamlineIntegration forwardParams = new StreamlineIntegration
            {
                Seed = seed,
                OriginalDir = d,
                Streamline = new List<Vector3d> { seed },
                PreviousDirection = d,
                PreviousPoint = seed.Clone().Add(d),
                Valid = true,
            };

            forwardParams.Valid = this.PointInBounds(forwardParams.PreviousPoint);

            Vector3d negD = d.Clone().Negate();
            StreamlineIntegration backwardParams = new StreamlineIntegration
            {
                Seed = seed,
                OriginalDir = negD,
                Streamline = new List<Vector3d>(),
                PreviousDirection = negD,
                PreviousPoint = seed.Clone().Add(negD),
                Valid = true,
            };

            backwardParams.Valid = this.PointInBounds(backwardParams.PreviousPoint);

            while (count < this.Params.PathIterations && (forwardParams.Valid || backwardParams.Valid))
            {
                StreamlineIntegrationStep(forwardParams, major, collideBoth);
                StreamlineIntegrationStep(backwardParams, major, collideBoth);

                // Join up circles
                double sqDistanceBetweenPoints = forwardParams.PreviousPoint.DistanceToSquared(backwardParams.PreviousPoint);

                if (!pointsEscaped && sqDistanceBetweenPoints > this.ParamsSq.Dcirclejoin)
                {
                    pointsEscaped = true;
                }

                if (pointsEscaped && sqDistanceBetweenPoints <= this.ParamsSq.Dcirclejoin)
                {
                    forwardParams.Streamline.Add(forwardParams.PreviousPoint);
                    forwardParams.Streamline.Add(backwardParams.PreviousPoint);
                    backwardParams.Streamline.Add(backwardParams.PreviousPoint);
                    break;
                }

                count++;
            }

            backwardParams.Streamline.Reverse();
            backwardParams.Streamline.AddRange(forwardParams.Streamline);
            return backwardParams.Streamline.ToArray();
        }

    }
}
