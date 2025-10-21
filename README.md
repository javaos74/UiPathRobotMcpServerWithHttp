# UiRobotSSE - UiPath Robot MCP Server

A Model Context Protocol (MCP) server that provides integration with UiPath Robot, allowing you to list, configure, and execute UiPath processes through MCP-compatible clients.

UiPath Robot과 통합되는 Model Context Protocol (MCP) 서버로, MCP 호환 클라이언트를 통해 UiPath 프로세스를 나열하고 구성하며 실행할 수 있습니다.

## Features / 기능

- **UiPath Attended Robot Integration**: Seamless integration with UiPath Assistant and local Robot / UiPath Assistant 및 로컬 Robot과의 원활한 통합
- **Process Discovery**: Automatically discover all processes visible in UiPath Assistant / UiPath Assistant에서 보이는 모든 프로세스 자동 검색
- **Automatic Installation**: Install missing processes on first execution / 최초 실행 시 누락된 프로세스 자동 설치
- **Dynamic Schema Generation**: Generate input schemas for each process / 각 프로세스에 대한 입력 스키마 자동 생성
- **Process Execution**: Execute UiPath processes with parameters / 매개변수를 사용한 UiPath 프로세스 실행
- **Progress Tracking**: Real-time progress notifications during execution / 실행 중 실시간 진행 상황 알림
- **AI-Friendly Tool Mapping**: Direct mapping from process names to tool names for better AI understanding / AI 이해도 향상을 위한 프로세스 이름에서 도구 이름으로의 직접 매핑
- **MCP 0.4.0 Compatible**: Built with the latest MCP specification / 최신 MCP 사양으로 구축

## Prerequisites / 사전 요구사항

### 1. .NET SDK Installation / .NET SDK 설치

Install .NET 8.0 LTS SDK from the official Microsoft website:
공식 Microsoft 웹사이트에서 .NET 8.0 LTS SDK를 설치하세요:

```bash
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
# Or use package managers:

# Windows (using winget)
winget install Microsoft.DotNet.SDK.8

# macOS (using Homebrew)
brew install --cask dotnet-sdk

# Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

Verify installation / 설치 확인:

```bash
dotnet --version
# Should show 8.0.x
```

### 2. UiPath Robot Installation / UiPath Robot 설치

Install UiPath Robot on your machine. The MCP server communicates with the local UiPath Robot service.
컴퓨터에 UiPath Robot을 설치하세요. MCP 서버는 로컬 UiPath Robot 서비스와 통신합니다.

Download from: https://www.uipath.com/product/robot

#### UiPath Attended Robot Integration / UiPath Attended Robot 통합

This MCP server integrates with **UiPath Attended Robot**, which means:
이 MCP 서버는 **UiPath Attended Robot**과 통합되며, 이는 다음을 의미합니다:

- **Process Discovery**: All processes visible in UiPath Assistant are automatically discovered and exposed as MCP tools / UiPath Assistant에서 보이는 모든 프로세스가 자동으로 검색되어 MCP 도구로 노출됩니다
- **Automatic Installation**: On first execution, the server may attempt to install processes that are not yet installed locally / 최초 실행 시 서버는 아직 로컬에 설치되지 않은 프로세스를 설치하려고 시도할 수 있습니다
- **Initial Startup Time**: If you have many processes to install, the first startup may take considerable time / 설치할 프로세스가 많은 경우 최초 시작 시간이 상당히 오래 걸릴 수 있습니다

#### Tool Naming Convention / 도구 명명 규칙

Each UiPath process becomes an MCP tool with the following mapping:
각 UiPath 프로세스는 다음 매핑으로 MCP 도구가 됩니다:

- **Tool Name**: Direct mapping from UiPath process name / UiPath 프로세스 이름에서 직접 매핑
- **Tool Description**: Uses the process description from UiPath / UiPath의 프로세스 설명 사용
- **Input Schema**: Automatically generated from process input arguments / 프로세스 입력 인수에서 자동 생성

**Best Practices for AI Integration / AI 통합을 위한 모범 사례:**

When developing UiPath processes for AI integration, consider these naming and description guidelines:
AI 통합을 위한 UiPath 프로세스를 개발할 때 다음 명명 및 설명 지침을 고려하세요:

```
✅ Good Examples / 좋은 예시:
- Name: "ExtractInvoiceData"
  Description: "Extract structured data from PDF invoices including vendor, amount, and date"

