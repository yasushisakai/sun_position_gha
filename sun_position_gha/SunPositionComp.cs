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

            pManager.AddTimeParameter("Date", "D", "the date", GH_ParamAccess.item);
            pManager.AddNumberParameter("Latitude", "Lat", "latitude of location (in decimals)", GH_ParamAccess.item, 35.699402); // Tokyo Nikken Sekkei
            pManager.AddNumberParameter("Longitude", "Lon", "longitude of location (in decimals)", GH_ParamAccess.item, 139.751073); // Tokyo Nikken Sekkei
            pManager.AddNumberParameter("Time Zone", "Z", "time zone of location", GH_ParamAccess.item, 9.0d); // Japan
            pManager.AddBooleanParameter("Cull Below Horizon", "H", "cull results if its under the horizon", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.
            pManager.AddNumberParameter("Azimuth", "A", "azimuth of the sun positition (in radians). +Y is North. ", GH_ParamAccess.item);
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

            double latitude = 35.699402;
            double longitude = 139.751073;

            double timezone = 9.0;

            bool is_cull_below_horizon = true;

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref date)) return;
            if (!DA.GetData(1, ref latitude)) return;
            if (!DA.GetData(2, ref longitude)) return;
            if (!DA.GetData(3, ref timezone)) return;
            if (!DA.GetData(4, ref is_cull_below_horizon)) return;

            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:
            double[] sunpos = SunPosition(date, latitude, longitude, timezone);

            // assign results
            // return null if is_cull_below_horizon is true and sunpos[1](elevation) is <0.
            if ((is_cull_below_horizon) && sunpos[1] < 0)
            {
                DA.SetData(0, null);
                DA.SetData(1, null);
            }
            else
            {
                DA.SetData(0, sunpos[0]+Math.PI/2.0);
                DA.SetData(1, sunpos[1]);
            }
        }

        private double[] SunPosition(DateTime _date, double _latitude, double _longitude, double _timezone)
        {
            double[] sunpos = new double[2];

            Int32 day = _date.DayOfYear;

            double hour = _date.Hour + _date.Minute / 60.0 + _date.Second / 3600.0 - _timezone;
            Int32 delta_year = _date.Year - 1949;
            double leap_years = Math.Truncate((double)(delta_year / 4));


            double jullian_date = 32916.5 + delta_year * 365 + leap_years + day + hour / 24.0;
            double time = jullian_date - 51545.0;

            // Ecliptic Coordinates

            double mean_longitude = 280.460 + 0.9856474 * time;
            mean_longitude = mean_longitude % 360;

            double mean_anomaly = 357.528 + 0.9856003 * time;
            mean_anomaly = mean_anomaly % 360;
            mean_anomaly = mean_anomaly < 0.0 ? mean_anomaly + 360 : mean_anomaly;
            mean_anomaly = mean_anomaly * (Math.PI / 180.0);

            double ecliptic_longitude = mean_longitude + 1.915 * Math.Sin(mean_anomaly) + 0.020 * Math.Sin(2.0 * mean_anomaly);
            ecliptic_longitude = ecliptic_longitude % 360;
            ecliptic_longitude = ecliptic_longitude < 0.0 ? ecliptic_longitude + 360 : ecliptic_longitude;
            ecliptic_longitude = mean_longitude * (Math.PI / 180.0);

            double obliquity = (23.439 - 0.0000004 * time) * (Math.PI / 180.0);

            // Celestial Coordinates
            double num = Math.Cos(obliquity) * Math.Sin(ecliptic_longitude);
            double den = Math.Cos(ecliptic_longitude);

            // right ascension 赤経
            double ra = Math.Atan(num / den);

            ra = (den < 0) ? ra + Math.PI : ra;
            ra = ((den >= 0) && (num < 0)) ? ra + Math.PI * 2 : ra;

            // declination: the rise angle of the sun in equatorial coordinate system
            // 赤緯: 赤道座標の赤緯（仰角）
            double declination = Math.Asin(Math.Sin(obliquity) * Math.Sin(ecliptic_longitude)); // 赤緯

            // local coordinates
            double gmst = (6.697375 + 0.0657098242 * time + hour) % 24; // greenwich mean sidereal time
            double lmst = (((gmst + _longitude / 15.0) % 24) * 15.0) * (Math.PI / 180.0);

            // hour angle : the orientation of the sun in equatorial coordinate system
            // 時角: 赤道座標の時角（方位）
            double hour_angle = lmst - ra;

            if (hour_angle < -Math.PI)
            {
                hour_angle += Math.PI * 2;
            }
            else if (hour_angle > Math.PI)
            {
                hour_angle -= Math.PI * 2;
            }

            double latitude = _latitude * (Math.PI / 180.0);

            //　太陽高度
            double elevation = Math.Asin(
              Math.Sin(declination) * Math.Sin(latitude) + Math.Cos(declination) * Math.Cos(latitude) * Math.Cos(hour_angle)
              );

            // 太陽方位
            double azimuth = Math.Asin(
              -1 * Math.Cos(declination) * Math.Sin(hour_angle) / Math.Cos(elevation)
              );

            double zenith_angle = Math.Acos(Math.Sin(latitude) * Math.Sin(declination) + Math.Cos(latitude) * Math.Cos(hour_angle));

            //flags
            bool azi_cos_pos = 0 <= Math.Sin(declination) - Math.Sin(elevation) * Math.Sin(latitude);
            bool azi_sin_neg = Math.Sin(azimuth) < 0;

            if (azi_cos_pos && azi_sin_neg) azimuth += Math.PI * 2;
            if (!azi_cos_pos) azimuth = Math.PI - azimuth;

            sunpos[0] = azimuth;
            sunpos[1] = elevation;

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
                return Properties.Resources.sun2;
                //return null;
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
