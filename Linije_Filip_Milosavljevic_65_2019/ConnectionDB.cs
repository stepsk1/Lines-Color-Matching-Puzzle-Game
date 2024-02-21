using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linije_Filip_Milosavljevic_65_2019
{
    public class ConnectionDB
    {
        private static readonly object kljuc = new object();

        private static SqlConnection connection = null;

        private ConnectionDB()
        {
            connection = new SqlConnection(@"Data Source=(local);Initial Catalog=linije;Integrated Security=True;");
        }

        public static SqlConnection Connect()
        {
            if (connection == null)
                lock (kljuc)
                    if (connection == null)
                        new ConnectionDB();

            return connection;
        }
    }
}