- Name: "SendEmailNotification" 
  Description: "Send automated email notifications with customizable subject and body content"

- Name: "ProcessExcelReport"
  Description: "Generate monthly sales reports from Excel data with charts and summaries"

❌ Avoid / 피해야 할 것:
- Name: "Process1" 
  Description: "Does something"

- Name: "Main"
  Description: ""
```

**Customization Options / 사용자 정의 옵션:**

Process names and descriptions can be modified through:
프로세스 이름과 설명은 다음을 통해 수정할 수 있습니다:

- **UiPath Studio**: During development phase / 개발 단계에서
- **UiPath Orchestrator**: During deployment and management / 배포 및 관리 중
- **Process Metadata**: Update process properties for better AI understanding / AI 이해도 향상을 위한 프로세스 속성 업데이트

### 3. Add UiPath NuGet Feed / UiPath NuGet 피드 추가

Add the UiPath official NuGet feed to access UiPath packages:
UiPath 패키지에 액세스하기 위해 UiPath 공식 NuGet 피드를 추가하세요:

```bash
dotnet nuget add source https://pkgs.dev.azure.com/uipath/Public.Feeds/_packaging/UiPath-Official/nuget/v3/index.json --name "UiPath-Official"
```

## Installation & Setup / 설치 및 설정

### 1. Clone the Repository / 저장소 복제

```bash
git clone https://github.com/javaos74/UiPathRobotMcpServerWithHttp.git
cd UiPathRobotMcpServerWithHttp
```

### 2. Restore Dependencies / 종속성 복원

```bash
cd UiRobotSSE
dotnet restore
```

### 3. Build the Project / 프로젝트 빌드

```bash
dotnet build
```

## Running the Server / 서버 실행

### Development Mode / 개발 모드

```bash
cd UiRobotSSE
dotnet run
```

The server will start on `http://127.0.0.1:3002/sse` by default.
서버는 기본적으로 `http://127.0.0.1:3002/sse `에서 시작됩니다.

### Production Mode / 프로덕션 모드

```bash
cd UiRobotSSE
dotnet build --configuration Release
dotnet run --configuration Release
```

## Testing with MCP Inspector / MCP Inspector로 테스트

MCP Inspector is a useful tool for testing and debugging MCP servers. Here's how to test your UiRobotSSE server:
MCP Inspector는 MCP 서버를 테스트하고 디버깅하는 데 유용한 도구입니다. UiRobotSSE 서버를 테스트하는 방법은 다음과 같습니다:

### 1. Install MCP Inspector / MCP Inspector 설치

```bash
# Install via npm
npm install -g @modelcontextprotocol/inspector

# Or use npx (no installation required)
npx @modelcontextprotocol/inspector
```

### 2. Start Your MCP Server / MCP 서버 시작

First, start your UiRobotSSE server:
먼저 UiRobotSSE 서버를 시작하세요:

```bash
cd UiRobotSSE
dotnet run
```

The server should be running on `http://127.0.0.1:3002/sse`
서버가 `http://127.0.0.1:3002/sse`에서 실행되어야 합니다

### 3. Connect MCP Inspector / MCP Inspector 연결

#### Option A: Direct HTTP Connection / 직접 HTTP 연결

```bash
# Connect to the HTTP endpoint
npx @modelcontextprotocol/inspector http://127.0.0.1:3002/sse
```

#### Option B: Process Connection / 프로세스 연결

```bash
# Connect via process (alternative method)
npx @modelcontextprotocol/inspector dotnet run --project /path/to/UiRobotSSE/UiRobotSSE.csproj
```

