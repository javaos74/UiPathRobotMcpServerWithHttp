using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Diagnostics.Metrics;
using System.Text.Json;
using UiPath.Robot.Api;

namespace UiPathRobotMcpServerOverHttp.Helpers
{
    public class UiPathRobotToolHandler
    {
        private RobotClient client = new RobotClient();
        private ListToolsResult toollist = new ListToolsResult();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _workerTask;
        private McpServer? _server;


        private void Start(int intervalseconds)
        {
            if (_workerTask != null && !_workerTask.IsCompleted)
            {
                Console.WriteLine("Worker is already running.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            _workerTask = Task.Run(async () =>
            {
#if DEBUG
                Console.WriteLine($"Periodic worker started with interval {intervalseconds}s.");
#endif
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // 주기적으로 실행될 작업
                        updateToolList();

                        // 다음 주기를 기다림
                        await Task.Delay(intervalseconds * 1000, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        // 취소 요청 시 루프 종료
                        Console.WriteLine("Periodic worker cancelled.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred in periodic worker: {ex.Message}");
                        // 오류 발생 시에도 계속 실행하려면 break 제거
                    }
                }
#if DEBUG
                Console.WriteLine("Periodic worker stopped.");
#endif 
            }, cancellationToken);
        }

        private void Stop()
        {
            if (_workerTask != null && !_workerTask.IsCompleted)
            {
                _cancellationTokenSource?.Cancel();
                // 작업이 완료될 때까지 기다릴 수도 있습니다 (옵션)
                // _workerTask.Wait();
                Console.WriteLine("Cancellation requested for periodic worker.");
            }
        }

        public UiPathRobotToolHandler(int intervalseconds = 60 * 5) // default 5 minutes
        {
            Start(intervalseconds);
        }

        public void SetServer(McpServer server)
        {
            _server = server;
        }

