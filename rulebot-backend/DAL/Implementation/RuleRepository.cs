using Microsoft.AspNetCore.Components.Forms;
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
        public List<RuleDefinition> GetRuleDefinitions(string database, int ruleType, string connectionString)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            Select * from Rules where DatabaseName=@database and RuleType=@RuleType; 
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@database", database);
                sqlComm.Parameters.AddWithValue("@RuleType", ruleType);

                SqlDataReader reader = sqlComm.ExecuteReader();

                List<RuleDefinition> rules = new List<RuleDefinition>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item = new RuleDefinition();
                        item.Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id"));
                        item.ProcessId = reader.IsDBNull(reader.GetOrdinal("ProcessId")) ? "" : reader.GetString(reader.GetOrdinal("ProcessId"));                        item.Pages = reader.IsDBNull(reader.GetOrdinal("Pages")) ? "" : reader.GetString(reader.GetOrdinal("Pages"));
                        item.Stage = reader.IsDBNull(reader.GetOrdinal("Stage")) ? "" : reader.GetString(reader.GetOrdinal("Stage"));
                        item.Parameters = reader.IsDBNull(reader.GetOrdinal("Parameters")) ? "" : reader.GetString(reader.GetOrdinal("Parameters"));
                        item.RuleType = reader.IsDBNull(reader.GetOrdinal("RuleType")) ? 0 : reader.GetInt32(reader.GetOrdinal("RuleType"));
                        item.Variables = reader.IsDBNull(reader.GetOrdinal("Variables")) ? null : reader.GetString(reader.GetOrdinal("Variables"));
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

        public bool AddEditRuleDefinition(RuleDefinition def, string connectionString)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            IF @Id = 0
BEGIN
    INSERT INTO Rules (ProcessId, Pages, Stage, Parameters, RuleType, Variables, DatabaseName)
    VALUES (@ProcessId, @Pages, @Stage, @Parameters, @RuleType, @Variables, @database);

    Update LockedProcesses set IsLocked=1 where ProcessId=@ProcessId;
END
ELSE
BEGIN
    UPDATE Rules
    SET ProcessId = @ProcessId,
        Pages = @Pages,
        Stage = @Stage,
        Parameters = @Parameters,
        Variables= @Variables
    WHERE Id = @Id;
END
";
                if (def.RuleType== 1 && (def.Variables==null || def.Variables=="" ))
                {
                    def.Variables = "Data,Collection";
                    var paras= def.Parameters.Split(',');
                    paras[2] = "2";
                    paras[3] = "4";
                    def.Parameters = string.Join(",", paras);
                }
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@Id", def.Id);
                sqlComm.Parameters.AddWithValue("@ProcessId", def.ProcessId);
                sqlComm.Parameters.AddWithValue("@Pages", def.Pages);
                sqlComm.Parameters.AddWithValue("@Stage", def.Stage);
                sqlComm.Parameters.AddWithValue("@Parameters", def.Parameters);
                sqlComm.Parameters.AddWithValue("@RuleType", def.RuleType);
                sqlComm.Parameters.AddWithValue("@Variables", def.Variables==null? DBNull.Value:def.Variables);
                sqlComm.Parameters.AddWithValue("@database", def.database);
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

        public bool DeleteRuleDefinition(int id, string connectionString)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            Delete from Rules where Id= @Id
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@Id", id);
                
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
        public List<PageProps> getPageProps(string processId, string pages, string connectionString)
        {
            SqlConnection sqlConn = null;
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
        CAST(PARSENAME(REPLACE(r.Parameters, ',', '.'), 2) AS INT) AS VariableHeight,
        CAST(PARSENAME(REPLACE(r.Parameters, ',', '.'), 1) AS INT) AS VariableWidth,
        CAST(PARSENAME(REPLACE(r.Parameters, ',', '.'), 3) AS INT) AS Height,
        CAST(PARSENAME(REPLACE(r.Parameters, ',', '.'), 4) AS INT) AS Width,
        ROW_NUMBER() OVER (
            PARTITION BY p.Page ORDER BY r.RuleType
        ) AS RowNum
    FROM SplitPages p
    JOIN Rules r ON r.ProcessId = @ProcessId
    WHERE r.RuleType IN (1, 2, 3)
      AND ',' + r.Pages + ',' LIKE '%,' + p.Page + ',%'
)
SELECT Page, Height, Width, VariableHeight, VariableWidth
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
                        item.VarHeight = reader.IsDBNull(reader.GetOrdinal("VariableHeight")) ? 0 : reader.GetInt32(reader.GetOrdinal("VariableHeight"));
                        item.VarWidth = reader.IsDBNull(reader.GetOrdinal("VariableWidth")) ? 0 : reader.GetInt32(reader.GetOrdinal("VariableWidth"));
                        //item.Variables = reader.IsDBNull(reader.GetOrdinal("Variables")) ? null : reader.GetString(reader.GetOrdinal("Variables"));
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

        public List<String> getRulePages(string processId, int ruleType, int ruleId, string connectionString)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            ;WITH AllPages AS (
                SELECT 
                    TRIM(value) AS Page
                FROM Rules
                CROSS APPLY STRING_SPLIT(Pages, ',')
                WHERE ProcessId = @ProcessId AND RuleType = @RuleType AND Id <> @RuleId
            )
            SELECT DISTINCT Page
            FROM AllPages
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@ProcessId", processId);
                sqlComm.Parameters.AddWithValue("@RuleType", ruleType);
                sqlComm.Parameters.AddWithValue("@RuleId", ruleId);

                SqlDataReader reader = sqlComm.ExecuteReader();

                List<String> pages = new List<String>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item = reader.IsDBNull(reader.GetOrdinal("Page")) ? "" : reader.GetString(reader.GetOrdinal("Page"));
                        pages.Add(item);
                    }
                }

                return pages;

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
