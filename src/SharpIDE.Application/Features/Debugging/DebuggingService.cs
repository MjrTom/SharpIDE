using System.Diagnostics;
using Ardalis.GuardClauses;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Newtonsoft.Json.Linq;
using SharpIDE.Application.Features.Debugging.Experimental;

namespace SharpIDE.Application.Features.Debugging;

public class DebuggingService
{
	public async Task Attach(int debuggeeProcessId, CancellationToken cancellationToken = default)
	{
		Guard.Against.NegativeOrZero(debuggeeProcessId, nameof(debuggeeProcessId), "Process ID must be a positive integer.");
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

		var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = @"C:\Users\Matthew\Downloads\netcoredbg-win64\netcoredbg\netcoredbg.exe",
				//FileName = @"C:\Users\Matthew\.vscode-insiders\extensions\ms-dotnettools.csharp-2.83.5-win32-x64\.debugger\x86_64\vsdbg.exe",
				Arguments = "--interpreter=vscode",
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};
		process.Start();

		var debugProtocolHost = new DebugProtocolHost(process.StandardInput.BaseStream, process.StandardOutput.BaseStream, false);
		debugProtocolHost.LogMessage += (sender, args) =>
		{
			Console.WriteLine($"Log message: {args.Message}");
		};
		debugProtocolHost.EventReceived += (sender, args) =>
		{
			Console.WriteLine($"Event received: {args.EventType}");
		};
		debugProtocolHost.DispatcherError += (sender, args) =>
		{
			Console.WriteLine($"Dispatcher error: {args.Exception}");
		};
		debugProtocolHost.RequestReceived += (sender, args) =>
		{
			Console.WriteLine($"Request received: {args.Command}");
		};
		debugProtocolHost.RegisterEventType<OutputEvent>(@event =>
		{
			;
		});
		debugProtocolHost.RegisterClientRequestType<HandshakeRequest, HandshakeArguments, HandshakeResponse>(responder =>
		{
			var args = responder.Arguments;
			var signatureResponse = VsSigner.Sign(responder.Arguments.Value);
			responder.SetResponse(new HandshakeResponse(signatureResponse));
		});
		debugProtocolHost.RegisterEventType<StoppedEvent>(@event =>
		{
			;
			var threadId = @event.ThreadId;
		});
		debugProtocolHost.VerifySynchronousOperationAllowed();
		var initializeRequest = new InitializeRequest
		{
			ClientID = "vscode",
			ClientName = "Visual Studio Code",
			AdapterID = "coreclr",
			Locale = "en-us",
			LinesStartAt1 = true,
			ColumnsStartAt1 = true,
			PathFormat = InitializeArguments.PathFormatValue.Path,
			SupportsVariableType = true,
			SupportsVariablePaging = true,
			SupportsRunInTerminalRequest = true,
			SupportsHandshakeRequest = true
		};
		debugProtocolHost.Run();
		var response = debugProtocolHost.SendRequestSync(initializeRequest);

		var attachRequest = new AttachRequest
		{
			ConfigurationProperties = new Dictionary<string, JToken>
			{
				["name"] = "AttachRequestName",
				["type"] = "coreclr",
				["processId"] = debuggeeProcessId,
				["console"] = "internalConsole", // integratedTerminal, externalTerminal, internalConsole
			}
		};
		debugProtocolHost.SendRequestSync(attachRequest);

		// var breakpointRequest = new SetBreakpointsRequest
		// {
		// 	Source = new Source { Path = @"C:\Users\Matthew\Documents\Git\BlazorCodeBreaker\src\WebApi\Program.cs" },
		// 	Breakpoints = [new SourceBreakpoint { Line = 7 }]
		// };
		// var breakpointsResponse = debugProtocolHost.SendRequestSync(breakpointRequest);
		new DiagnosticsClient(debuggeeProcessId).ResumeRuntime();
		var configurationDoneRequest = new ConfigurationDoneRequest();
		debugProtocolHost.SendRequestSync(configurationDoneRequest);
	}
	// Typically you would do attachRequest, configurationDoneRequest, setBreakpointsRequest, then ResumeRuntime. But netcoredbg blows up on configurationDoneRequuest if ResumeRuntime hasn't been called yet.
}
