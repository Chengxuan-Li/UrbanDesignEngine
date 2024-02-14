using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using UrbanDesignEngine.DataStructure;

namespace UrbanDesignEngine.IO
{
    public enum VariableGetterStatus
    {
        MissingMandatoryInput = 0,
        TypeError = 1,
        MissingOptionalInput = 2,
        ValueError = 4,
        Success = 99,
    }

    public class VariableGetter<T>
    {
        public static VariableGetterStatus GetVariable(GH_Component component, IGH_DataAccess da, int index, bool mandatory, out T v)
        {
            return GetVariable(component, da, index, mandatory, x => true, out v);
        }


        public static VariableGetterStatus GetVariable(GH_Component component, IGH_DataAccess da, int index, bool mandatory, Predicate<T> predicate, out T v)
        {
            v = default;
            if (!da.GetData(index, ref v))
            {
                if (mandatory)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Missing input at " + index.ToString() + ":" + GlobalSettings.SCPrefix + typeof(T).ToString() + " definition");
                    return VariableGetterStatus.MissingMandatoryInput;
                }
                return VariableGetterStatus.MissingOptionalInput;
            }
            if (v == null)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input at " + index.ToString() + "is null");
                return VariableGetterStatus.ValueError;
            }
            if (predicate(v))
            {
                return VariableGetterStatus.Success;
            } else
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input at " + index.ToString() + "is error");
                return VariableGetterStatus.ValueError;
            }
            
        }

        public static VariableGetterStatus GetVariableList(GH_Component component, IGH_DataAccess da, int index, bool mandatory, out List<T> vs)
        {
            return GetVariableList(component, da, index, mandatory, x => true, out vs);
        }

        public static VariableGetterStatus GetVariableList(GH_Component component, IGH_DataAccess da, int index, bool mandatory, Predicate<T> predicate, out List<T> vs)
        {
            vs = new List<T>();
            if (!da.GetDataList(index, vs))
            {
                if (mandatory)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Missing input" + index.ToString() + ":" + GlobalSettings.SCPrefix + typeof(T).ToString() + " definition");
                    return VariableGetterStatus.MissingMandatoryInput;
                }
                return VariableGetterStatus.MissingOptionalInput;
            }
            for(int i = 0; i < vs.Count; i++)
            {
                if (vs[i] == null)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input at " + index.ToString() + ", pos " + i.ToString() + " is null");
                    return VariableGetterStatus.ValueError;
                }
                if (!predicate(vs[i]))
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input at " + index.ToString() + ", pos " + i.ToString() + " is error");
                    return VariableGetterStatus.ValueError;
                }
            }
            return VariableGetterStatus.Success;
        }
    }
}
