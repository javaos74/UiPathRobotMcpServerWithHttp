﻿using Microsoft.VisualBasic;
using ModelContextProtocol.Server;
using System.ComponentModel;
using UiPath.Robot.Api;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using UiPathRobotMcpServerOverHttp.Helpers;

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
            return "No processes were found.";
        }
        else
        {
            return JsonConvert.SerializeObject(JArray.FromObject(
                processes.Select(p => new { Name = p.Name, Description = p.Description, Key = p.Key })));
        }

    }

    [McpServerTool, Description("Get specific process input argument for invocation")] 
    public static string GetProcessInputParameter(
        RobotClient client,
        [Description("Process Key to get process input argument")] string processKey)   
    {
        /* expected json schema for input arguments
        {
            "type": "object",
            "properties": {
                "UserPrompt": {
                    "type": "string"
                },
                "Age" : {
                    "type" : "number"
                }
            },
            "required": []
        } */
#if DEBUG
        //Debugger.Launch();
#endif
        JSchema param_schema = new JSchema();
        param_schema.Type = JSchemaType.Object;
        var result = client.InstallProcess(new InstallProcessParameters(processKey)).Result;
        foreach( var arg in result.InputArgumentsSchema)
        {
            param_schema.Properties.Add(arg.Name, new JSchema { Type = Utils.GetJsonSchemaType( arg.Type) });
            if( arg.IsRequired)
            {
                param_schema.Required.Add(arg.Name);
            }   
        }

        return param_schema.ToString();
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
        var process = client.GetProcesses().Result.Where( p => p.Key.ToString() == processKey).FirstOrDefault();
        if( process == null)
        {
            return "No processes were found.";
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
            var result = await client.RunJob(job);
            return JsonConvert.SerializeObject(result.Arguments);
        }
    }
   
    
}
