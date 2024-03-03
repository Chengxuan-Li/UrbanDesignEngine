using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Maths
{
    public class Distribution
    {
        protected double s;
        public double Sigma => s;

        protected double m;
        public double Mu => m;

        protected Func<double, double> pdf;

        protected Func<double, double> decay;

        protected Distribution(double sigma, double mu)
        {
            s = sigma;
            m = mu;
        }

        public static Distribution Gaussian(double sigma, double mu)
        {
            Distribution d = new Distribution(sigma, mu);
            d.pdf = x => Math.Exp(-0.5 * Math.Pow((x - d.Mu) / d.Sigma, 2)) / d.Sigma / 2.50662827463; // sqrt(2*pi) is simplified as 2.50662827463
            d.decay = x => Math.Exp(-0.5 * Math.Pow((x - d.Mu) / d.Sigma, 2));
            return d;
        }

        public static Distribution GaussianDecay(double decayedTarget, double mu)
        {
            return Gaussian(decayedTarget / 3, mu); // assume the 3-sigma rule (99.7%)
        }

        public double Decay(double x)
        {
            return decay.Invoke(x);
        }

        public double PDF(double x)
        {
            return pdf.Invoke(x);
        }
    }

}
