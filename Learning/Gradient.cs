using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UrbanDesignEngine.Learning
{
    public class GradientLearning
    {
        public double LearningRate = 0.1;
        public int MaxIterations = 100;
        public double epsilon = 0.01;
        public List<double> Xs;
        public bool Descent;

        public Predicate<List<double>> compliance = xs => true;
        public Func<List<double>, double> TargetFunction = d =>
        {
            d = d.ConvertAll(x => -Math.Pow(x, 2));
            return d.Sum();
        };

        public static List<double> Gradient(List<double> xs, Func<List<double>, double> targetFunction, double finiteDifference)
        {
            List<double> result = new List<double>();
            List<double> gradientAdjustedXs;
            for (int i = 0; i < xs.Count; i++)
            {
                gradientAdjustedXs = xs.ToList();
                gradientAdjustedXs[i] += finiteDifference;
                double f_positiveFiniteDifference = targetFunction.Invoke(gradientAdjustedXs);
                gradientAdjustedXs[i] -= finiteDifference;
                double f_negativeFiniteDifference = targetFunction.Invoke(gradientAdjustedXs);
                result.Add((f_positiveFiniteDifference - f_negativeFiniteDifference) / 2 / finiteDifference);
            }
            return result;
        }

        public static double GradientScalar(List<double> gradient)
        {
            List<double> gs = new List<double>();
            gradient.ForEach(g => gs.Add(g * g));
            return Math.Sqrt(gs.Sum());
        }

        public GradientLearning(List<double> initialXs, Func<List<double>, double> targetFunc, bool descent)
        {
            Xs = initialXs;
            Descent = descent;
        }


        public void Learn()
        {
            int iterations = 0;
            List<double> gradient;
            List<double> newXs;
            while (iterations < MaxIterations)
            {
                newXs = new List<double>();
                gradient = Gradient(Xs, TargetFunction, epsilon);
                if (GradientScalar(gradient) <= epsilon)
                {
                    return;
                }
                gradient = gradient.ConvertAll(g => g * LearningRate * (Descent ? -1.0 : 1.0));
                for (int i = 0; i < gradient.Count; i++)
                {
                    newXs.Add(Xs[i] + gradient[i]);
                }
                if (compliance.Invoke(newXs))
                {
                    Xs = newXs.ToList();
                } else
                {
                    return;
                }
                iterations += 1;
            }
            return;
        }
    }
}

