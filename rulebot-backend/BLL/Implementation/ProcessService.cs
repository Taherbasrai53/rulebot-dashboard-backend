using rulebot_backend.BLL.Definition;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace rulebot_backend.BLL.Implementation
{
    public class ProcessService: IProcessService
    {
        IProcessRepository _procRepo;
        IUserRepository _userRepo;
        public ProcessService(IProcessRepository procRepo, IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _procRepo = procRepo;
        }

        //public List<ProcessItem> getProcessNames(int userId, string connectionString)
        //{
        //    return _procRepo.getProcessNames(connectionString);
        //} 
        
        public LokScreenData getSelectedProcesses(int userId, string tenant_db, string client_db)
        {
            var locked= _procRepo.getSelectedProcesses(tenant_db);
            var all = _procRepo.getProcessNames(client_db);

            var filteredAll = all
                .Where(proc => !locked.Any(s => s.ProcessId.ToLower() == proc.ProcessId.ToLower()))
                .ToList();

            return new LokScreenData
            {
                locked= locked,
                available= filteredAll
            };
        }

        public List<ProcessItem> getSelectedProcessDb(string database, string tenant_db)
        {
            return _procRepo.getSelectedProcessesByDb(tenant_db, database);
        }

        public void addSelectedProcesses(string tenant_db, List<LockedProcess> selectedProcesses, string database)
        {
            _procRepo.addSelectedProcesses(tenant_db, selectedProcesses, database);
        }

        public List<String> getProcessDetails(string processId, int userId, string connectionString)
        {
            var xmlPath = _procRepo.getProcessXML(connectionString, processId);

            //get pages from xml logic here
            var psi = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = $"/opt/rulebot-backend/rulebot-dashboard-backend/rulebot-backend/GetPages.py \"{xmlPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };

            process.Start();
            string error = process.StandardError.ReadToEnd();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
                throw new Exception($"Python error: {error}");

            var result = JsonSerializer.Deserialize<List<string>>(output);

            

            if (File.Exists(xmlPath))
            {
                File.Delete(xmlPath);               
            }

            return result;
        }

    }
}
