using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Collections;
using Rhino.DocObjects;

namespace UrbanDesignEngine.DataStructure
{
    public class UDEAttributes
    {
        public ObjectAttributes Attributes;
        public double OffsetDistance
        {
            get
            {
                double result;
                if (TryGetDouble("UDEOffsetDistance", out result))
                {
                    return result;
                } else
                {
                    return GlobalSettings.DefaultOffsetDitance;
                }
            }
        }
        public static UDEAttributes FromObjectAttributes(ObjectAttributes attributes)
        {
            return new UDEAttributes { Attributes = attributes };
        }

        public static UDEAttributes FromGuid(Guid guid)
        {
            return new UDEAttributes { Attributes = RhinoDoc.ActiveDoc.Objects.FindId(guid).Attributes };
        }

        public bool TryGetDouble(string key, out double result)
        {
            string val = Attributes.GetUserString(key);
            result = default;
            if (String.IsNullOrEmpty(val))
            {
                return false;
            }
            if (double.TryParse(val, out result))
            {
                return true;
            }
            return false;
        }

        public bool Set(string key, string value)
        {
            return Attributes.SetUserString(key, value);
        }

        public string Get(string key)
        {
            return Attributes.GetUserString(key);
        }
    }
}
