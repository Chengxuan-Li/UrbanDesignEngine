using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Learning
{
    public interface ISolvable<T> where T: ISolvable<T>
    {
        // TODO
        // Dummy
        T Duplicate();

        T DefaultState { get; }
        T StateParametersInitialise();
    }
}