### 4. Test Server Functionality / 서버 기능 테스트

Once connected, you can test the following in MCP Inspector:
연결되면 MCP Inspector에서 다음을 테스트할 수 있습니다:

#### A. List Available Tools / 사용 가능한 도구 나열

1. Click on **"Tools"** tab in the inspector / Inspector에서 **"Tools"** 탭 클릭
2. You should see all UiPath processes as available tools / 모든 UiPath 프로세스가 사용 가능한 도구로 표시되어야 합니다
3. Each tool should have a name and description / 각 도구에는 이름과 설명이 있어야 합니다

#### B. Inspect Tool Schemas / 도구 스키마 검사

1. Click on any tool to view its input schema / 도구를 클릭하여 입력 스키마 확인
2. Verify that the schema matches the UiPath process arguments / 스키마가 UiPath 프로세스 인수와 일치하는지 확인
3. Check for required vs optional parameters / 필수 매개변수와 선택적 매개변수 확인

#### C. Execute Tools / 도구 실행

1. Select a tool and fill in the required parameters / 도구를 선택하고 필수 매개변수 입력
2. Click **"Call Tool"** to execute / **"Call Tool"**을 클릭하여 실행
3. Monitor the progress notifications in real-time / 실시간으로 진행 상황 알림 모니터링
4. Verify the execution results / 실행 결과 확인

### 5. Common Test Scenarios / 일반적인 테스트 시나리오

#### Test Case 1: Process Discovery / 프로세스 검색 테스트
```
Expected: All UiPath Assistant processes appear as tools
예상 결과: 모든 UiPath Assistant 프로세스가 도구로 나타남

Verification:
- Tool count matches UiPath Assistant process count
- Tool names match process names
- Descriptions are populated
```

#### Test Case 2: Schema Generation / 스키마 생성 테스트
```
Expected: Each tool has a valid JSON schema
예상 결과: 각 도구에 유효한 JSON 스키마가 있음

Verification:
- Schema type is "object"
- Properties match process input arguments
- Required fields are correctly marked
```

#### Test Case 3: Process Execution / 프로세스 실행 테스트
```
Expected: Process executes successfully with progress updates
예상 결과: 진행 상황 업데이트와 함께 프로세스가 성공적으로 실행됨

Verification:
- Progress notifications are received
- Final result contains output arguments
- No error messages in console
```

### 6. Troubleshooting Inspector Issues / Inspector 문제 해결

#### Connection Issues / 연결 문제
```bash
# Check if server is running
curl http://127.0.0.1:3002/sse

# Verify MCP endpoint
curl -X POST http://127.0.0.1:3002/sse \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
```

#### No Tools Visible / 도구가 보이지 않음
- Ensure UiPath Robot is running / UiPath Robot이 실행 중인지 확인
- Check UiPath Assistant for available processes / 사용 가능한 프로세스에 대해 UiPath Assistant 확인
- Review server console logs for errors / 오류에 대한 서버 콘솔 로그 검토

#### Execution Failures / 실행 실패
- Verify process parameters are correct / 프로세스 매개변수가 올바른지 확인
- Check UiPath Robot permissions / UiPath Robot 권한 확인
- Monitor server logs during execution / 실행 중 서버 로그 모니터링

### 7. Advanced Testing / 고급 테스트

For automated testing, you can use the MCP Inspector programmatically:
자동화된 테스트를 위해 MCP Inspector를 프로그래밍 방식으로 사용할 수 있습니다:

```javascript
// Example: Automated tool discovery test
const inspector = require('@modelcontextprotocol/inspector');

async function testToolDiscovery() {
  const client = await inspector.connect('http://127.0.0.1:3002/sse');
  const tools = await client.listTools();
  console.log(`Found ${tools.length} tools`);
  
  for (const tool of tools) {
    console.log(`Tool: ${tool.name} - ${tool.description}`);
  }
}
```

## MCP Client Configuration / MCP 클라이언트 구성

