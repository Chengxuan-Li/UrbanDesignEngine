using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Learning
{
    public class SolutionExplorer<SG, T> where SG : SolutionStage<SolutionState<T>> where T: ISolvable<T>
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


}
