using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Algorithms
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

    public abstract class SolutionState
    {
        public abstract double FitnessScore { get; }

        public abstract bool IsInitiated { get; }

        protected List<Action<SolutionState>> actions { get; }

        public void AddAction(Action<SolutionState> action)
        {
            actions.Add(action);
        }

        protected List<Predicate<SolutionState>> runtimeCompliance { get; }

        public void AddRuntimeCompliance(Predicate<SolutionState> predicate)
        {
            runtimeCompliance.Add(predicate);
        }

        protected List<Predicate<SolutionState>> finalCompliance { get; }

        public void AddFinalCompliance(Predicate<SolutionState> predicate)
        {
            finalCompliance.Add(predicate);
        }

        public bool RuntimeCompliance
        {
            get
            {
                foreach(Predicate<SolutionState> predicate in runtimeCompliance)
                {
                    if (predicate.Invoke(this))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool FinalCompliance
        {
            get
            {
                foreach (Predicate<SolutionState> predicate in finalCompliance)
                {
                    if (predicate.Invoke(this))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public abstract void Initialise();

        public SolutionState Propagate()
        {
            SolutionState newState = Duplicate();
            foreach (Action<SolutionState> action in actions)
            {
                action.Invoke(newState);
            }
            return newState;
        }

        public abstract SolutionState Duplicate();
    }
}
