using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.Tensor
{
    public struct GridIndex : IDuplicable<GridIndex>, IEquatable<GridIndex>
    {
        public int x;
        public int y;

        public GridIndex(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public GridIndex Duplicate()
        {
            return new GridIndex { x = x, y = y };
        }

        public bool Equals(GridIndex other)
        {
            return x == other.x && y == other.y;
        }
    }

    public class Grid : IDuplicable<Grid>
    {
        public Dictionary<GridIndex, List<Vector3d>> Content = new Dictionary<GridIndex, List<Vector3d>>();
        protected int xDim = 0;
        protected int yDim = 0;

        public int XDim => xDim;

        public int YDim => yDim;

        public Grid Duplicate()
        {
            return new Grid() { xDim = xDim, yDim = yDim, Content = Content.ToDictionary(entry => entry.Key.Duplicate(), entry => entry.Value.ConvertAll(v => new Vector3d(v))) };
        }

        public bool Get(GridIndex gi, out List<Vector3d> vecs)
        {
            return Content.TryGetValue(gi, out vecs);
        }

        public bool Get(int x, int y, out List<Vector3d> vecs)
        {
            return Content.TryGetValue(new GridIndex(x, y), out vecs);
        }

        public void Set(int x, int y, Vector3d vec)
        {
            xDim = Math.Max(x + 1, xDim);
            yDim = Math.Max(y + 1, yDim);
            if (Content.TryGetValue(new GridIndex(x, y), out List<Vector3d> vecs))
            {
                vecs.Add(vec);
            } else
            {
                Content.Add(new GridIndex(x, y), new List<Vector3d> { vec });
            }
        }

        public void Set(GridIndex id, Vector3d vec)
        {
            Set(id.x, id.y, vec);
        }
    }


    public class GridStorage
    {
        private Vector3d gridDimensions; // number of Xs and Ys
        private Grid Grid;
        private double dsepSq;
        private readonly Vector3d worldDimensions;
        private readonly Vector3d origin;
        private readonly double dsep;

        public GridStorage(Vector3d worldDimensions, Vector3d origin, double dsep)
        {
            this.worldDimensions = worldDimensions;
            this.origin = origin;
            this.dsep = dsep;
            this.dsepSq = this.dsep * this.dsep;
            this.gridDimensions = worldDimensions / this.dsep;
            this.Grid = new Grid();
        }

        public void AddAll(GridStorage gridStorage)
        {
            foreach (var kvp in gridStorage.Grid.Content)
            {
                if (Grid.Get(kvp.Key, out List<Vector3d> vecs))
                {
                    vecs.AddRange(kvp.Value);
                } else
                {
                    foreach (Vector3d v in kvp.Value)
                    {
                        Grid.Set(kvp.Key.x, kvp.Key.y, v);
                    }
                    
                }
            }
            /*
            for (int i = 0; i < gridStorage.Grid.Length; i++)
            {
                for (int j = 0; j < gridStorage.Grid[i].Length; j++)
                {
                    foreach (var sample in gridStorage.Grid[i][j])
                    {
                        AddSample(sample);
                    }
                }
            }
            */
        }

        public void AddPolyline(List<Vector3d> line)
        {
            foreach (var v in line)
            {
                AddSample(v);
            }
        }

        public void AddSample(Vector3d v)
        {
            GridIndex coords = GetSampleCoords(v);
            AddSample(v, coords);
        }

        public void AddSample(Vector3d v, GridIndex coords)
        {
            Grid.Set(coords.x, coords.y, v);
        }

        public bool IsValidSample(Vector3d v, double dSq = double.NaN)
        {
            if (double.IsNaN(dSq))
            {
                dSq = this.dsepSq;
            }

            GridIndex coords = GetSampleCoords(v);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    GridIndex cell = coords.Duplicate();
                    cell.x += x;
                    cell.y += y;
                    if (!VectorOutOfBounds(cell, this.gridDimensions))
                    {
                        if (!Grid.Get(cell, out List<Vector3d> vecs)) return false;
                        if (!VectorFarFromVectors(v, vecs , dSq))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool VectorFarFromVectors(Vector3d v, List<Vector3d> vectors, double dSq)
        {
            foreach (var sample in vectors)
            {
                if (!sample.Equals(v))
                {
                    double distanceSq = new Point3d(sample).DistanceTo(new Point3d(v));
                    if (distanceSq < dSq)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public List<Vector3d> GetNearbyPoints(Vector3d v, double distance)
        {
            int radius = (int)Math.Ceiling((distance / this.dsep) - 0.5);
            GridIndex coords = GetSampleCoords(v);
            List<Vector3d> outList = new List<Vector3d>();
            for (int x = -1 * radius; x <= 1 * radius; x++)
            {
                for (int y = -1 * radius; y <= 1 * radius; y++)
                {
                    GridIndex cell = coords.Duplicate();
                    cell.x += x;
                    cell.y += y;
                    if (!VectorOutOfBounds(cell, this.gridDimensions))
                    {
                        if (Grid.Get(cell, out List<Vector3d> vecs))
                        {
                            outList.AddRange(vecs);
                        }
                    }
                }
            }

            return outList;
        }

        private Vector3d WorldToGrid(Vector3d v)
        {
            return new Vector3d(v) - this.origin;
        }

        private Vector3d GridToWorld(Vector3d v)
        {
            return new Vector3d(v) + this.origin;
        }

        private bool VectorOutOfBounds(GridIndex gridV, Vector3d bounds)
        {
            return (gridV.x < 0 || gridV.y < 0 ||
                gridV.x >= bounds.X || gridV.y >= bounds.Y);
        }

        private bool VectorOutOfBounds(Vector3d gridV, Vector3d bounds)
        {
            return (gridV.X < 0 || gridV.Y < 0 ||
                gridV.X >= bounds.X || gridV.Y >= bounds.Y);
        }

        private GridIndex GetSampleCoords(Vector3d worldV)
        {
            Vector3d v = WorldToGrid(worldV);
            if (VectorOutOfBounds(v, this.worldDimensions))
            {
                return new GridIndex(0, 0);
            }

            return new GridIndex(
                (int)Math.Floor(v.X / this.dsep),
                (int)Math.Floor(v.Y / this.dsep)
            );
        }
    }

}
