using Microsoft.Data.SqlClient;
using rulebot_backend.DAL.Definition;

namespace rulebot_backend.Business.Implementation
{
    public class UserRepository:IUserRepository
    {
        private readonly IConfiguration _configuration;
        public UserRepository(IConfiguration configuration)
        {
            _configuration = configuration;  
        }
        public String getClientConnectionString(int userId)
        {
            SqlConnection sqlConn = null;
            var connectionString= _configuration.GetConnectionString("DefaultConnection");
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
select ConnectionString from [User] WHERE UserId = @Id
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@Id", userId);
                object result = sqlComm.ExecuteScalar();
                

                return result.ToString();

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
