using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace UrbanDesignEngine
{
    public class UrbanDesignEngineInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "UrbanDesignEngine";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("4cb7d9a9-42f8-4aed-a3c4-ce6ea58d6126");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
