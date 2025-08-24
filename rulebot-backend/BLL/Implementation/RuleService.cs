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

        public List<RuleDefinition> GetRuleDefinitions(string database, int ruleType, string connectionString)
        {
            return _ruleRepo.GetRuleDefinitions(database, ruleType, connectionString);
        }

        public bool SaveEditRule(RuleDefinition ruleDefinition, int userId, string connectionString)
        {
            
            ruleDefinition.UserId = userId;
            if(ValidatePages(ruleDefinition, connectionString))
            {
                return false;
            }

            return _ruleRepo.AddEditRuleDefinition(ruleDefinition, connectionString);
        }

        private bool ValidatePages(RuleDefinition ruleDefinition, string connectionString)
        {
            var pages = _ruleRepo.getRulePages(ruleDefinition.ProcessId, ruleDefinition.RuleType, ruleDefinition.Id, connectionString);
            var intersect= ruleDefinition.Pages.Split(',').Intersect(pages);

            return intersect.Any();
        }
        public bool DeleteRule(int id, string connectionString)
        {            
            return _ruleRepo.DeleteRuleDefinition(id, connectionString);
        }

        public Dictionary<string, string> ExecuteRule(List<RuleDefinition> ruleDefinitions, int userId, string connectionString)
        {
            Dictionary<string, string> xmlPaths= new Dictionary<string, string>();
            Dictionary<string, string> results = new();

            foreach ( var def in ruleDefinitions)
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
                    var stages= def.Stage.Split(',');
                    var pages= def.Pages.Split(',');
                    var props = def.Parameters.Split(",");
                    var variables = string.IsNullOrWhiteSpace(def.Variables)
                            ? new[] { "Data", "Collection" }
                            : def.Variables.Split(",");

                    var stageProps = stages
                            .Select(stage => $"('{stage}', {props[0]}, {props[1]})");

                    var variableProps = variables
                        .Select(variable => $"('{variable}', {(props.Length > 2 ? int.Parse(props[2]) : 2)}, {(props.Length > 3 ? int.Parse(props[3]) : 4)})");

                    var allTuples = string.Join(", ", stageProps.Concat(variableProps));
                    var formattedArgs = $"\"{xmlPaths[def.ProcessId]}\" \"{def.Pages}\" \"[{allTuples}]\"";

                    psi = new ProcessStartInfo
                    {
                        FileName = "python3",
                        Arguments = $"/opt/rulebot-backend/rulebot-dashboard-backend/rulebot-backend/Rule1Exec.py {formattedArgs}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else if (def.RuleType == 2)
                {
                   
                    var props = def.Parameters.Split(",");
                    
                    var formattedArgs = $"\"{xmlPaths[def.ProcessId]}\" \"{def.Pages}\" \"[({props[0]}, {props[1]})]\"";

                    psi = new ProcessStartInfo
                    {
                        FileName = "python3",
                        Arguments = $"/opt/rulebot-backend/rulebot-dashboard-backend/rulebot-backend/Rule2Exec.py {formattedArgs}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else if (def.RuleType == 3)
                {
                    var rule3Params = JsonSerializer.Deserialize<List<Rule3Parameter>>(def.Parameters);

                    var propsFormatted = rule3Params
    .Select(p =>
        $"('{p.blockNamePrefix}', '{p.dataItemPrefix}', '{p.colorGradient}')")
    .ToList();

                    var propsArgument = $"[{string.Join(", ", propsFormatted)}]";

                    var formattedArgs = $"\"{xmlPaths[def.ProcessId]}\" \"{def.Pages}\" \"{propsArgument}\"";


                    psi = new ProcessStartInfo
                    {
                        FileName = "python3",
                        Arguments = $"/opt/rulebot-backend/rulebot-dashboard-backend/rulebot-backend/Rule3Exec.py {formattedArgs}",
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
                if (!File.Exists(result[0]))
                {
                    throw new FileNotFoundException("XML file not found", result[0]);
                }

                string xmlContent = File.ReadAllText(result[0]);
                results[def.ProcessId] = xmlContent;
                if (File.Exists(result[0]))
                {
                    File.Delete(result[0]);
                }
            }

            foreach (var path in xmlPaths.Values)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            return results;
        }

        public List<DashboardData> GetDashBoardParams(string processId, string pages, int userId, string client_db, string tenant_db)
        {
            var path = _procRepo.getProcessXML(client_db, processId);

            var props = _ruleRepo.getPageProps(processId, pages, tenant_db);

            List<DashboardData> res= new List<DashboardData>();
            //write execution logic here
            foreach( var prop in props)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python3", 
                    Arguments = $"/opt/rulebot-backend/rulebot-dashboard-backend/rulebot-backend/DashboardDataScript.py \"{path}\" \"{prop.Page}\" {prop.Height} {prop.Width} {prop.VarHeight} {prop.VarWidth}",
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
                dashboardData.StageUnCompliantNum = result[1];
                dashboardData.DataItemCompliantNum=result[2];
                dashboardData.DataItemUnCompliantNum = result[3];

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
