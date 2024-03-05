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

        public static StreamlineParams Default => new StreamlineParams 
        { 
            Dsep = 10,
            Dtest = 2,
            Dstep = 0.5,
            Dcirclejoin = 1,
            Dlookahead = 2,
            Joinangle = Math.PI / 6.0,
            PathIterations = 5000,
            SeedTries = 3,
            SimplifyTolerance = 0.01,
            CollideEarly = 0.1,
        };
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

        protected FieldIntegrator integrator;
        protected Vector3d origin;
        protected Vector3d worldDimensions;
        protected StreamlineParams parameters;

        public List<List<Vector3d>> allStreamlines = new List<List<Vector3d>>();
        public List<List<Vector3d>> streamlinesMajor = new List<List<Vector3d>>();
        public List<List<Vector3d>> streamlinesMinor = new List<List<Vector3d>>();
        public List<List<Vector3d>> allStreamlinesSimple = new List<List<Vector3d>>();

        public Random Random = new Random();


        public StreamlineGenerator(FieldIntegrator integrator, Vector3d origin, Vector3d worldDimensions, StreamlineParams parameters)
        {
            this.integrator = integrator;
            this.origin = origin;
            this.worldDimensions = worldDimensions;
            this.parameters = parameters;

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

            //SetParamsSq();
            paramsSq = parameters;
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
                    if (streamline[0].Equals(streamline[streamline.Count - 1]))
                    {
                        continue;
                    }

                    var newStart = GetBestNextPoint(streamline[0], streamline[4], streamline);
                    if (newStart != null)
                    {
                        foreach (var p in PointsBetween(streamline[0], newStart, parameters.Dstep))
                        {
                            streamline.Insert(0, p);
                            Grid(major).AddSample(p);
                        }
                    }

                    var newEnd = GetBestNextPoint(streamline[streamline.Count - 1], streamline[streamline.Count - 4], streamline);
                    if (newEnd != null)
                    {
                        foreach (var p in PointsBetween(streamline[streamline.Count - 1], newEnd, parameters.Dstep))
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
            double d = new Point3d(v1).DistanceTo(new Point3d(v2));
            int nPoints = (int)Math.Floor(d / dstep);
            if (nPoints == 0) return new List<Vector3d>();

            Vector3d stepVector = v2 - v1;

            List<Vector3d> outPoints = new List<Vector3d>();
            for (int i = 1; i <= nPoints; i++)
            {
                Vector3d next = new Vector3d(v1) + new Vector3d(stepVector) * (i / (double)nPoints);
                if (integrator.Integrate(next, true).Length > 0.001) // Test for degenerate point
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

        public Vector3d GetBestNextPoint(Vector3d point, Vector3d previousPoint, List<Vector3d> streamline)
        {
            List<Vector3d> nearbyPoints = majorGrid.GetNearbyPoints(point, parameters.Dlookahead);
            nearbyPoints.AddRange(minorGrid.GetNearbyPoints(point, parameters.Dlookahead));
            Vector3d direction = new Vector3d(point) - previousPoint;

            Vector3d closestSample = default;
            double closestDistance = double.PositiveInfinity;

            foreach (Vector3d sample in nearbyPoints)
            {
                if (!sample.Equals(point) && !sample.Equals(previousPoint))
                {
                    Vector3d differenceVector = new Vector3d(sample) - point;
                    if ((differenceVector.X * direction.X + differenceVector.Y * direction.Y) < 0)
                    {
                        // Backwards
                        continue;
                    }

                    double distanceToSample = new Point3d(point).DistanceTo(new Point3d(sample));
                    if (distanceToSample < 2 * paramsSq.Dstep)
                    {
                        closestSample = sample;
                        break;
                    }

                    double angleBetween = Math.Abs(AngleBetween(direction, differenceVector));

                    if (angleBetween < parameters.Joinangle && distanceToSample < closestDistance)
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
                Vector3d direction1 = new Vector3d(direction);
                direction1.Unitize();
                direction1 = direction1 * parameters.SimplifyTolerance * 4;
                closestSample = closestSample + direction1;
            }

            return closestSample;
        }

        double AngleBetween(Vector3d v1, Vector3d v2)
        {
            double angle = Math.Atan2(v1.Y, v1.X) - Math.Atan2(v2.Y, v2.X);
            if (angle > Math.PI) angle -= 2.0 * Math.PI;
            if (angle <= -Math.PI) angle += 2.0 * Math.PI;
            return angle;
        }

        public void AddExistingStreamlines(StreamlineGenerator s)
        {
            majorGrid.AddAll(s.majorGrid);
            minorGrid.AddAll(s.minorGrid);
        }

        public void SetGrid(StreamlineGenerator s)
        {
            majorGrid = s.majorGrid;
            minorGrid = s.minorGrid;
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

        public void RunCreateAllStreamlines()
        {
            streamlinesDone = false;
            bool major = true;
            while (CreateStreamline(major))
            {
                major = !major;
            }
            JoinDanglingStreamlines();
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

        protected List<Vector3d> SimplifyStreamline(List<Vector3d> streamline)
        {
            List<Vector3d> simplified = new List<Vector3d>();
            Polyline pl = new Polyline(streamline.ConvertAll(v => new Point3d(v)));
            Curve curve = pl.ToPolylineCurve().Simplify(CurveSimplifyOptions.Merge, parameters.SimplifyTolerance, parameters.SimplifyTolerance);
            curve.TryGetPolyline(out pl);
            foreach (Vector3d point in pl.ConvertAll(p => new Vector3d(p)))
            {
                simplified.Add(new Vector3d(point.X, point.Y, 0));
            }
            return simplified.ToList();
        }

        protected bool CreateStreamline(bool major)
        {
            Vector3d seed = GetSeed(major);
            if (seed == null)
            {
                return false;
            }
            List<Vector3d> streamline = IntegrateStreamline(seed, major);
            if (ValidStreamline(streamline))
            {
                Grid(major).AddPolyline(streamline);
                Streamlines(major).Add(streamline);
                allStreamlines.Add(streamline);

                allStreamlinesSimple.Add(SimplifyStreamline(streamline));

                // Add candidate seeds
                if (!streamline[0].Equals(streamline[streamline.Count - 1]))
                {
                    CandidateSeeds(!major).Add(streamline[0]);
                    CandidateSeeds(!major).Add(streamline[streamline.Count - 1]);
                }
            }

            return true;
        }

        protected bool ValidStreamline(List<Vector3d> s)
        {
            return s.Count > 5;
        }

        /*
        protected void SetParamsSq()
        {
            paramsSq = new Dictionary<string, object>(parameters);
            foreach (string p in paramsSq.Keys.ToList())
            {
                if (paramsSq[p] is double)
                {
                    paramsSq[p] = (double)paramsSq[p] * (double)paramsSq[p];
                }
            }
        }
        */

        protected Vector3d SamplePoint()
        {
            // TODO better seeding scheme
            return new Vector3d(
                Random.NextDouble() * worldDimensions.X,
                Random.NextDouble() * worldDimensions.Y,
                0)
                + origin;
        }

        protected Vector3d GetSeed(bool major)
        {
            // Candidate seeds first
            if (SEED_AT_ENDPOINTS && CandidateSeeds(major).Count > 0)
            {
                while (CandidateSeeds(major).Count > 0)
                {
                    Vector3d sd = CandidateSeeds(major).Last();
                    CandidateSeeds(major).RemoveAt(CandidateSeeds(major).Count - 1);
                    if (IsValidSample(major, sd, paramsSq.Dsep))
                    {
                        return sd;
                    }
                }
            }

            Vector3d seed = SamplePoint();
            int i = 0;
            while (!IsValidSample(major, seed, (double)paramsSq.Dsep))
            {
                if (i >= (int)parameters.SeedTries)
        {
                    return default;
                }
                seed = SamplePoint();
                i++;
            }

            return seed;
        }

        protected bool IsValidSample(bool major, Vector3d point, double dSq, bool bothGrids = false)
        {
            bool gridValid = Grid(major).IsValidSample(point, dSq);
            if (bothGrids)
            {
                gridValid = gridValid && Grid(!major).IsValidSample(point, dSq);
            }
            return integrator.OnLand(point) && gridValid;
        }

        protected List<Vector3d> CandidateSeeds(bool major)
        {
            return major ? candidateSeedsMajor : candidateSeedsMinor;
        }

        protected List<List<Vector3d>> Streamlines(bool major)
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
                    if (new Point3d(testSample).DistanceTo(new Point3d(streamlineForwards[i])) < dcollideselfSq)
                    {
                        return true;
                    }
                }

                // Backwards check
                for (int i = 0; i < streamlineBackwards.Count; i += nStreamlineStep)
                {
                    if (new Point3d(testSample).DistanceTo(new Point3d(streamlineBackwards[i])) < dcollideselfSq)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool StreamlineTurned(Vector3d seed, Vector3d originalDir, Vector3d point, Vector3d direction)
        {
            if ((originalDir.X * direction.X + originalDir.Y * direction.Y) < 0)
            {
                // TODO optimize
                Vector3d perpendicularVector = new Vector3d(originalDir.Y, -originalDir.X, 0);
                Vector3d vc = point - seed;
                bool isLeft = vc.X * perpendicularVector.X + vc.Y * perpendicularVector.Y < 0;
                bool directionUp = direction.X * perpendicularVector.X + direction.Y * perpendicularVector.Y > 0;
                return isLeft == directionUp;
            }

            return false;
        }

        protected void StreamlineIntegrationStep(StreamlineIntegration parameters, bool major, bool collideBoth)
        {
            if (parameters.Valid)
            {
                parameters.Streamline.Add(parameters.PreviousPoint);
                Vector3d nextDirection = this.integrator.Integrate(parameters.PreviousPoint, major);

                // Stop at degenerate point
                if ((nextDirection.Length) < 0.01)
                {
                    parameters.Valid = false;
                    return;
                }

                // Make sure we travel in the same direction
                if ((nextDirection.X * parameters.PreviousDirection.X + nextDirection.Y * parameters.PreviousDirection.Y) < 0)
                {
                    nextDirection = -nextDirection;
                }

                Vector3d nextPoint = parameters.PreviousPoint + (nextDirection);

                // Visualize stopping points
                // if (this.StreamlineTurned(parameters.Seed, parameters.OriginalDir, nextPoint, nextDirection)) {
                //     parameters.Valid = false;
                //     parameters.Streamline.Add(Vector3d.ZeroVector());
                // }

                if (this.PointInBounds(nextPoint) && this.IsValidSample(major, nextPoint, this.paramsSq.Dtest, collideBoth)
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

        protected List<Vector3d> IntegrateStreamline(Vector3d seed, bool major)
        {
            int count = 0;
            bool pointsEscaped = false;  // True once two integration fronts have moved dlookahead away

            // Whether or not to test validity using both grid storages
            // (Collide with both major and minor)
            bool collideBoth = new Random().NextDouble() < this.parameters.CollideEarly;

            Vector3d d = this.integrator.Integrate(seed, major);

            StreamlineIntegration forwardParams = new StreamlineIntegration
            {
                Seed = seed,
                OriginalDir = d,
                Streamline = new List<Vector3d> { seed },
                PreviousDirection = d,
                PreviousPoint = seed + d,
                Valid = true,
            };

            forwardParams.Valid = this.PointInBounds(forwardParams.PreviousPoint);

            Vector3d negD = -d;
            StreamlineIntegration backwardParams = new StreamlineIntegration
            {
                Seed = seed,
                OriginalDir = negD,
                Streamline = new List<Vector3d>(),
                PreviousDirection = negD,
                PreviousPoint = seed + negD,
                Valid = true,
            };

            backwardParams.Valid = this.PointInBounds(backwardParams.PreviousPoint);

            while (count < this.parameters.PathIterations && (forwardParams.Valid || backwardParams.Valid))
            {
                StreamlineIntegrationStep(forwardParams, major, collideBoth);
                StreamlineIntegrationStep(backwardParams, major, collideBoth);

                // Join up circles
                double sqDistanceBetweenPoints = new Point3d(forwardParams.PreviousPoint).DistanceTo(new Point3d(backwardParams.PreviousPoint));

                if (!pointsEscaped && sqDistanceBetweenPoints > this.paramsSq.Dcirclejoin)
                {
                    pointsEscaped = true;
                }

                if (pointsEscaped && sqDistanceBetweenPoints <= this.paramsSq.Dcirclejoin)
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
            return backwardParams.Streamline.ToList();
        }

    }
}
