using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;
using System.Diagnostics;

namespace rulebot_backend.DAL.Implementation
{
    public class RuleRepository:IRuleRepository
    {
        IConfiguration _configuration { get; set; }
        public RuleRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public List<RuleDefinition> GetRuleDefinitions(int userId, int ruleType)
        {
            SqlConnection sqlConn = null;
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            Select * from Rules where UserId=@Id and RuleType=@RuleType; 
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@Id", userId);
                sqlComm.Parameters.AddWithValue("@RuleType", ruleType);

                SqlDataReader reader = sqlComm.ExecuteReader();

                List<RuleDefinition> rules = new List<RuleDefinition>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item = new RuleDefinition();
                        item.Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id"));
                        item.ProcessId = reader.IsDBNull(reader.GetOrdinal("ProcessId")) ? "" : reader.GetString(reader.GetOrdinal("ProcessId"));
                        item.UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId"));
                        item.Pages = reader.IsDBNull(reader.GetOrdinal("Pages")) ? "" : reader.GetString(reader.GetOrdinal("Pages"));
                        item.Stage = reader.IsDBNull(reader.GetOrdinal("Stage")) ? "" : reader.GetString(reader.GetOrdinal("Stage"));
                        item.Parameters = reader.IsDBNull(reader.GetOrdinal("Parameters")) ? "" : reader.GetString(reader.GetOrdinal("Parameters"));
                        item.RuleType = reader.IsDBNull(reader.GetOrdinal("RuleType")) ? 0 : reader.GetInt32(reader.GetOrdinal("RuleType"));
                        rules.Add(item);
                    }
                }

                return rules;

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

        public bool AddEditRuleDefinition(RuleDefinition def)
        {
            SqlConnection sqlConn = null;
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            IF @Id = 0
BEGIN
    INSERT INTO Rules (ProcessId, UserId, Pages, Stage, Parameters, RuleType)
    VALUES (@ProcessId, @UserId, @Pages, @Stage, @Parameters, @RuleType);
END
ELSE
BEGIN
    UPDATE Rules
    SET ProcessId = @ProcessId,
        UserId = @UserId,
        Pages = @Pages,
        Stage = @Stage,
        Parameters = @Parameters
    WHERE Id = @Id;
END
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@Id", def.Id);
                sqlComm.Parameters.AddWithValue("@UserId", def.UserId);
                sqlComm.Parameters.AddWithValue("@ProcessId", def.ProcessId);
                sqlComm.Parameters.AddWithValue("@Pages", def.Pages);
                sqlComm.Parameters.AddWithValue("@Stage", def.Stage);
                sqlComm.Parameters.AddWithValue("@Parameters", def.Parameters);
                sqlComm.Parameters.AddWithValue("@RuleType", def.RuleType);                

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

        public List<PageProps> getPageProps(string processId, string pages)
        {
            SqlConnection sqlConn = null;
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            WITH SplitPages AS (
    SELECT TRIM(value) AS Page
    FROM STRING_SPLIT(@Pages, ',')
),
RankedRules AS (
    SELECT 
        p.Page AS Page,
        r.Parameters,
        r.RuleType,
        CAST(PARSENAME(REPLACE(r.Parameters, ',', '.'), 2) AS INT) AS Height,
        CAST(PARSENAME(REPLACE(r.Parameters, ',', '.'), 1) AS INT) AS Width,
        ROW_NUMBER() OVER (
            PARTITION BY p.Page ORDER BY r.RuleType
        ) AS RowNum
    FROM SplitPages p
    JOIN Rules r ON r.ProcessId = @ProcessId
    WHERE r.RuleType IN (1, 2, 3)
      AND ',' + r.Pages + ',' LIKE '%,' + p.Page + ',%'
)
SELECT Page, Height, Width
FROM RankedRules
WHERE RowNum = 1;

";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@ProcessId", processId);
                sqlComm.Parameters.AddWithValue("@Pages", pages);

                SqlDataReader reader = sqlComm.ExecuteReader();

                List<PageProps> props = new List<PageProps>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item = new PageProps();
                        item.Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? 0 : reader.GetInt32(reader.GetOrdinal("Height"));
                        item.Page = reader.IsDBNull(reader.GetOrdinal("Page")) ? "" : reader.GetString(reader.GetOrdinal("Page"));
                        item.Width = reader.IsDBNull(reader.GetOrdinal("Width")) ? 0 : reader.GetInt32(reader.GetOrdinal("Width"));

                        props.Add(item);
                    }
                }

                return props;

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
