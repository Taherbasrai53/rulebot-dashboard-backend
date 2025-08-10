using Microsoft.Data.SqlClient;
using rulebot_backend.DAL.Definition;

namespace rulebot_backend.DAL.Implementation
{
    public class ConnectionRepository: IConnectionRepository
    {
        private readonly IConfiguration _configuration;
        public ConnectionRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Boolean checkConnectionString(string connectionString)
        {
            SqlConnection sqlConn = null;

            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
select 1 from BPAProcess
";
                sqlComm.CommandType = System.Data.CommandType.Text;

                SqlDataReader reader = sqlComm.ExecuteReader();

                List<String> pages = new List<String>();
                if (reader.HasRows)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (sqlConn != null)
                {
                    try
                    {
                        sqlConn.Close();
                    }
                    catch { }
                }
            }
        }
    }
}
