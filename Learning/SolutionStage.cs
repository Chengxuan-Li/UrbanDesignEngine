using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Learning
{
    public class SolutionStage<T> where T : ISolvable<T>
    {
        public SolutionStageParameter SolutionStageParameter = SolutionStageParameter.Default;

        public SolutionStageContinuity SolutionStageContinuity => SolutionStageParameter.SolutionStageContinuity;

        public FitnessLearningMethod FitnessLearningMethod => SolutionStageParameter.FitnessLearningMethod;

        public SearchingPattern SearchPatter => SolutionStageParameter.SearchingPattern;

        public int MaxIterations => SolutionStageParameter.MaxIterations;

        public int NumberOfDuplicatedSingles => SolutionStageParameter.NumDuplicate;

        public Dictionary<SolutionStage<T>, Predicate<SolutionState<T>>> StageTransferRules;

        public List<Action<SolutionState<T>>> CurrentPropagationActions;

        public List<Predicate<SolutionState<T>>> CurrentRuntimeComplianceCheckers;

        public List<Predicate<SolutionState<T>>> StageFinalComplianceChechers;

        public List<SolutionState<T>> SolutionStates = new List<SolutionState<T>>();

        public SolutionStage<T> NextStage(SolutionState<T> state)
        {
            SolutionStage<T> nextStage = null;
            foreach (var stagePredicatePair in StageTransferRules)
            {
                if (stagePredicatePair.Value.Invoke(state))
                {
                    nextStage = stagePredicatePair.Key;
                    break;
                }
            }
            if (nextStage != null)
            {
                nextStage.assignStateToStageAsNext(state);
            }
            return nextStage;
        }

        /// <summary>
        /// The constructor only for SolutionStages as a NextStage
        /// In this case no preset SolutionState is given and will
        /// be assigned during runtime.
        /// Thus, this instance will not, by definition, support
        /// MultipleRandom search pattern and if supplied, it will
        /// use a SingleDuplicateRandom search pattern instead.
        /// </summary>
        /// <param name="parameter"></param>
        public SolutionStage(SolutionStageParameter parameter)
        {
            SolutionStageParameter = parameter;
            if (parameter.SearchingPattern == SearchingPattern.MultipleRandom)
            {
                parameter.SearchingPattern = SearchingPattern.SingleDuplicatedRandom;
            }
        }

        /// <summary>
        /// This method should only be used when creating and initialising a NextStage instance
        /// </summary>
        /// <param name="state"></param>
        protected void assignStateToStageAsNext(SolutionState<T> state)
        {
            SolutionStates = new List<SolutionState<T>>();
            state.StageInstanceReference = this;
            if (SolutionStageParameter.SearchingPattern == SearchingPattern.SingleDuplicatedRandom)
            {
                SolutionStates.Add(state);
                for (int i = 0; i < NumberOfDuplicatedSingles - 1; i++)
                {
                    SolutionStates.Add(state.Duplicate());
                }
            }
            else
            {
                SolutionStates.Add(state);
            }
        }

        public SolutionStage(List<SolutionState<T>> states, SolutionStageParameter parameter)
        {
            SolutionStageParameter = parameter;
            parameter.SearchingPattern = SearchingPattern.MultipleRandom;
            foreach (SolutionState<T> state in states)
            {
                state.StageInstanceReference = this;
                SolutionStates.Add(state);
            }
        }

        public SolutionStage(SolutionState<T> state, SolutionStageParameter parameter)
        {
            SolutionStageParameter = parameter;
            state.StageInstanceReference = this;
            if (SolutionStageParameter.SearchingPattern == SearchingPattern.SingleDuplicatedRandom)
            {
                SolutionStates.Add(state);
                for (int i = 0; i < NumberOfDuplicatedSingles - 1; i++)
                {
                    SolutionStates.Add(state.Duplicate());
                }
            } else if (SolutionStageParameter.SearchingPattern == SearchingPattern.SingleRandom)
            {
                SolutionStates.Add(state);
            } else if (SolutionStageParameter.SearchingPattern == SearchingPattern.MultipleRandom)
            {
                SolutionStates.Add(state);
                for (int i = 0; i < NumberOfDuplicatedSingles - 1; i++)
                {
                    SolutionState<T> st = state.Duplicate();
                    st.StateInitialise();
                    SolutionStates.Add(st);
                }
            }
        }

        /// <summary>
        /// Randomise every state
        /// </summary>
        public void Initialise()
        {
            foreach (SolutionState<T> state in SolutionStates)
            {
                state.StateInitialise();
            }
        }

        public SolutionStageSolverStatus Solve()
        {
            foreach (SolutionState<T> state in SolutionStates)
            {
                state.Propagate()
            }
        }

        public SolutionStageSolverStatus SolutionStateSearch(ref SolutionState<T> state)
        {
            int attempt = 0;

            SolutionState<T> newState;
            while (attempt < MaxIterations)
            {
                newState = state.Propagate();
                if (newState.RuntimeCompliance)
                {
                    // if this propagated state is compliant with both runtime and stage reqs,
                    if (newState.StageFinalCompliance)
                    {
                        // if this is a final stage, then a valid solution state becomes a final solution
                        if (SolutionStageContinuity == SolutionStageContinuity.Final)
                        {
                            state = newState;
                            return SolutionStageSolverStatus.ValidFinalSolution;
                        }
                        // if this is a stage-and-continue, then a valid solution stage becomes a
                        // stage-specific valid solution and will not be changed
                        if (SolutionStageContinuity == SolutionStageContinuity.StageAndContinue)
                        {
                            state = newState;
                            var nextStage = NextStage(state);
                            if (nextStage == null)
                            {
                                return SolutionStageSolverStatus.InvalidFinalOrStagedSolutionDueToNoNextStage;
                            }
                            // freeze and stage the state outcome and carry on
                            if (nextStage.SolutionStateSearch(ref state) != SolutionStageSolverStatus.ValidFinalSolution)
                            {
                                return SolutionStageSolverStatus.StageSpecificSolution;
                            } else
                            {
                                return SolutionStageSolverStatus.ValidFinalSolution;
                            }
                        }
                        // if this is a nonstop stage, then solution search continues to the next stage
                        if (SolutionStageContinuity == SolutionStageContinuity.NonStopContinue)
                        {
                            // state = newState;
                            var nextStage = NextStage(newState);
                            if (nextStage == null)
                            {
                                state = newState;
                                return SolutionStageSolverStatus.InvalidFinalOrStagedSolutionDueToNoNextStage;
                            }
                            if (nextStage.SolutionStateSearch(ref newState) == SolutionStageSolverStatus.ValidFinalSolution)
                            {
                                state = newState;
                                return SolutionStageSolverStatus.ValidFinalSolution;
                            }
                        }
                    }
                    // keep searching in this stage until reaching a solution that is staged, final, or invalid final
                    SolutionStageSolverStatus status = SolutionStateSearch(ref newState);
                    if (status != SolutionStageSolverStatus.ExceededIterationLimit)
                    {
                        state = newState;
                        return status;
                    }

                }
                attempt++;
            }
            return SolutionStageSolverStatus.ExceededIterationLimit;
        }

    }

    public enum SolutionStageContinuity
    {
        Final = 0, // The end of all solution stages, i.e. the end of a solution exploration
        StageAndContinue = 1, // The end fo a solution stage. All solution states in and before this stage are freezed. Future deadends will not affect this solution stage
        NonStopContinue = 2, // The end of a solution stage; future deadends would change or revert solution states in this stage
    }

    public enum FitnessLearningMethod
    {
        None = 0,
        GradientAscent = 1000,
        GradientDecent = 1001,
        SimulatedAnnealing = 2000, // TODO
    }

    public enum SearchingPattern
    {
        SingleRandom = 1000,
        SingleDuplicatedRandom = 1100,
        MultipleRandom = 9000,
        // TODO
    }

    public enum SolutionStageSolverStatus
    {
        ExceededIterationLimit = 1000, // No valid solution given before the maximum number of iterations have been reached
        StageSpecificSolution = 9000, // which means it is not a valid final solution, cuz otherwise it will be 9100
        ValidFinalSolution = 9100, // valid final solution, could be achieved with continue-mode stages AND stage-mode stages
        InvalidFinalOrStagedSolutionDueToNoNextStage = 4040,

    }
    
    public class SolutionStageParameter
    {
        public static SolutionStageParameter Default => new SolutionStageParameter();

        public int MaxIterations = 10;

        public int NumDuplicate = 20;

        public SolutionStageContinuity SolutionStageContinuity = SolutionStageContinuity.NonStopContinue;

        public FitnessLearningMethod FitnessLearningMethod = FitnessLearningMethod.GradientAscent;

        public SearchingPattern SearchingPattern = SearchingPattern.MultipleRandom;
    }
}
