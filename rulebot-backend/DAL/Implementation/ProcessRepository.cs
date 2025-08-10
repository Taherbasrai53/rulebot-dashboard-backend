using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace rulebot_backend.Business.Implementation
{
    public class ProcessRepository: IProcessRepository
    {
        public List<LockedProcess> getProcessNames(string connectionString)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            Select pr.processid as ProcessId, pr.ProcessType as ProcessType, pr.name as Name from BPAProcess as pr
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                

                SqlDataReader reader = sqlComm.ExecuteReader();
                var builder = new SqlConnectionStringBuilder(connectionString);

                string databaseName = builder.InitialCatalog;
                List<LockedProcess > processItems = new List<LockedProcess>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item=new LockedProcess();
                        item.ProcessId = reader.IsDBNull(reader.GetOrdinal("ProcessId")) ? "" : reader.GetGuid(reader.GetOrdinal("ProcessId")).ToString();
                        item.ProcessType = reader.IsDBNull(reader.GetOrdinal("ProcessType")) ? "" : reader.GetString(reader.GetOrdinal("ProcessType"));
                        item.ProcessName = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name"));
                        item.Database = databaseName;
                        item.Rules = 0;
                        processItems.Add(item);
                    }
                }

                return processItems;

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
        public List<LockedProcess> getSelectedProcesses(string connectionString)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            SELECT 
  LP.ProcessId,
  LP.ProcessType,
  LP.Name,
  LP.ProcessType,
  LP.DatabaseName,
  COUNT(R.ProcessId) AS RuleCount
FROM LockedProcesses LP
LEFT JOIN Rules R ON LP.ProcessId = R.ProcessId
GROUP BY 
  LP.ProcessId,
  LP.ProcessType,
  LP.Name,
  LP.ProcessType,
  LP.DatabaseName
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                

                SqlDataReader reader = sqlComm.ExecuteReader();

                List<LockedProcess> processes = new List<LockedProcess>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item=new LockedProcess();
                        item.ProcessId = reader.IsDBNull(reader.GetOrdinal("ProcessId")) ? "" : reader.GetString(reader.GetOrdinal("ProcessId")).ToString();
                        item.ProcessType = reader.IsDBNull(reader.GetOrdinal("ProcessType")) ? "" : reader.GetString(reader.GetOrdinal("ProcessType"));
                        item.ProcessName = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name"));
                        item.Database = reader.IsDBNull(reader.GetOrdinal("DatabaseName")) ? "" : reader.GetString(reader.GetOrdinal("DatabaseName"));
                        item.Rules = reader.IsDBNull(reader.GetOrdinal("RuleCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("RuleCount"));
                        processes.Add(item);
                    }
                }

                return processes;

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

        public List<ProcessItem> getSelectedProcessesByDb(string connectionString, string database)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            select * from LockedProcesses where DatabaseName = @database
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@database", database);


                SqlDataReader reader = sqlComm.ExecuteReader();

                List<ProcessItem> processes = new List<ProcessItem>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item = new ProcessItem();
                        item.ProcessId = reader.IsDBNull(reader.GetOrdinal("ProcessId")) ? "" : reader.GetString(reader.GetOrdinal("ProcessId")).ToString();
                        item.ProcessType = reader.IsDBNull(reader.GetOrdinal("ProcessType")) ? "" : reader.GetString(reader.GetOrdinal("ProcessType"));
                        item.ProcessName = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name"));
                        processes.Add(item);
                    }
                }

                return processes;

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
        public bool addSelectedProcesses(string connectionString, List<LockedProcess> req, string database)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
 BEGIN TRY
    BEGIN TRANSACTION;

    DELETE FROM LockedProcesses;

    WITH ParsedProcesses AS (
        SELECT *
        FROM OPENJSON(@ProcessJson)
        WITH (
            ProcessId UNIQUEIDENTIFIER,
            ProcessType NVARCHAR(100),
            ProcessName NVARCHAR(200)
        )
    )
    INSERT INTO LockedProcesses (ProcessId, ProcessType, Name, DatabaseName)
    SELECT p.ProcessId, p.ProcessType, p.ProcessName, @DatabaseName
    FROM ParsedProcesses p;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH


";
                var json = JsonConvert.SerializeObject(req);
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@ProcessJson", json);
                sqlComm.Parameters.AddWithValue("@DatabaseName", database);

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

        public String getProcessXML(string connectionString, string processId)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();
                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
select processXml from BPAProcess WHERE processid = @Id
";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@Id", processId);
                object result = sqlComm.ExecuteScalar();
                string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"process-{processId}.xml");

                if (result != null && result != DBNull.Value)
                {
                    string xmlContent = result.ToString();

                    string directory = Path.GetDirectoryName(outputFilePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(outputFilePath, xmlContent);
                }
                else
                {
                    throw new Exception("Failed to fetch the xml");
                }

                return outputFilePath;

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

        public void UpdateProcessXml(string connectionString, string processId, string xmlFilePath)
        {
            SqlConnection sqlConn = null;
            try
            {
                // Read XML content from the file
                if (!File.Exists(xmlFilePath))
                {
                    throw new FileNotFoundException("XML file not found", xmlFilePath);
                }

                string xmlContent = File.ReadAllText(xmlFilePath);

                // Open SQL connection and execute update
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();

                SqlCommand sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = @"
            UPDATE BPAProcess 
            SET processXml = @XmlContent 
            WHERE processid = @Id
        ";
                sqlComm.CommandType = System.Data.CommandType.Text;
                sqlComm.Parameters.AddWithValue("@XmlContent", xmlContent);
                sqlComm.Parameters.AddWithValue("@Id", processId);

                int rowsAffected = sqlComm.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception($"No row found with processId = {processId} to update.");
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (sqlConn != null)
                {
                    try { sqlConn.Close(); } catch { }
                }
            }
        }
    }
}
