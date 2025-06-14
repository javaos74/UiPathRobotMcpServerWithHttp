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
        private IMcpServer? _server;


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

        public UiPathRobotToolHandler(int intervalseconds = 60*5) // default 5 minutes
        {
            Start(intervalseconds);
        }

        private void updateToolList()
        {
#if DEBUG
            Console.WriteLine($"Updating process list... Current time: {DateTime.Now}");
#endif
            _lock.EnterWriteLock();
            try
            {
                toollist.Tools = client.GetProcesses().Result.Select(
                    p => new Tool
                    {
                        Name = p.Name,
                        Description = p.Description,
                        InputSchema = Utils.GetInputParamv2(client, p.Key)
                    }).ToList();
                if( _server != null)
                    _server.SendNotificationAsync("notifications/tools/list_changed").Wait();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public async ValueTask<ListToolsResult> UiPathRobotListHandler(RequestContext<ListToolsRequestParams> request, CancellationToken token)
        {
            _lock.EnterReadLock(); // 읽기 락 획득
            try
            {
                _server = request.Server; // 서버 인스턴스 저장
                return await ValueTask.FromResult(toollist);
            }
            finally
            {
                _lock.ExitReadLock(); // 읽기 락 해제
            }
        }

        private async Task reportProgress( IMcpServer? server, ProgressToken? progressToken, Task task)
        {
            int progress = 0;
            int total = 100;
            while (!task.IsCompleted)
            {
                if (server != null)
                {
                    await server.SendNotificationAsync("notifications/progress", new
                    {
                        Progress = progress < 99 ? progress++ : 99,
                        Total = total,
                        ProgressToken = progressToken
                    });
                }
                await Task.Delay(5000); // 5초 대기
            }
        }

        public async ValueTask<CallToolResponse> UiPathRobotToolCallHandler(RequestContext<CallToolRequestParams> request, CancellationToken token)
        {
            var progressToken = request.Params?.Meta?.ProgressToken;
            var process = client.GetProcesses().Result.Where(p => p.Name == request.Params?.Name).FirstOrDefault();
            if (process == null)
            {
                return await ValueTask.FromResult(new CallToolResponse()
                {
                    Content = [new Content() { Text = "No process found...", Type = "text" }]
                });
            }
            var job = process.ToJob();

            foreach (var k in request.Params?.Arguments?.Keys)
            {
                var v = (JsonElement)request.Params.Arguments[k];
                switch (v.ValueKind)
                {
                    case JsonValueKind.String:
                        job.InputArguments[k] = v.ToString();
                        break;
                    case JsonValueKind.Number:
                        Decimal dec;
                        long lng;
                        if( v.TryGetInt64( out lng))
                            job.InputArguments[k] = lng;
                        else if (v.TryGetDecimal(out dec))
                            job.InputArguments[k] = dec;
                        break;
//                    case JsonValueKind.Object:
//                        job.InputArguments[k] = v.Deserialize( ;
//                        break;
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
            try
            {
                var jobTask = client.RunJob(job);
                var progressTask = reportProgress(request.Server, progressToken, jobTask);   
                Task.WaitAll(jobTask, progressTask); // Wait for both job and progress reporting to complete

                var call_resp = new CallToolResponse() {
                    IsError = false,
                    Content = [new Content() { Text = $"Job finished successfully", Type = "text" }],
                };
                foreach( var k in jobTask.Result.Arguments.Keys)
                {
                    object? value = jobTask.Result.Arguments[k];
                    TypeCode code = Type.GetTypeCode(value?.GetType());

                    switch (code)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            call_resp.Content.Add(new Content() { Text = $"{k}: {jobTask.Result.Arguments[k]}", Type = "text" });
                            break;
                        case TypeCode.Boolean:
                            call_resp.Content.Add(new Content() { Text = $"{k}: {jobTask.Result.Arguments[k]}", Type = "text" });
                            break;
                        case TypeCode.String:
                            call_resp.Content.Add(new Content() { Text = $"{k}: {jobTask.Result.Arguments[k]}", Type = "text" });
                            break;
                        case TypeCode.DateTime:
                            call_resp.Content.Add(new Content() { Text = $"{k}: {jobTask.Result.Arguments[k]}", Type = "text" });
                            break;
                        default:
                            call_resp.Content.Add(new Content() { Text = $"{k}: {jobTask.Result.Arguments[k]}", Type = "text" });
                            break;
                    }

                };
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
                return await ValueTask.FromResult(new CallToolResponse()
                {
                    IsError = true,
                    Content = [new Content() { Text = e.Message, Type = "text" }]
                });
            }            
        }
    }
}
