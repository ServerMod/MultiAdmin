using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin
{
    class Utils
    {
        public static String GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH_mm");
        }

        // this is a legacy method since there is no proper unix time method before net framework 4.6 :(
        public static long GetUnixTime()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            return (long)t.TotalSeconds;
        }
    }
}
