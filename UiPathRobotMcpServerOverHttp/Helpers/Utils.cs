using Newtonsoft.Json.Schema;
using System.Reflection.Metadata;
using System.Text.Json;
using UiPath.Robot.Api;

namespace UiPathRobotMcpServerOverHttp.Helpers
{
    public class Utils
    {
        public static  JsonElement GetInputParamv2(RobotClient client, string key)
        {
            try
            {
                var result = client.InstallProcess(new InstallProcessParameters(key)).Result;
                JSchema param_schema = new JSchema();
                param_schema.Type = JSchemaType.Object;
                foreach (var arg in result.InputArgumentsSchema)
                {
                    param_schema.Properties.Add(arg.Name, new JSchema { Type = Utils.GetJsonSchemaType(arg.Type) });
                    if (arg.IsRequired)
                    {
                        param_schema.Required.Add(arg.Name);
                    }
                }
                return JsonSerializer.Deserialize<JsonElement>(param_schema.ToString());
            }
            catch (Exception ex)
            {
                return JsonSerializer.Deserialize<JsonElement>("{}");
            }
        }
        public static JsonElement GetInputParam( InstallProcessResult result)
        {
            JSchema param_schema = new JSchema();
            param_schema.Type = JSchemaType.Object;
            foreach (var arg in result.InputArgumentsSchema)
            {
                param_schema.Properties.Add(arg.Name, new JSchema { Type = Utils.GetJsonSchemaType(arg.Type) });
                if (arg.IsRequired)
                {
                    param_schema.Required.Add(arg.Name);
                }
            }
            return JsonSerializer.Deserialize < JsonElement > (param_schema.ToString());
        }
        public static JSchemaType? GetJsonSchemaType(string? val)
        {
            string? _type = val?.Split(',')[0];
            JSchemaType? jtype = JSchemaType.None;
            switch (_type)
            {
                case "System.String":
                case "System.DateTime":
                case "System.Guid":
                    jtype = JSchemaType.String;
                    break;
                case "System.Int64":
                case "System.UInt64":
                case "System.Int32":
                case "System.UInt32":
                case "System.Int16":
                case "System.UInt16":
                case "System.Byte":
                    jtype = JSchemaType.Integer;
                    break;
                case "System.Single":
                case "System.Double":
                    jtype = JSchemaType.Number;
                    break;
                case "System.Boolean":
                    jtype = JSchemaType.Boolean;
                    break;
                case "System.Object":
                    jtype = JSchemaType.Object;
                    break;
                case "System.Array":
                    jtype = JSchemaType.Array;
                    break;
            }
            return jtype;
        }
    }
}
