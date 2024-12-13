using System.Data;
using System.Data.SqlClient;

namespace ResourceProvisioner.API
{
    class SqlInjection
    {
        private string categoryInput; // Simulated user input
        private string connectionString;

        public SqlInjection(string category, string connString)
        {
            categoryInput = category;
            connectionString = connString;
        }

        public DataSet GetDataSetByCategory()
        {
            // BAD: the category might have SQL special characters in it
            using (var connection = new SqlConnection(connectionString))
            {
                var query1 = "SELECT ITEM,PRICE FROM PRODUCT WHERE ITEM_CATEGORY='"
                  + categoryInput + "' ORDER BY PRICE";
                var adapter = new SqlDataAdapter(query1, connection);
                var result = new DataSet();
                adapter.Fill(result);
                return result;
            }

            // GOOD: use parameters with stored procedures
            using (var connection = new SqlConnection(connectionString))
            {
                var adapter = new SqlDataAdapter("ItemsStoredProcedure", connection);
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                var parameter = new SqlParameter("category", categoryInput);
                adapter.SelectCommand.Parameters.Add(parameter);
                var result = new DataSet();
                adapter.Fill(result);
                return result;
            }

            // GOOD: use parameters with dynamic SQL
            using (var connection = new SqlConnection(connectionString))
            {
                var query2 = "SELECT ITEM,PRICE FROM PRODUCT WHERE ITEM_CATEGORY=@category ORDER BY PRICE";
                var adapter = new SqlDataAdapter(query2, connection);
                var parameter = new SqlParameter("category", categoryInput);
                adapter.SelectCommand.Parameters.Add(parameter);
                var result = new DataSet();
                adapter.Fill(result);
                return result;
            }
        }
    }
}
