using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using UrbanDesignEngine.DataStructure;
using UrbanDesignEngine.Tensor;

namespace UrbanDesignEngine.IO
{
    /// <summary>
    /// Custom class to get script variables from GH_Goo dataflow and generates error messages where necessary
    /// </summary>
    public class ScriptVariableGetter
    {
        GH_Component Component;
        IGH_DataAccess DA;
        int Index;
        bool Mandatory;
        

        public static ScriptVariableGetter AllAttributableScriptVariableClassesGetter(GH_Component component, IGH_DataAccess da, int index, bool mandatory)
        {
            return new ScriptVariableGetter()
            {
                Component = component,
                DA = da,
                Index = index,
                Mandatory = mandatory,
            };
        }


        public VariableGetterStatus GetVariableFromAllSimpleTensorFieldTypes(out SimpleTensorField result)
        {
            result = default;
            // Add classes manually ... lol
            if (BaseGet(out SimpleTensorField r0))
            {
                result = r0;
                Component.ClearRuntimeMessages();
                return VariableGetterStatus.Success;
            }

            if (BaseGet(out RadialTensorField r1))
            {
                result = r1;
                Component.ClearRuntimeMessages();
                return VariableGetterStatus.Success;
            }
            if (BaseGet(out LineTensorField r2))
            {
                result = r2;
                Component.ClearRuntimeMessages();
                return VariableGetterStatus.Success;
            }
            if (BaseGet(out PolylineTensorField r3))
            {
                result = r3;
                Component.ClearRuntimeMessages();
                return VariableGetterStatus.Success;
            }
            if (BaseGet(out CurveTensorField r4))
            {
                result = r4;
                Component.ClearRuntimeMessages();
                return VariableGetterStatus.Success;
            }
            //Component.AddRuntimeMessage(Mandatory ? GH_RuntimeMessageLevel.Error : GH_RuntimeMessageLevel.Warning, "Input type error or missing"); // TODO ditinguish warning and error 
            return VariableGetterStatus.TypeError;
        }


        /*
        public VariableGetterStatus GetVariablesListFromAllAttributableTypes(out List<IAttributable> result)
        {
            result = new List<IAttributable>();
            // TODO
        }*/

        public VariableGetterStatus GetVariableFromAllAttributableTypes(out IAttributable result)
        {
            result = default;
            // Add classes manually ... lol
            if (BaseGet(out Attributes r0))
            {
                result = r0;
                Component.ClearRuntimeMessages();
                return VariableGetterStatus.Success;
            }
            
            if (BaseGet(out NetworkNode r1))
            {
                result = r1;
                Component.ClearRuntimeMessages();
                return VariableGetterStatus.Success;
            }
            if (BaseGet(out NetworkEdge r2))
            {
                result = r2;
                Component.ClearRuntimeMessages();
                return VariableGetterStatus.Success;
            }
            if (BaseGet(out NetworkFace r3))
            {
                result = r3;
                Component.ClearRuntimeMessages();
                return VariableGetterStatus.Success;
            }
            if (BaseGet(out NetworkGraph r4))
            {
                result = r4;
                Component.ClearRuntimeMessages();
                return VariableGetterStatus.Success;
            }
            Component.AddRuntimeMessage(Mandatory ? GH_RuntimeMessageLevel.Error : GH_RuntimeMessageLevel.Warning, "Input type error or missing"); // TODO ditinguish warning and error 
            return VariableGetterStatus.TypeError;
        }

        bool BaseGet<ScriptVariable>(out ScriptVariable result)
        {
            GHIOParam<ScriptVariable> svGHIO = default;
            result = default;
            if (DA.GetData(Index, ref svGHIO))
            {
                if (svGHIO.TryGetContent(out result))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Get the script varaible from an input stream of GH_Goo
        /// </summary>
        /// <param name="component">The GH component calling this method; use "this"</param>
        /// <param name="da">The DataAccess stream calling this method; use "DA"</param>
        /// <param name="index">The index of the input parameter</param>
        /// <param name="mandatory">true if this input parameter is mandatory</param>
        /// <param name="sv">output script variable; default if failed</param>
        /// <returns>True if successfully gets the variable in the correct type; otherwise false</returns>
        public static VariableGetterStatus GetScriptVariable<ScriptVariable>(GH_Component component, IGH_DataAccess da, int index, bool mandatory, out ScriptVariable sv)
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

        public static VariableGetterStatus GetScriptVariableList<ScriptVariable>(GH_Component component, IGH_DataAccess da, int index, bool mandatory, out List<ScriptVariable> svs)
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
