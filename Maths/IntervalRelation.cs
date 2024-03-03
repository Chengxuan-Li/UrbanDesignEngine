using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Maths
{
    public enum IntervalRelation
    {
        Disjoint = 0,
        Touches = 1,
        Contains = 2,
        Within = 3,
        Identical = 9,
        Error = 999,
    }
}
