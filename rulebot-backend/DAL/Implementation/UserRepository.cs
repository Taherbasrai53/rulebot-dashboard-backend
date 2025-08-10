using Microsoft.Data.SqlClient;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;
using System.Reflection.PortableExecutable;

namespace rulebot_backend.Business.Implementation
{
    public class UserRepository:IUserRepository
    {
        private readonly IConfiguration _configuration;
        public UserRepository(IConfiguration configuration)
        {
            _configuration = configuration;  
        }

        public User ValidateLogin(LoginDto req, string connectionString)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
select * from [Users] WHERE Username = @username AND Password = @password
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@username", req.Username);
                sqlComm.Parameters.AddWithValue("@password", req.Password);

                SqlDataReader reader = sqlComm.ExecuteReader();
                User user = null;  
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        user = new User();
                        user.Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id"));
                        user.UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? "" : reader.GetString(reader.GetOrdinal("UserName"));
                        user.Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? "" : reader.GetString(reader.GetOrdinal("Password"));
                        user.PermissionJson = reader.IsDBNull(reader.GetOrdinal("Permissions")) ? "" : reader.GetString(reader.GetOrdinal("Permissions"));
                    }
                }
                return user;
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
        public String getClientConnectionString(string RID)
        {
            SqlConnection sqlConn = null;
            var connectionString= _configuration.GetConnectionString("DefaultConnection");
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
select ClientDatabase from Clients_Data WHERE RID = @rid
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@rid", RID);
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
