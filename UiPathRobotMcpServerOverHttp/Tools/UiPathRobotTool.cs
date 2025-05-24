using Microsoft.VisualBasic;
using ModelContextProtocol.Server;
using System.ComponentModel;
using UiPath.Robot.Api;
using PTST.UiPath.Orchestrator.API;
using PTST.UiPath.Orchestrator.Models;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UiPath.Robot.MCP.Tools;

[McpServerToolType]
public sealed class UiPathRobotTool
{
 
    [McpServerTool, Description("Get installed process list")]
    public static async Task<string> GetProcessList(
        RobotClient client) 
    {
#if DEBUG
        //Debugger.Launch();
#endif
        var processes =  await client.GetProcesses();
        if( processes == null || processes.Count == 0)
        {
            return "No processes found in robot.";
        }
        else
        {
            return JsonConvert.SerializeObject(JArray.FromObject(processes.Select( p => new { Name = p.Name, Description = p.Description, Key = p.Key})));
        }

    }

    [McpServerTool, Description("Get specific process input argument for invocation")] 
    public static string GetProcessInputParameter(
        RobotClient client,
        [Description("Process Key to get process input argument")] string processKey)   
    {
#if DEBUG
        //Debugger.Launch();
#endif
        var helper = RobotHelper.getRobotHelper();
        var release = helper.findProcessWithKey(processKey);
        if( release == null)
        {
            return "No processes found in specified folders.";
        }
        else        
        {
            var inputArguments = release.Arguments.Input;
            return helper.ConvertToParameter(inputArguments);
        }   
    }


    [McpServerTool, Description("Invoke process with given arguments")]
    public static async Task<string> InvokeProcess(
        RobotClient client,
        [Description("Process Key to invoke")] string processKey,
        [Description("Input Arguments")] Dictionary<string, object> inputArguments)
    {
#if DEBUG
        //Debugger.Launch();
#endif
        var helper = RobotHelper.getRobotHelper();
        var process = client.GetProcesses().Result.Where( p => p.Key.ToString() == processKey).FirstOrDefault();
        if( process == null)
        {
            return "No processes found in specified folders.";
        }
        else
        {
            var job = process.ToJob();
            foreach(var k in inputArguments.Keys)
            { 
                var v = (JToken)inputArguments[k];
                switch( v.Type)
                {
                    case JTokenType.String:
                        job.InputArguments[k] = v.ToString();
                        break;
                    case JTokenType.Integer:
                        job.InputArguments[k] = (int)v;
                        break;
                    case JTokenType.Float:
                        job.InputArguments[k] = (double)v;
                        break;
                    case JTokenType.Array:
                        job.InputArguments[k] = v.ToObject<List<string>>();
                        break;
                    case JTokenType.Boolean:
                        job.InputArguments[k] = (bool)v ;
                        break;
                }
            }
            var result = await helper.getRobotClient().RunJob(job);
            return JsonConvert.SerializeObject(result.Arguments);
        }
    }
   

}
