using Microsoft.Data.SqlClient;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace rulebot_backend.Business.Implementation
{
    public class ProcessRepository: IProcessRepository
    {
        public List<ProcessItem> getProcessNames(string connectionString)
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

                List<ProcessItem > processItems = new List<ProcessItem>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item=new ProcessItem();
                        item.ProcessId = reader.IsDBNull(reader.GetOrdinal("ProcessId")) ? "" : reader.GetGuid(reader.GetOrdinal("ProcessId")).ToString();
                        item.ProcessType = reader.IsDBNull(reader.GetOrdinal("ProcessType")) ? "" : reader.GetString(reader.GetOrdinal("ProcessType"));
                        item.ProcessName = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name"));
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
