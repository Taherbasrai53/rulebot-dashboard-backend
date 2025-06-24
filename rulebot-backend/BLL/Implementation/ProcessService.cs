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

        public List<ProcessItem> getProcessNames(int userId)
        {
            var connectionString = _userRepo.getClientConnectionString(userId);
            return _procRepo.getProcessNames(connectionString);
        }

        public List<String> getProcessDetails(string processId, int userId)
        {
            var connectionString = _userRepo.getClientConnectionString(userId);
            var xmlPath = _procRepo.getProcessXML(connectionString, processId);

            //get pages from xml logic here
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"GetPages.py \"{xmlPath}\"",
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
