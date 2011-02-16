using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFLib
{
    class Str
    {
        
        
    }
    static class Conv
    {
        public static DateTime TimeStampToDateTime(long ts)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dt.AddSeconds(ts);
            return dt;
        }
        public static long DateTimeToTimeStamp(DateTime dt)
        {
            DateTime or = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64((dt - or).TotalSeconds);
        }
    }
}
