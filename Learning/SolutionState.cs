using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Learning
{
    /*
    public interface ISolutionState
    {
        double FitnessScore { get; }
        bool IsInitiated { get; }
        bool RuntimeCompliance { get; }
        bool FinalCompliance { get; }
        void Initialise();
        ISolutionState Propagate();
    }*/

    public class SolutionState<T> where T : ISolvable<T>
    {
        public SolutionState(T target)
        {
            TargetObject = target.Duplicate();
        }

        public SolutionState<T> Duplicate()
        {
            T duplicatedTarget = TargetObject.Duplicate();
            SolutionState<T> duplicated = new SolutionState<T> (duplicatedTarget)
            {
                StageInstanceReference = StageInstanceReference, // ref!
                AllPropagationActions = AllPropagationActions, // ref!
                AllRuntimeComplianceCheckers = AllRuntimeComplianceCheckers, // ref!

            };
            return duplicated;
        }
        
        public T TargetObject;

        public SolutionStage<T> StageInstanceReference;

        public List<Action<SolutionState<T>>> AllPropagationActions;

        public List<Predicate<SolutionState<T>>> AllRuntimeComplianceCheckers;

        public bool RuntimeCompliance
        {
            get
            {
                foreach(Predicate<SolutionState<T>> predicate in StageInstanceReference.CurrentRuntimeComplianceCheckers)
                {
                    if (!predicate.Invoke(this))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool StageFinalCompliance
        {
            get
            {
                foreach(Predicate<SolutionState<T>> predicate in StageInstanceReference.StageFinalComplianceChechers)
                {
                    if (!predicate.Invoke(this))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void StateInitialise()
        {
            TargetObject.StateParametersInitialise();
        }

        public SolutionState<T> Propagate()
        {
            SolutionState<T> newState = Duplicate();
            foreach (Action<SolutionState<T>> action in newState.StageInstanceReference.CurrentPropagationActions)
            {
                action.Invoke(newState);
            }
            return newState;
        }


    }

}
}
