using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace annual_date_time_gha
{
    public class AnnualDateTime : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public AnnualDateTime()
            : base("Annual Date Time", "Annual",
                "Datetime propagator",
                "NikkenDDL", "solar")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddIntegerParameter("Year", "Y", "the year", GH_ParamAccess.item, 2015);
            pManager.AddIntegerParameter("Start Hour", "S", "the start hour (values are between 0-23)", GH_ParamAccess.item, 6);
            pManager.AddIntegerParameter("End Hour", "E", "the end hour (values are between 0-23) end hour should not exceed start hour", GH_ParamAccess.item, 21);

            pManager.AddIntegerParameter("Interval", "I", "Intervals within hour range.", GH_ParamAccess.item, 1);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTimeParameter("Date times", "D", "date times", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // declaration
            Int32 year=2015, start_hour=0, end_hour=24, interval=1;
            List<DateTime> datetimes;

            if (!DA.GetData(0, ref year)) return;
            if (!DA.GetData(1, ref start_hour)) return;
            if (!DA.GetData(2, ref end_hour)) return;
            if (!DA.GetData(3, ref interval)) return;

            datetimes = getAnnualDateTime(year,start_hour,end_hour,interval);

            DA.SetDataList(0, datetimes);

        }


        private List<DateTime> getAnnualDateTime(Int32 _year, Int32 _start_hour, Int32 _end_hour, Int32 _interval ) { 
            Int32 year_days = DateTime.IsLeapYear(_year) ? 366 : 365;
            Int32 datetime_daily = ((_end_hour - _start_hour) / _interval) +1;

            //DateTime[] datetimes = new DateTime [year_days * datetime_daily];
            List<DateTime> datetimes = new List<DateTime>();

            DateTime temp_date = new DateTime(_year,1,1,_start_hour,0,0);
            DateTime temp_datetime;

            for (int i = 0; i < year_days; i++) {
                temp_datetime = temp_date;
                for(int j=0;j<datetime_daily;j++){
                    datetimes.Add(temp_datetime);
                    temp_datetime = temp_datetime.AddHours((double)_interval);
                }
                temp_date = temp_date.AddDays(1.0);
            }

                return datetimes;
        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{7beda220-4d03-4aee-89f4-607348815131}"); }
        }
    }
}
