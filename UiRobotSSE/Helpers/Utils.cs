using Newtonsoft.Json.Schema;
using System.Reflection.Metadata;
using System.Text.Json;
using UiPath.Robot.Api;

namespace UiPathRobotMcpServerOverHttp.Helpers
{
    public class Utils
    {
        public static JsonElement GetInputParamv2(RobotClient client, string key)
        {
            try
            {
                var result = client.InstallProcess(new InstallProcessParameters(key)).Result;
                JSchema param_schema = new JSchema();
                param_schema.Type = JSchemaType.Object;

                // 프로퍼티가 없는 경우에도 유효한 스키마 생성
                if (result.InputArgumentsSchema == null || !result.InputArgumentsSchema.Any())
                {
                    // 빈 객체 스키마 반환
                    var emptySchema = new
                    {
                        type = "object",
                        properties = new { },
                        required = new string[0]
                    };
                    return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(emptySchema));
                }

                foreach (var arg in result.InputArgumentsSchema)
                {
                    var schemaType = Utils.GetJsonSchemaType(arg.Type);
                    if (schemaType != null && schemaType != JSchemaType.None)
                    {
                        param_schema.Properties.Add(arg.Name, new JSchema { Type = schemaType });
                        if (arg.IsRequired)
                        {
                            param_schema.Required.Add(arg.Name);
                        }
                    }
                }

                var schemaJson = param_schema.ToString();
                return JsonSerializer.Deserialize<JsonElement>(schemaJson);
            }
            catch (Exception)
            {
                // MCP 0.4.0 호환 기본 스키마 반환
                var defaultSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = new string[0]
                };
                return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(defaultSchema));
            }
        }
        public static JsonElement GetInputParam(InstallProcessResult result)
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
            return JsonSerializer.Deserialize<JsonElement>(param_schema.ToString());
        }
        public static JSchemaType? GetJsonSchemaType(string? val)
        {
            if (string.IsNullOrEmpty(val))
                return JSchemaType.String; // 기본값으로 string 반환

            string? _type = val.Split(',')[0];
            JSchemaType jtype = JSchemaType.String; // 기본값 설정

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
                case "System.Decimal":
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
                default:
                    // 알 수 없는 타입은 string으로 처리
                    jtype = JSchemaType.String;
                    break;
            }
            return jtype;
        }
    }
}