### For Kiro IDE / Kiro IDE용

Add the following to your MCP configuration:
MCP 구성에 다음을 추가하세요:

```json
{
  "mcpServers": {
    "uipath-robot": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/UiRobotSSE/UiRobotSSE.csproj"],
      "env": {},
      "disabled": false,
      "autoApprove": []
    }
  }
}
```

### For Other MCP Clients / 다른 MCP 클라이언트용

Connect to the HTTP transport endpoint:
HTTP 전송 엔드포인트에 연결하세요:

- **URL**: `http://127.0.0.1:3002/sse`
- **Transport**: HTTP
- **Protocol**: MCP 0.4.0-preview.3

## Available Tools / 사용 가능한 도구

The server automatically discovers and exposes UiPath processes as MCP tools. Each installed UiPath process becomes available as a tool with:
서버는 UiPath 프로세스를 자동으로 검색하고 MCP 도구로 노출합니다. 설치된 각 UiPath 프로세스는 다음과 함께 도구로 사용할 수 있습니다:

- **Dynamic Name**: Based on the process name / 프로세스 이름 기반 동적 이름
- **Auto-generated Schema**: Input parameters based on process arguments / 프로세스 인수 기반 자동 생성 스키마
- **Progress Tracking**: Real-time execution progress / 실시간 실행 진행 상황

### Example Tools / 도구 예시

The tools available depend on the UiPath processes visible in your UiPath Assistant. Examples of well-named processes for AI integration:
사용 가능한 도구는 UiPath Assistant에서 보이는 UiPath 프로세스에 따라 달라집니다. AI 통합을 위한 잘 명명된 프로세스의 예시:

**Business Process Automation / 비즈니스 프로세스 자동화:**

- `ProcessInvoices`: "Automatically process and validate incoming invoices from email attachments"
- `GenerateMonthlyReport`: "Create comprehensive monthly sales and performance reports"
- `UpdateCustomerDatabase`: "Synchronize customer information across multiple systems"

**Data Processing / 데이터 처리:**

- `ExtractWebData`: "Scrape product information from e-commerce websites with error handling"
- `ConvertExcelToPDF`: "Convert Excel spreadsheets to formatted PDF reports with charts"
- `ValidateDataQuality`: "Check data integrity and generate quality assessment reports"

**Communication & Notifications / 커뮤니케이션 및 알림:**

- `SendBulkEmails`: "Send personalized email campaigns with attachment support"
- `CreateTeamsNotification`: "Post automated status updates to Microsoft Teams channels"
- `GenerateSlackAlert`: "Send critical system alerts to designated Slack channels"

**System Integration / 시스템 통합:**

- `SyncSalesforceData`: "Bidirectional synchronization between Salesforce and local database"
- `BackupSystemFiles`: "Automated backup of critical system files with compression"
- `MonitorServerHealth`: "Check server status and generate health reports"

> **Note**: Tool names and descriptions come directly from your UiPath processes. Improve AI interaction by using descriptive names and detailed descriptions when developing your automations.
>
> **참고**: 도구 이름과 설명은 UiPath 프로세스에서 직접 가져옵니다. 자동화를 개발할 때 설명적인 이름과 자세한 설명을 사용하여 AI 상호작용을 개선하세요.

## Configuration / 구성

### Port Configuration / 포트 구성

To change the default port, modify `Program.cs`:
기본 포트를 변경하려면 `Program.cs`를 수정하세요:

```csharp
app.Run("http://127.0.0.1:YOUR_PORT");
```

### Process Update Interval / 프로세스 업데이트 간격

The server automatically refreshes the process list every 5 minutes. To change this interval, modify the constructor in `UiPathRobotToolHandler.cs`:
서버는 5분마다 프로세스 목록을 자동으로 새로 고칩니다. 이 간격을 변경하려면 `UiPathRobotToolHandler.cs`의 생성자를 수정하세요:

```csharp
public UiPathRobotToolHandler(int intervalseconds = 300) // 300 seconds = 5 minutes
```

