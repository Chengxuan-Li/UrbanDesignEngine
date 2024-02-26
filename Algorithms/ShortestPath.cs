using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanDesignEngine.DataStructure;
using QuikGraph.Algorithms.Observers;

namespace UrbanDesignEngine.Algorithms
{
    public class ShortestPath
    {
        public NetworkGraph Graph;
        public Func<NetworkEdge, double> WeightFunc = e => e.UnderlyingCurve.GetLength(); // default weight factor using curve length
        public QuikGraph.Algorithms.ShortestPath.
                UndirectedDijkstraShortestPathAlgorithm<NetworkNode, NetworkEdge> Dijkstra;

        UndirectedVertexPredecessorRecorderObserver
                <NetworkNode, NetworkEdge>
                predecessorObserver;

        UndirectedVertexDistanceRecorderObserver
               <NetworkNode, NetworkEdge>
               distanceObserver;

        IDisposable pOb;
        IDisposable dOb;

        public ShortestPath(NetworkGraph graph)
        {
            Graph = graph;
            Dijkstra = new
            QuikGraph.Algorithms.ShortestPath.
            UndirectedDijkstraShortestPathAlgorithm
            <NetworkNode, NetworkEdge>
            (Graph.Graph, WeightFunc);
        }

        public ShortestPath(NetworkGraph graph, Func<NetworkEdge, double> weightFunc)
        {
            Graph = graph;
            WeightFunc = weightFunc;
            Dijkstra = new
            QuikGraph.Algorithms.ShortestPath.
            UndirectedDijkstraShortestPathAlgorithm
            <NetworkNode, NetworkEdge>
            (Graph.Graph, WeightFunc);
        }

        public void Solve(NetworkNode root)
        {
            // attach a Vertex Predecessor Recorder Observer to give us the paths
            predecessorObserver = new 
            UndirectedVertexPredecessorRecorderObserver
            <NetworkNode, NetworkEdge>();
            pOb = predecessorObserver.Attach(Dijkstra);

            // attach a distance observer to give us the shortest path distances
            distanceObserver = new
            UndirectedVertexDistanceRecorderObserver
            <NetworkNode, NetworkEdge>(WeightFunc);
            dOb = distanceObserver.Attach(Dijkstra);

            Dijkstra.Compute(root);
           
        }

        public void Dispose()
        {
            pOb.Dispose();
            dOb.Dispose();
        }

        public bool PathTo(NetworkNode node, out List<NetworkEdge> path)
        {
            bool result = predecessorObserver.TryGetPath(node, out IEnumerable<NetworkEdge> p);
            path = result? p.ToList() : new List<NetworkEdge>();
            return result;
        }

        public IDictionary<NetworkNode, double> Distances()
        {
            return distanceObserver.Distances;
        }

    }
}
