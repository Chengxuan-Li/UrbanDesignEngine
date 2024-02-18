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

        public string Name = "Unnamed Solution Stage";

        public SolutionStageContinuity SolutionStageContinuity => SolutionStageParameter.SolutionStageContinuity;

        public FitnessLearningMethod FitnessLearningMethod => SolutionStageParameter.FitnessLearningMethod;

        public SearchingPattern SearchingPattern => SolutionStageParameter.SearchingPattern;


        public int MaxIterations => SolutionStageParameter.MaxIterations;

        public int NumberOfDuplicatedSingles => SolutionStageParameter.NumDuplicate;

        public Dictionary<SolutionStage<T>, Predicate<SolutionState<T>>> StageTransferRules = new Dictionary<SolutionStage<T>, Predicate<SolutionState<T>>>();

        public List<Action<SolutionState<T>>> CurrentPropagationActions = new List<Action<SolutionState<T>>>();

        public List<Predicate<SolutionState<T>>> CurrentRuntimeComplianceCheckers = new List<Predicate<SolutionState<T>>>();

        public List<Predicate<SolutionState<T>>> StageFinalComplianceChechers = new List<Predicate<SolutionState<T>>>();

        public List<SolutionState<T>> SolutionStates = new List<SolutionState<T>>();

        public SolutionStage<T> NextStage(SolutionState<T> state)
        {
            SolutionStage<T> nextStage = null;
            foreach (var stagePredicatePair in StageTransferRules)
            {
                if (stagePredicatePair.Value.Invoke(state))
                {
                    nextStage = stagePredicatePair.Key.DuplicateSettings();
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
        /// NOTE 20240218: EVERYTING IS NOW SINGLE - TODO in future
        /// </summary>
        /// <param name="parameter"></param>
        public SolutionStage(SolutionStageParameter parameter)
        {
            SolutionStageParameter = parameter;
            if (parameter.SearchingPattern == SearchingPattern.MultipleRandom)
            {
                // parameter.SearchingPattern = SearchingPattern.SingleDuplicatedRandom; // TODO add this functionality
                parameter.SearchingPattern = SearchingPattern.SingleRandom;
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

        public List<SolutionStageSolverStatus> Solve()
        {
            if (SearchingPattern == SearchingPattern.SingleRandom)
            {
                SolutionState<T> state = SolutionStates[0];
                SolutionStageSolverStatus status = SolutionStateSearch(ref state);
                SolutionStates[0] = state;
                return new List<SolutionStageSolverStatus> { status };
            } else
            {
                List<SolutionStageSolverStatus> result = new List<SolutionStageSolverStatus>();
                for (int i = 0; i < SolutionStates.Count; i++)
                {
                    SolutionState<T> state = SolutionStates[i];
                    SolutionStageSolverStatus status = SolutionStateSearch(ref state);
                    SolutionStates[i] = state;
                    result.Add(status);
                }
                return result;
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

        public SolutionStage<T> DuplicateSettings()
        {
            SolutionStage<T> dup = new SolutionStage<T>(SolutionStageParameter);
            dup.CurrentPropagationActions = CurrentPropagationActions;
            dup.CurrentRuntimeComplianceCheckers = CurrentRuntimeComplianceCheckers;
            dup.StageFinalComplianceChechers = StageFinalComplianceChechers;
            dup.Name = Name;
            dup.StageTransferRules = StageTransferRules;
            return dup;
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


    public class SolutionStageUnitTest
    {

        public SolutionStageUnitTest()
        {

            SolvableUnitTest sut = new SolvableUnitTest();
            SolutionState<SolvableUnitTest> state = new SolutionState<SolvableUnitTest>(sut);
            var sge = new SolutionStage<SolvableUnitTest>(state, new SolutionStageParameter() { SolutionStageContinuity = SolutionStageContinuity.StageAndContinue, SearchingPattern = SearchingPattern.SingleDuplicatedRandom });
            sge.Name = "SG1";
            sge.CurrentPropagationActions.Add(x => x.TargetObject.P1_AddRandomInt.Invoke(x.TargetObject));
            sge.CurrentRuntimeComplianceCheckers.Add(x => x.TargetObject.P1_Runtime.Invoke(x.TargetObject));
            sge.StageFinalComplianceChechers.Add(x => x.TargetObject.P1_Final.Invoke(x.TargetObject));

            var sg1 = new SolutionStage<SolvableUnitTest>(state, new SolutionStageParameter() { SolutionStageContinuity = SolutionStageContinuity.StageAndContinue, SearchingPattern = SearchingPattern.SingleDuplicatedRandom });
            sg1.Name = "SG1";
            sg1.CurrentPropagationActions.Add(x => x.TargetObject.P1_AddRandomInt.Invoke(x.TargetObject));
            sg1.CurrentRuntimeComplianceCheckers.Add(x => x.TargetObject.P1_Runtime.Invoke(x.TargetObject));
            sg1.StageFinalComplianceChechers.Add(x => x.TargetObject.P1_Final.Invoke(x.TargetObject));


            var sg2 = new SolutionStage<SolvableUnitTest>(new SolutionStageParameter() { SolutionStageContinuity = SolutionStageContinuity.StageAndContinue, SearchingPattern = SearchingPattern.SingleRandom });
            sg2.Name = "SG2";
            sg2.CurrentPropagationActions.Add(x => x.TargetObject.P2_Add2ToRandomItemExceptBounds.Invoke(x.TargetObject));
            sg2.CurrentRuntimeComplianceCheckers.Add(x => x.TargetObject.P2_Runtime.Invoke(x.TargetObject));
            sg2.StageFinalComplianceChechers.Add(x => x.TargetObject.P2_Final.Invoke(x.TargetObject));

            var sgf = new SolutionStage<SolvableUnitTest>(new SolutionStageParameter() { SolutionStageContinuity = SolutionStageContinuity.Final, SearchingPattern = SearchingPattern.SingleRandom });
            sgf.Name = "SGF";
            sgf.CurrentPropagationActions.Add(x => x.TargetObject.P1_AddRandomInt.Invoke(x.TargetObject));
            sgf.CurrentRuntimeComplianceCheckers.Add(x => x.TargetObject.P1_Runtime.Invoke(x.TargetObject));
            sgf.StageFinalComplianceChechers.Add(x => x.TargetObject.P1_Final.Invoke(x.TargetObject));



            sg1.StageTransferRules.Add(sg1, sut.ToSG1);
            sg1.StageTransferRules.Add(sg2, sut.ToSG2);
            sg1.StageTransferRules.Add(sgf, sut.ToSGF);
            sg2.StageTransferRules.Add(sg1, sut.ToSG1);
            sg2.StageTransferRules.Add(sg2, sut.ToSG2);
            sg2.StageTransferRules.Add(sgf, sut.ToSGF);



        }

    }

    public class SolvableUnitTest : ISolvable<SolvableUnitTest>
    {

        public List<int> values = new List<int> {0, 100};
        public int CountEven
        {
            get
            {
                int i = 0;
                values.ForEach(v => i += (v % 2 == 0) ? 1 : 0);
                return i;
            }
        }

        public int MinGap
        {
            get
            {
                values.Sort();
                int gap = 1000;
                for (int i = 0; i < values.Count - 1; i ++)
                {
                    gap = Math.Min(values[i + 1] - values[i], gap);
                }
                return gap;
            }
        }

        public int MinGapBefore;

        public int CountEvenBefore;
        
        public int CountDivisibleBy4
        {
            get
            {
                int i = 0;
                values.ForEach(v => i += (v % 4 == 0) ? 1 : 0);
                return i;
            }
        }
        public Action<SolvableUnitTest> P1_AddRandomInt => s =>
        {
            Random random = new Random();
            MinGapBefore = MinGap;
            int i = 0;
            while (s.values.Contains(i))
            {
                i = random.Next(1, 100);
            }
            s.values.Add(i);
            s.values.Sort();
        };

        public Action<SolvableUnitTest> P2_Add2ToRandomItemExceptBounds => s =>
        {
            Random random = new Random();
            int c = s.values.Count;
            s.CountEvenBefore = s.CountEven;
            int i = 1;
            if (c > 2)
            {
                i = random.Next(1, c - 1);
                s.values[i] += 2;
            }
        };

        public Predicate<SolvableUnitTest> P1_Runtime => s =>
        {
            return s.CountEven == s.values.Count;
        };

        public Predicate<SolvableUnitTest> P2_Runtime => s =>
        {
            return s.CountDivisibleBy4 == s.values.Count;
        };

        public Predicate<SolvableUnitTest> P1_Final => s =>
        {
            if (s.MinGapBefore >= 10)
            {
                return s.MinGap <= s.MinGapBefore / 2;
            } else
            {
                return s.MinGap <= 4;
            }
        };

        public Predicate<SolvableUnitTest> P2_Final => s =>
        {
            s.values.Sort();
            for (int i = 0; i < s.values.Count - 1; i++)
            {
                if (s.values[i] == s.values[i + 1])
                {
                    return false;
                }
            }
            return true;
        };

        public Predicate<SolutionState<SolvableUnitTest>> ToSG1 => s => s.StageInstanceReference.Name == "SG2";

        public Predicate<SolutionState<SolvableUnitTest>> ToSG2 => s => s.StageInstanceReference.Name == "SG1" && s.TargetObject.MinGap > 4;

        public Predicate<SolutionState<SolvableUnitTest>> ToSGF => s => s.TargetObject.MinGap <= 4;


        public SolvableUnitTest Duplicate()
        {
            var sut = new SolvableUnitTest();
            sut.values = values.ToList();
            return sut;

        }

        public SolvableUnitTest StateParametersInitialise()
        {
            throw new NotImplementedException();
        }
    }
}
