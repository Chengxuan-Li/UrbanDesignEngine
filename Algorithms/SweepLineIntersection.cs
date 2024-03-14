using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace UrbanDesignEngine.Algorithms
{
    public static class SweepLineIntersection
    {
		static double epsilon = 0.000001;

		enum pointSegmentRelation
        {
			Disjoint = 0,
			Coincident = 1,
			OnSegment = 2,
        }


		public static bool LineLineIntersection(Line line1, Line line2)
        {
			return doIntersect(
				new Segment() {
					Left = new Point() { X = line1.From.X, Y = line1.From.Y },
					Right = new Point() { X = line1.To.X, Y = line1.To.Y }
				}, 
				new Segment() {
					Left = new Point() { X = line2.From.X, Y = line2.From.Y },
					Right = new Point() { X = line2.To.X, Y = line2.To.Y }
				}, true);
        }

		public static bool TempLineNetworkIntersection(Line line, List<Line> networkLines, out List<double> parameters)
        {
			parameters = new List<double>();
			double paramA;
			double paramB;
			foreach (Line networkLine in networkLines)
            {
				if (Intersection.LineLine(line, networkLine, out paramA, out paramB))
                {
					if (paramA > 0 && paramA < 1 && paramB >= 0 && paramB <= 1) parameters.Add(paramA);
                }
            }
			return parameters.Count > 0;
        }

        public static bool LineNetworkIntersection(Line line, List<Line> networkLines, out List<int> indices)
        {
			List<Segment> segments = new List<Segment> {
				new Segment {
					Left = new Point {
						X = line.FromX,
						Y = line.FromY
					},
					Right = new Point {
						X = line.ToX,
						Y = line.ToY
					}
				} 
			};
			networkLines.ForEach(
				networkLine => segments.Add(
					new Segment
					{
						Left = new Point
						{
							X = networkLine.FromX,
							Y = networkLine.FromY
						},
						Right = new Point
						{
							X = networkLine.ToX,
							Y = networkLine.ToY
						}
					})
				);
			List<int> indicesA;
			List<int> indicesB;
			indices = new List<int>();
			int numIntersections = isIntersect(segments, true, out indicesA, out indicesB);
			if (numIntersections == 0)
            {
				return false;
            } else
            {
				for (int i = 0; i < indicesA.Count; i++)
                {
					if (indicesA[i] == 0)
                    {
						indices.Add(indicesB[i] - 1);
                    }
                }
				return indicesB.Count > 0;
            }
        }

		public static bool NetworkSelfIntersection(List<Line> networkLines, bool ignoreCoincidentEnds, out List<int> indicesA, out List<int> indicesB)
        {
			indicesA = new List<int>();
			indicesB = new List<int>();
			List<Segment> segments = new List<Segment>();
			networkLines.ForEach(
				networkLine => segments.Add(
					new Segment
					{
						Left = new Point
						{
							X = networkLine.FromX,
							Y = networkLine.FromY
						},
						Right = new Point
						{
							X = networkLine.ToX,
							Y = networkLine.ToY
						}
					})
				);
			int numIntersections = isIntersect(segments, ignoreCoincidentEnds, out indicesA, out indicesB);
			return numIntersections > 0;
        }

		static void PerformIntersection(bool ignoreCoincidentEnds)
        {

        }

		internal class Point
        {
			public double X;
			public double Y;

        }

		// A line segment with left as Point
		// with smaller x value and right with
		// larger x value.
		internal class Segment
        {
			public Point Left
            {
				get
				{
					return (pA.X > pB.X) ? pB : pA;
				}
				set
                {
					pA = value;
                }
            }
			public Point Right
            {
				get
				{
					return (pA.X > pB.X) ? pA : pB;
				}
				set
				{
					pB = value;
				}
			}

			protected Point pA;
			protected Point pB;

        }

		// An event for sweep line algorithm
		// An event has a point, the position
		// of point (whether left or right) and
		// index of point in the original input
		// array of segments.
		internal class Event : IComparable<Event>
        {
			public double x;
			public double y;
			public bool isLeft;
			public int index;
			public Event(double _x, double _y, bool l, int i)
			{
				x = _x;
				y = _y;
				isLeft = l;
				index = i;
			}

			// This is for maintaining the order in set.
			public int CompareTo(Event e) 
			{
				if (y == e.y) return x.CompareTo(e.x);
				return y.CompareTo(e.y);
			}
		}

		// Given three collinear points p, q, r, the function checks if
		// point q lies on line segment 'pr'
		static pointSegmentRelation onSegment(Point p, Point q, Point r)
		{
			if (q.X < Math.Max(p.X, r.X) && q.X > Math.Min(p.X, r.X) &&
				q.Y < Math.Max(p.Y, r.Y) && q.Y > Math.Min(p.Y, r.Y))
            {
				return pointSegmentRelation.OnSegment;
			}
				
			if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
				q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
            {
				return pointSegmentRelation.Coincident;
			}
			return pointSegmentRelation.Disjoint;
		}

		// To find orientation of ordered triplet (p, q, r).
		// The function returns following values
		// 0 --> p, q and r are collinear
		// 1 --> Clockwise
		// 2 --> Counterclockwise
		static int orientation(Point p, Point q, Point r)
		{
			// See https://www.geeksforgeeks.org/orientation-3-ordered-points/
			// for details of below formula.
			double val = (q.Y - p.Y) * (r.X - q.X) -
					(q.X - p.X) * (r.Y - q.Y);

			if (Math.Abs(val) <= epsilon) return 0; // collinear

			return (val > 0) ? 1 : 2; // clock or counterclock wise
		}

		// The main function that returns true if line segment 'p1q1'
		// and 'p2q2' intersect.
		static bool doIntersect(Segment s1, Segment s2, bool ignoreCoincidentEnds)
		{
			Point p1 = s1.Left, q1 = s1.Right, p2 = s2.Left, q2 = s2.Right;

			// Find the four orientations needed for general and
			// special cases
			int o1 = orientation(p1, q1, p2);
			int o2 = orientation(p1, q1, q2);
			int o3 = orientation(p2, q2, p1);
			int o4 = orientation(p2, q2, q1);


			if (ignoreCoincidentEnds)
            {
				if (o1 == 0 && onSegment(p1, p2, q1) == pointSegmentRelation.Coincident) return false;
				if (o2 == 0 && onSegment(p1, q2, q1) == pointSegmentRelation.Coincident) return false;
				if (o3 == 0 && onSegment(p2, p1, q2) == pointSegmentRelation.Coincident) return false;
				if (o4 == 0 && onSegment(p2, q1, q2) == pointSegmentRelation.Coincident) return false;
			}

			// General case
			if (o1 != o2 && o3 != o4)
				return true;

			// Special Cases
			// p1, q1 and p2 are collinear and p2 lies on segment p1q1
			if (o1 == 0 && onSegment(p1, p2, q1) != pointSegmentRelation.Disjoint) return true;

			// p1, q1 and q2 are collinear and q2 lies on segment p1q1
			if (o2 == 0 && onSegment(p1, q2, q1) != pointSegmentRelation.Disjoint) return true;

			// p2, q2 and p1 are collinear and p1 lies on segment p2q2
			if (o3 == 0 && onSegment(p2, p1, q2) != pointSegmentRelation.Disjoint) return true;

			// p2, q2 and q1 are collinear and q1 lies on segment p2q2
			if (o4 == 0 && onSegment(p2, q1, q2) != pointSegmentRelation.Disjoint) return true;

			return false; // Doesn't fall in any of the above cases
		}

		// Find predecessor of iterator in s.
		static SortedSet<Event>.Enumerator Pred(SortedSet<Event> s, SortedSet<Event>.Enumerator it)
		{
			if (it.Equals(s.GetEnumerator())) // If it is the beginning
				return s.GetEnumerator(); // return the end
			it.MoveNext(); // Move the iterator one step backwards
			return it; // Return the updated iterator
		}

		// Find successor of iterator in s.
		static SortedSet<Event>.Enumerator Succ(SortedSet<Event> s, SortedSet<Event>.Enumerator it)
		{
			it.MoveNext(); // Move the iterator one step forward
			return it; // Return the updated iterator
		}




		// Returns true if any two lines intersect.
		static int isIntersect(List<Segment> segments, bool ignoreCoincidentEnds, out List<int> indicesA, out List<int> indicesB)
		{
			Segment[] arr = segments.ToArray();
			int n = arr.Length;
			indicesA = new List<int>();
			indicesB = new List<int>();

			Dictionary<string, int> mp = new Dictionary<string, int>(); // to note the pair for which intersection is checked already
										   // Pushing all points to a vector of events
			List<Event> e = new List<Event>();
			for (int i = 0; i < n; i++)
			{
				e.Add(new Event(arr[i].Left.X, arr[i].Left.Y, true, i));
				e.Add(new Event(arr[i].Right.X, arr[i].Right.Y, false, i));
			}

			// Sorting all events according to x coordinate.
			e.Sort((e1, e2) => e1.x.CompareTo(e2.x));

			// For storing active segments.
			SortedSet<Event> s = new SortedSet<Event>();
			int ans = 0;
			// Traversing through sorted points
			for (int i = 0; i < 2 * n; i++)
			{
				Event curr = e[i];
				int index = curr.index;

				// If current point is left of its segment
				if (curr.isLeft)
				{
					// Get above and below points
					var next = s.GetViewBetween(curr, curr).Max; // Max value within the view (inclusive)
					var prev = Pred(s, s.GetEnumerator()).Current;
					// Check if current point intersects with
					// any of its adjacent
					bool flag = false;
					if (next != null && doIntersect(arr[next.index], arr[index], ignoreCoincidentEnds))
					{
						string key = (next.index + 1).ToString() + " " + (index + 1).ToString();
						if (!mp.ContainsKey(key))
						{
							mp.Add(key, 1); //if not already checked we can increase count in map
							ans++;
							indicesA.Add(next.index);
							indicesB.Add(index);
						}
					}
					if (prev != null && doIntersect(arr[prev.index], arr[index], ignoreCoincidentEnds))
					{
						string key = (prev.index + 1).ToString() + " " + (index + 1).ToString();
						if (!mp.ContainsKey(key))
						{
							mp.Add(key, 1); //if not already checked we can increase count in map
							ans++;
							indicesA.Add(prev.index);
							indicesB.Add(index);
						}
					}
					// if same line segment is there then decrease answer as it got increased twice
					//if (prev != null && next != null && next.index == prev.index) ans--;
					// TODO this is temporarily suspended

					// Insert current point (or event)
					s.Add(curr);
				}

				// If current point is right of its segment
				else
				{
					// Find the iterator
					var it = s.GetEnumerator();
					while (it.MoveNext())
					{
						if (it.Current.x == arr[index].Left.X && it.Current.y == arr[index].Left.Y && it.Current.isLeft)
							break;
					}

					// Find above and below points
					var next = Succ(s, it).Current;
					var prev = Pred(s, it).Current;

					// If above and below point intersect
					if (next != null && prev != null)
					{
						string key = (next.index + 1).ToString() + " " + (prev.index + 1).ToString();
						string key1 = (prev.index + 1).ToString() + " " + (next.index + 1).ToString();
						if (!mp.ContainsKey(key) && !mp.ContainsKey(key1) && doIntersect(arr[prev.index], arr[next.index], ignoreCoincidentEnds))
						{
							ans++;
							mp.Add(key, 1);
							indicesA.Add(next.index);
							indicesB.Add(prev.index);
						}
					}

					// Remove current segment
					s.Remove(curr);
				}
			}
			//print pair of lines having intersection

			/*
			foreach (var pr in mp)
			{
				Console.WriteLine("Line: " + pr.Key + "\n");
			}
			*/
			return ans;
		}
	}



 
}








