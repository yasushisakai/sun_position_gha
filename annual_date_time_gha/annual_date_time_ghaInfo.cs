using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace annual_date_time_gha
{
    public class annual_date_time_ghaInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "annualdatetimegha";
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
                return new Guid("0fc8ca6d-0781-4b9c-ba3a-9a6df4b0d9d4");
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
