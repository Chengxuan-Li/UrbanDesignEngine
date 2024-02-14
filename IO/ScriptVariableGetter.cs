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
        /// <param name="mandatory">true if this input parameter is mandatory</param>
        /// <param name="sv">output script variable; default if failed</param>
        /// <returns>True if successfully gets the variable in the correct type; otherwise false</returns>
        public static VariableGetterStatus GetScriptVariable(GH_Component component, IGH_DataAccess da, int index, bool mandatory, out ScriptVariable sv)
        {
            GHIOParam<ScriptVariable> svGHIO = default;
            sv = default;
            if (!da.GetData(index, ref svGHIO)) // interface results in this being false
            {
                if (mandatory)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Missing input at " + index.ToString() + ":" + GlobalSettings.SCPrefix + typeof(ScriptVariable).ToString() + " definition");
                    return VariableGetterStatus.MissingMandatoryInput;
                }
                return VariableGetterStatus.MissingOptionalInput;
            }

            if (!svGHIO.TryGetContent(out sv))
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input error at " + index.ToString() + ":" + GlobalSettings.SCPrefix + typeof(ScriptVariable).ToString() + " input is of wrong type");
                return VariableGetterStatus.TypeError;
            }
            return VariableGetterStatus.Success;
        }

        public static VariableGetterStatus GetScriptVariableList(GH_Component component, IGH_DataAccess da, int index, bool mandatory, out List<ScriptVariable> svs)
        {
            List<GHIOParam<ScriptVariable>> svGHIOs = new List<GHIOParam<ScriptVariable>>();
            svs = new List<ScriptVariable>();
            if (!da.GetDataList(index, svGHIOs))
            {
                if (mandatory)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Missing input at " + index.ToString() + ":" + GlobalSettings.SCPrefix + typeof(ScriptVariable).ToString() + " definition");
                    return VariableGetterStatus.MissingMandatoryInput;
                }
                return VariableGetterStatus.MissingOptionalInput;
            }
            ScriptVariable sv;
            foreach (var svGHIO in svGHIOs)
            {
                if (!svGHIO.TryGetContent(out sv))
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input error at " + index.ToString() + ":" + GlobalSettings.SCPrefix + typeof(ScriptVariable).ToString() + " input is of wrong type");
                    return VariableGetterStatus.TypeError;
                }
                svs.Add(sv);
            }
            return VariableGetterStatus.Success;
        }
        
        /*
        public static VariableGetterStatus GetOptionalScriptVariable(GH_Component component, IGH_DataAccess da, int index, out ScriptVariable sv)
        {
            GHIOParam<ScriptVariable> svGHIO = default;
            sv = default;
            if (!da.GetData(index, ref svGHIO))
            {
                return VariableGetterStatus.MissingOptionalInput;
            }
            if (!svGHIO.GetContent(out sv))
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input error: " + GlobalSettings.SCPrefix + typeof(ScriptVariable).ToString() + " input is of wrong type");
                return VariableGetterStatus.TypeError;
            }
            return VariableGetterStatus.Success;
        }

        */

    }
}
