﻿using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace sun_position_gha
{
    public class sun_position_ghaInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "sunpositiongha";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                return Properties.Resources.sun2;
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
                return new Guid("72514e2d-24ef-4033-b653-00dfd4473170");
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
