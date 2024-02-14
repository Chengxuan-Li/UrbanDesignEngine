using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.DataStructure
{
    public interface IAttributable
    {
        Attributes GetAttributesInstance();
        void SetAttribute(string key, object val);
        T GetAttribute<T>(string key);
        bool TryGetAttribute<T>(string key, out T val);
    }
}
