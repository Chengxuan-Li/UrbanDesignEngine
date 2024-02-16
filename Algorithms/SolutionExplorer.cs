using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Algorithms
{
    public class SolutionExplorer<S> where S : SolutionState
    {
        public int MaxIterations = 10;
        S initialState;

        public SolutionExplorer(S solutionState)
        {
            if (!solutionState.IsInitiated) solutionState.Initialise();
            initialState = solutionState;
        }

        public bool Solve()
        {
            S state = (S)initialState.Duplicate();
            return Explore(ref state);
        }

        public bool Explore(ref S solutionState)
        {
            int attempt = 0;
            
            S newState;
            while (attempt < MaxIterations)
            {
                newState = (S)solutionState.Propagate();
                if (newState.RuntimeCompliance)
                {
                    if (newState.FinalCompliance)
                    {
                        solutionState = newState;
                        return true;
                    }
                    if (Explore(ref newState))
                    {
                        solutionState = newState;
                        return true;
                    }
                }
                attempt++;
            }
            return false;
        }
    }
}
