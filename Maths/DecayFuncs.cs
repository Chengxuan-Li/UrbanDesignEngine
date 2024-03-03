using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Maths
{
    public static class DecayFuncs
    {
        /// <summary>
        /// Gaussian decay function
        /// </summary>
        /// <param name="decayedTarget">Position of 3*Sigma; bigger values result in slower decays</param>
        /// <returns>Gaussian decay function with decay target</returns>
        public static Func<double, double> Gaussian(double decayedTarget)
        {
            return x => Math.Exp(-0.5 * Math.Pow((x) / decayedTarget * 3, 2));
        }

        /// <summary>
        /// Natural decay function 
        /// </summary>
        /// <param name="d">Decay factor; bigger values result in faster decays</param>
        /// <returns>Natural decay function with decay rate</returns>
        public static Func<double, double> Natural(double d)
        {
            return x => Math.Exp(-d * Math.Pow(x, 2));
        }

    }
}