## Troubleshooting / 문제 해결

### Common Issues / 일반적인 문제

1. **Port Already in Use / 포트가 이미 사용 중**

   ```bash
   # Find process using the port / 포트를 사용하는 프로세스 찾기
   lsof -i :3002  # macOS/Linux
   netstat -ano | findstr :3002  # Windows

   # Kill the process or change the port / 프로세스 종료 또는 포트 변경
   ```
2. **UiPath Robot Not Found / UiPath Robot을 찾을 수 없음**

   - Ensure UiPath Robot is installed and running / UiPath Robot이 설치되고 실행 중인지 확인
   - Check if the Robot service is started / Robot 서비스가 시작되었는지 확인
3. **No Processes Available / 사용 가능한 프로세스 없음**

   - Install UiPath processes through UiPath Studio / UiPath Studio를 통해 UiPath 프로세스 설치
   - Ensure processes are published to the local Robot / 프로세스가 로컬 Robot에 게시되었는지 확인
4. **Long Initial Startup Time / 긴 초기 시작 시간**

   - **Expected Behavior**: First run may take several minutes if many processes need installation / 많은 프로세스를 설치해야 하는 경우 첫 실행에 몇 분이 걸릴 수 있습니다 (예상되는 동작)
   - **Solution**: Be patient during first startup, subsequent runs will be faster / 첫 시작 시 인내심을 갖고 기다리세요. 이후 실행은 더 빠릅니다
   - **Monitoring**: Check console logs for installation progress / 설치 진행 상황은 콘솔 로그에서 확인하세요
5. **UiPath Assistant Integration Issues / UiPath Assistant 통합 문제**

   - Ensure UiPath Assistant is running and processes are visible / UiPath Assistant가 실행 중이고 프로세스가 보이는지 확인
   - Check if you're logged into UiPath Assistant / UiPath Assistant에 로그인되어 있는지 확인
   - Verify Robot connectivity in UiPath Assistant settings / UiPath Assistant 설정에서 Robot 연결 상태 확인
6. **Process Installation Failures / 프로세스 설치 실패**

   - Check network connectivity for downloading processes / 프로세스 다운로드를 위한 네트워크 연결 확인
   - Ensure sufficient disk space for process installation / 프로세스 설치를 위한 충분한 디스크 공간 확인
   - Verify UiPath licensing and permissions / UiPath 라이선스 및 권한 확인
7. **Tool Names Not AI-Friendly / AI 친화적이지 않은 도구 이름**

   - **Problem**: Generic names like "Main" or "Process1" / "Main" 또는 "Process1"과 같은 일반적인 이름
   - **Solution**: Update process names and descriptions in UiPath Studio before publishing / 게시하기 전에 UiPath Studio에서 프로세스 이름과 설명 업데이트
   - **Best Practice**: Use descriptive names that clearly indicate the process function / 프로세스 기능을 명확히 나타내는 설명적인 이름 사용

### Logs / 로그

The server outputs detailed logs to the console. For debugging, run in development mode:
서버는 콘솔에 자세한 로그를 출력합니다. 디버깅을 위해 개발 모드에서 실행하세요:

```bash
dotnet run --environment Development
```

## Contributing / 기여

1. Fork the repository / 저장소 포크
2. Create a feature branch / 기능 브랜치 생성
3. Make your changes / 변경 사항 적용
4. Submit a pull request / 풀 리퀘스트 제출

## License / 라이선스

This project is licensed under the MIT License - see the LICENSE file for details.
이 프로젝트는 MIT 라이선스에 따라 라이선스가 부여됩니다. 자세한 내용은 LICENSE 파일을 참조하세요.

## Version History / 버전 기록

- **v1.0.0**: Initial release with MCP 0.4.0 support / MCP 0.4.0 지원 초기 릴리스
- **v0.2.0**: Legacy version with MCP 0.2.0 / MCP 0.2.0을 사용한 레거시 버전
