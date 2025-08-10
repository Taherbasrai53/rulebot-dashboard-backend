using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;

namespace rulebot_backend.DAL.Implementation
{
    public class SubscriptionRepository: ISubscriptionRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SubscriptionRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool AddPlanForClient(SubscriptionDto req)
        {
            SqlConnection sqlConn = null;
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var user = _httpContextAccessor.HttpContext?.User;
            var RID = user?.FindFirst("RID")?.Value;

            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
DECLARE @ClientId INT;

SELECT @ClientId = Id
FROM Clients_Data
WHERE RID = @RID;

INSERT INTO Clients_Plans (ClientId, BOTS, StartingDate, EndingDate, Billed)
VALUES (@ClientId, @bots, @startingDate, @endingDate, @billed);";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@RID", RID);
                sqlComm.Parameters.AddWithValue("@bots", req.Bots);
                sqlComm.Parameters.AddWithValue("@startingDate", req.FromDate);
                sqlComm.Parameters.AddWithValue("@endingDate", req.ToDate);
                sqlComm.Parameters.AddWithValue("@billed", req.Billed);

                sqlComm.ExecuteNonQuery();
                return true;
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

        public List<SubscriptionDto> GetAllPlansForClient()
        {
            SqlConnection sqlConn = null;
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var user = _httpContextAccessor.HttpContext?.User;
            var RID = user?.FindFirst("RID")?.Value;

            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
DECLARE @ClientId INT;

SELECT @ClientId = Id
FROM Clients_Data
WHERE RID = @RID;

Select * from Clients_Plans where ClientId= @ClientId";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@RID", RID);

                SqlDataReader reader = sqlComm.ExecuteReader();

                List<SubscriptionDto> plans = new List<SubscriptionDto>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item = new SubscriptionDto();
                        item.Bots = reader.IsDBNull(reader.GetOrdinal("Bots")) ? 0 : reader.GetInt32(reader.GetOrdinal("Bots"));
                        item.FromDate = reader.IsDBNull(reader.GetOrdinal("StartingDate")) ? DateTime.UtcNow : reader.GetDateTime(reader.GetOrdinal("StartingDate"));
                        item.ToDate = reader.IsDBNull(reader.GetOrdinal("EndingDate")) ? DateTime.UtcNow : reader.GetDateTime(reader.GetOrdinal("EndingDate"));
                        item.Billed = reader.IsDBNull(reader.GetOrdinal("Billed")) ? 0 : reader.GetDouble(reader.GetOrdinal("Billed"));
                        plans.Add(item);
                    }
                }

                return plans;
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

        public int GetTodayBots()
        {
            SqlConnection sqlConn = null;
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var user = _httpContextAccessor.HttpContext?.User;
            var RID = user?.FindFirst("RID")?.Value;

            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
DECLARE @ClientId INT;

SELECT @ClientId = Id
FROM Clients_Data
WHERE RID = @RID;

SELECT Sum(bots)
FROM Clients_Plans
WHERE StartingDate <= CAST(GETUTCDATE() AS date)
  AND EndingDate >= CAST(GETUTCDATE() AS date)
  AND ClientId = @ClientId
GROUP BY ClientId;";

                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@RID", RID);

                object result = sqlComm.ExecuteScalar(); 

                int totalBots = (result == DBNull.Value) ? 0 : Convert.ToInt32(result);

                return totalBots;
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
