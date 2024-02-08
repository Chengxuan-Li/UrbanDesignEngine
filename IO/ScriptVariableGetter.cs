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
    /// <summary>
    /// Custom class to get script variables from GH_Goo dataflow and generates error messages where necessary
    /// </summary>
    /// <typeparam name="ScriptVariable">Type of script variable to get</typeparam>
    public class ScriptVariableGetter<ScriptVariable>
    {
        /// <summary>
        /// Get the script varaible from an input stream of GH_Goo
        /// </summary>
        /// <param name="component">The GH component calling this method; use "this"</param>
        /// <param name="da">The DataAccess stream calling this method; use "DA"</param>
        /// <param name="index">The index of the input parameter</param>
        /// <param name="sv">output script variable; default if failed</param>
        /// <returns>True if successfully gets the variable in the correct type; otherwise false</returns>
        public static bool GetScriptVariable(GH_Component component, IGH_DataAccess da, int index, out ScriptVariable sv)
        {
            GHIOParam<ScriptVariable> svGHIO = default;
            sv = default;
            if (!da.GetData(index, ref svGHIO))
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Missing input: " + GlobalSettings.SCPrefix + typeof(ScriptVariable).ToString() + " definition");
                return false;
            }

            if (!svGHIO.GetContent(out sv))
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input error: " + GlobalSettings.SCPrefix + typeof(ScriptVariable).ToString() + " input is of wrong type");
                return false;
            }
            return true;
        }

    }
}