        private void updateToolList()
        {
#if DEBUG
            Console.WriteLine($"Updating process list... Current time: {DateTime.Now}");
#endif
            try
            {
                // 락 외부에서 도구 목록 생성 (시간이 오래 걸리는 작업)
                var processes = client.GetProcesses().Result;
                var tools = new List<Tool>();

                foreach (var p in processes)
                {
                    try
                    {
                        // 각 프로세스 처리에 타임아웃 적용
                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var inputSchemaTask = Task.Run(() => Utils.GetInputParamv2(client, p.Key), cts.Token);

                        var inputSchema = inputSchemaTask.Result;
                        tools.Add(new Tool
                        {
                            Name = p.Name,
                            Description = p.Description ?? "UiPath Robot Process",
                            InputSchema = inputSchema
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing tool {p.Name}: {ex.Message}");
                        // 오류가 발생한 도구는 기본 스키마로 추가
                        var defaultSchema = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(
                            System.Text.Json.JsonSerializer.Serialize(new
                            {
                                type = "object",
                                properties = new { },
                                required = new string[0]
                            }));

                        tools.Add(new Tool
                        {
                            Name = p.Name,
                            Description = p.Description ?? "UiPath Robot Process",
                            InputSchema = defaultSchema
                        });
                    }
                }

                // 락 내부에서는 빠른 할당만 수행
                _lock.EnterWriteLock();
                try
                {
                    toollist.Tools = tools;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                // 알림 전송 (락 외부에서)
                if (_server != null)
                {
                    try
                    {
                        _server.SendNotificationAsync("notifications/tools/list_changed").Wait(TimeSpan.FromSeconds(5));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending notification: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating tool list: {ex.Message}");
            }
        }
        public async ValueTask<ListToolsResult> UiPathRobotListHandler(ListToolsRequestParams request, CancellationToken token)
        {
            // 타임아웃이 있는 읽기 락 시도
            if (_lock.TryEnterReadLock(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    return await ValueTask.FromResult(toollist);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            else
            {
                // 락 획득 실패 시 빈 목록 반환
                Console.WriteLine("Failed to acquire read lock for tools list");
                return await ValueTask.FromResult(new ListToolsResult { Tools = new List<Tool>() });
            }
        }

        private async Task reportProgress(McpServer? server, string? token, Task task)
        {
            if (server == null || string.IsNullOrEmpty(token))
                return;

            int progress = 0;
            int total = 100;
#if DEBUG
            Console.WriteLine($"report status : progressToken={token}");
#endif
            try
            {
                while (!task.IsCompleted)
                {
                    var progressStatus = new
                    {
                        Progress = progress < 99 ? progress++ : 99,
                        Total = total,
                        progressToken = token
                    };
#if DEBUG
                    Console.WriteLine( progressStatus.ToString());
#endif
                    
                    // MCP 0.4.0 progress notification 형식
                    await server.SendNotificationAsync("notifications/progress", progressStatus);

                    await Task.Delay(2000); // 2초 간격으로 업데이트
                }

                // 완료 시 100% 전송
                await server.SendNotificationAsync("notifications/progress", new
                {
                    Progress = 100,
                    Total = total,
                    progressToken = token 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending progress notification: {ex.Message}");
            }
        }

        public async ValueTask<CallToolResult> UiPathRobotToolCallHandler(CallToolRequestParams request, CancellationToken token)
        {
            var progressToken = request.Meta?["progressToken"]?.ToString();
  
            var process = client.GetProcesses().Result.Where(p => p.Name == request.Name).FirstOrDefault();
            if (process == null)
            {
                return await ValueTask.FromResult(new CallToolResult()
                {
                    Content = [new TextContentBlock { Text = "No process found..." }]
                });
            }
            var job = process.ToJob();

            if (request.Arguments != null)
            {
                foreach (var k in request.Arguments.Keys)
                {
                    var v = (JsonElement)request.Arguments[k];
                    switch (v.ValueKind)
                    {
                        case JsonValueKind.String:
                            job.InputArguments[k] = v.ToString();
                            break;
                        case JsonValueKind.Number:
                            Decimal dec;
                            long lng;
                            if (v.TryGetInt64(out lng))
                                job.InputArguments[k] = lng;
                            else if (v.TryGetDecimal(out dec))
                                job.InputArguments[k] = dec;
                            break;
                        //case JsonValueKind.Object:
                        //    job.InputArguments[k] = v.Deserialize(Dictionary<String,Object>);
                        //    break;
                        //                    case JsonValueKind.Array:
                        //                        job.InputArguments[k] = v.ToObject<List<string>>();
                        //                        break;
                        case JsonValueKind.True:
                            job.InputArguments[k] = true;
                            break;
                        case JsonValueKind.False:
                            job.InputArguments[k] = false;
                            break;
                    }
                }
            }
            try
            {
                var jobTask = client.RunJob(job);
                var progressTask = reportProgress(_server, progressToken, jobTask);
                Task.WaitAll(jobTask, progressTask); // Wait for both job and progress reporting to complete

                var call_resp = new CallToolResult()
                {
                    IsError = false,
                    Content = [new TextContentBlock { Text = $"Job finished successfully" }],
                };
                foreach (var k in jobTask.Result.Arguments.Keys)
                {
                    object? value = jobTask.Result.Arguments[k];
                    TypeCode code = Type.GetTypeCode(value?.GetType());

                    call_resp.Content.Add(new TextContentBlock { Text = $"{k}: {jobTask.Result.Arguments[k]}" });

                }
                ;
#if DEBUG
                //Console.WriteLine( $"{JsonSerializer.Serialize(jobTask.Result.Arguments)}");
                //foreach( var k in jobTask.Result.Arguments.Keys)
                //{
                //    Console.WriteLine($"{k}: {jobTask.Result.Arguments[k]}");
                //}
#endif
                return await ValueTask.FromResult(call_resp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return await ValueTask.FromResult(new CallToolResult()
                {
                    IsError = true,
                    Content = [new TextContentBlock { Text = e.Message }]
                });
            }
        }
    }
}
