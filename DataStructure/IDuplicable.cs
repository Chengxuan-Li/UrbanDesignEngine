using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.DataStructure
{
    public interface IDuplicable<T> where T: IDuplicable<T>
    {
        T Duplicate();
    }

    public interface IDuplicableComponent<T, G> where T: IDuplicableComponent<T, G>
    {
        T Duplicate(G newG);
    }
}
