using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace sun_position_gha
{
    public class SunPositionComp : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SunPositionComp()
            : base("Sun Position", "SunPos",
                "returns the sun position in a given time and location",
                "NikkenDDL", "solar")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Use the pManager object to register your input parameters.

            pManager.AddTimeParameter("Date", "Date", "the date", GH_ParamAccess.item);
            pManager.AddTimeParameter("Time", "T", "the time", GH_ParamAccess.item);
            pManager.AddNumberParameter("Latitude", "Lat", "latitude of location (in decimals)", GH_ParamAccess.item, 35.699402f); // Tokyo Nikken Sekkei
            pManager.AddNumberParameter("Longitude", "Lon", "longitude of location (in decimals)", GH_ParamAccess.item, 139.751073f); // Tokyo Nikken Sekkei
            pManager.AddNumberParameter("Time Zone", "Z", "time zone of location", GH_ParamAccess.item, 9.0f); // Japan
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.
            pManager.AddNumberParameter("Azimuth","A","azimuth of the sun positition (in radians)",GH_ParamAccess.item);
            pManager.AddNumberParameter("Elevation", "E", "elevation of the sun positition (in radians)", GH_ParamAccess.item);
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // variable declaration
            DateTime date = DateTime.Now;
            DateTime time = DateTime.Now;

            float latitude = 35.699402f;
            float longitude = 139.751073f;

            float timezone = 9.0f;

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref date)) return;
            if (!DA.GetData(1, ref time)) return;
            if (!DA.GetData(2, ref latitude)) return;
            if (!DA.GetData(3, ref longitude)) return;
            if (!DA.GetData(4, ref timezone)) return;

            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:
            double[] sunpos = SunPosition(date,time,latitude,longitude,timezone);

            // Finally assign the spiral to the output parameter.
            DA.SetData(0, sunpos[0]);
            DA.SetData(1, sunpos[1]);
        }

        private double[] SunPosition(DateTime _date, DateTime _time, double _latitude, double _longitude, double _timezone)
        {
            double[] sunpos = new double[2];

            Int32 day = _date.DayOfYear;

            double hour = _time.Hour + _time.Minute / 60.0d + _time.Second / 3600.0d - _timezone;
            Int32 delta_year = _date.Year - 1949;
            double leap_years = Math.Truncate(delta_year/4.0d);

            double jullian_date = 32916.5d + delta_year * 365 + leap_years + day + hour / 24d;
            double time = jullian_date - 51545.0;

            // Ecliptic coordination
            double mean_longitude = 280.460d + 0.9856474d * time;
            mean_longitude %= 360.0d;

            double mean_anomaly = 357.528d + 0.9856003d * time;
            mean_anomaly %= 360.0d;
            mean_anomaly = mean_anomaly.ToRadians();

            double ecliptic_longitude = mean_longitude + 1.915d * Math.Sin(mean_anomaly) + 0.020d * Math.Sin(2.0d * mean_anomaly);
            ecliptic_longitude %= 360.0d;
            ecliptic_longitude = mean_longitude.ToRadians();

            double obliquity = (23.439d - 0.0000004d * time).ToRadians();


            return sunpos;
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
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
            get { return new Guid("{8330cfbf-a3f9-42b6-8754-98a07eb79562}"); }
        }
    }
}
