using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sun_position_gha
{
    public static class NumericExtensions
    {
        public static double ToRadians(this double val) {
            return (Math.PI / 180) * val;
        }

    }
}
