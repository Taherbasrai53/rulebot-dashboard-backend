using rulebot_backend.BLL.Definition;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using System.IO;
using System.Numerics;

namespace rulebot_backend.BLL.Implementation
{
    public class RuleService:IRuleService
    {
        IRuleRepository _ruleRepo;
        IProcessRepository _procRepo;
        IUserRepository _userRepo;
        public RuleService(IRuleRepository ruleRepo, IProcessRepository processRepository, IUserRepository userRepo)
        {
            _ruleRepo = ruleRepo;
            _procRepo = processRepository;
            _userRepo = userRepo;
        }

        public List<RuleDefinition> GetRuleDefinitions(int userId, int ruleType)
        {
            return _ruleRepo.GetRuleDefinitions(userId, ruleType);
        }

        public bool SaveEditRule(RuleDefinition ruleDefinition, int userId)
        {
            ruleDefinition.UserId = userId;
            return _ruleRepo.AddEditRuleDefinition(ruleDefinition);
        }

        public bool ExecuteRule(List<RuleDefinition> ruleDefinitions, int userId)
        {
            var connectionString= _userRepo.getClientConnectionString(userId);
            Dictionary<string, string> xmlPaths= new Dictionary<string, string>();
            foreach( var def in ruleDefinitions)
            {
                var xmlPath = _procRepo.getProcessXML(connectionString, def.ProcessId);
                xmlPaths[def.ProcessId]= xmlPath;                
            }
            //var scripts = new string[] { "Rule1Exec.py", "Rule2Exec.py", "Rule3Exec.py"};
            foreach (var def in ruleDefinitions)
            {
                ProcessStartInfo psi= new ProcessStartInfo();
                if (def.RuleType == 1)
                {
                    
                    var props = def.Parameters.Split(",");
                    psi = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = $"Rule1Exec.py \"{xmlPaths[def.ProcessId]}\" {int.Parse(props[0])} {int.Parse(props[1])}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else if (def.RuleType == 2)
                {
                    var props = def.Parameters.Split(",");
                    psi = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = $"Rule2Exec.py \"{xmlPaths[def.ProcessId]}\" {int.Parse(props[0])} {int.Parse(props[1])}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else if (def.RuleType == 3)
                {
                    var props = def.Parameters.Split(",");
                    psi = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = $"Rule3Exec.py \"{xmlPaths[def.ProcessId]}\" \"{props[0]}\" \"{props[1]}\" \"{props[2]}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }             

                using var process = new Process { StartInfo = psi };

                process.Start();
                string error = process.StandardError.ReadToEnd();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"Python error: {error}");

                var result = JsonSerializer.Deserialize<List<string>>(output);
                _procRepo.UpdateProcessXml(connectionString, def.ProcessId, result[0]);
            }
            //write execution logic here


            foreach (var path in xmlPaths.Values)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            return true;
        }

        public List<DashboardData> GetDashBoardParams(string processId, string pages, int userId)
        {
            var connectionString = _userRepo.getClientConnectionString(userId);           
            var path = _procRepo.getProcessXML(connectionString, processId);

            var props = _ruleRepo.getPageProps(processId, pages);

            List<DashboardData> res= new List<DashboardData>();
            //write execution logic here
            foreach( var prop in props)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python", 
                    Arguments = $"DashboardDataScript.py \"{path}\" \"{prop.Page}\" {prop.Height} {prop.Width}",
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

                var result = JsonSerializer.Deserialize<List<int>>(output);

                if (result == null || result.Count != 4)
                    throw new Exception("Unexpected output from Python script.");

                var dashboardData= new DashboardData();
                dashboardData.Page = prop.Page;
                dashboardData.StageCompliantNum=result[0];
                dashboardData.StageUnCompliantNum=result[1];
                dashboardData.DataItemCompliantNum=result[2];
                dashboardData.DataItemUnCompliantNum=result[3];

                res.Add(dashboardData);
            }            


            if (File.Exists(path))
            {
                File.Delete(path);
            }
            

            return res;
        }
    }
}
