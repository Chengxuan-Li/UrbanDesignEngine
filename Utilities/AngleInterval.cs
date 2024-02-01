using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Utilities
{
    public class AngleInterval
    {

        public double Start;
        public double End;

        public AngleInterval(double a, double b)
        {
            Start = (a < b) ? a : b;
            End = (a < b) ? b : a;
        }

    }
}
