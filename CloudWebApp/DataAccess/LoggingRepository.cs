using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Diagnostics.AspNet;

namespace DataAccess
{
   public class LoggingRepository
    {
        public static void ReportError(Exception ex)
        {
            var myLogger = GoogleExceptionLogger.Create("programming-for-the-cloud", "pftcLog", "1");
            myLogger.Log(ex);
        }
    }
}
