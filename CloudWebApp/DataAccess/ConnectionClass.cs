using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace DataAccess
{
    public class ConnectionClass
    {
        public NpgsqlConnection MyConnection { get; set; }
        public NpgsqlTransaction MyTransaction { get; set; }

        public ConnectionClass()
        {
            string str = WebConfigurationManager.ConnectionStrings["postgresConnection"].ConnectionString;
            MyConnection = new NpgsqlConnection(str);
        }
    }
}
