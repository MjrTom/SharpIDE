using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using NeoSmart.AsyncLock;
using PolyType.SourceGenerator;
using SharpIDE.MsBuildHost.Contracts;
using StreamJsonRpc;

namespace SharpIDE.Application.Features.Build;

public partial class BuildService
{
    private Task? _fillPipeFromLoggerTask;
    private Process? _sharpIdeMsBuildHostProcess;
    private Socket? _buildLogSocket;
	private readonly AsyncLock _rpcInitLock = new AsyncLock();

    private async Task<IRpcBuildService> ConnectRpc()
    {
        using (await _rpcInitLock.LockAsync())
        {
            if (_rpcBuildService is not null) return _rpcBuildService;
            if (_fillPipeFromLoggerTask is not null)
                throw new InvalidOperationException(
                    "Build logger pipe is already open, but RPC service is not initialized. This should never happen.");
            var sharpIdeMsBuildHostDllPath = Path.Combine(AppContext.BaseDirectory, "SharpIdeMsBuildHost",
                "SharpIDE.MsBuildHost.dll");
            var startupInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = sharpIdeMsBuildHostDllPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            startupInfo.Environment["DOTNET_ROLL_FORWARD_TO_PRERELEASE"] = "1";
            var process = Process.Start(startupInfo);
            if (process is null) throw new InvalidOperationException("Failed to start SharpIDE.MsBuildHost");
            var handler = new LengthHeaderMessageHandler(process.StandardInput.BaseStream,
                process.StandardOutput.BaseStream,
                new NerdbankMessagePackFormatter
                    { TypeShapeProvider = TypeShapeProvider_SharpIDE_MsBuildHost_Contracts.Default });
            var rpc = new JsonRpc(handler);

            rpc.StartListening();

            var proxy = rpc.Attach<IRpcBuildService>();
            var (rpcBuildHostRuntimeVersion, rpcBuildHostMsBuildPath) = await proxy.GetMsbuildInfoAsync();
            _logger.LogInformation(
                "Connected to SharpIDE.MsBuildHost running on '{RpcBuildHostRuntimeVersion}' Runtime with MSBuild from SDK at '{RpcBuildHostMsBuildPath}'",
                rpcBuildHostRuntimeVersion, rpcBuildHostMsBuildPath);
            _fillPipeFromLoggerTask = await OpenMsBuildLoggerPipe(proxy);
            _sharpIdeMsBuildHostProcess = process;
            return proxy;
        }
    }

    private async Task<Task> OpenMsBuildLoggerPipe(IRpcBuildService rpcBuildService)
    {
        if (_buildLogSocket is not null) throw new InvalidOperationException("Build log socket is already open.");
        var pipe = new Pipe(new PipeOptions(useSynchronizationContext: false));
        var pipeReader = pipe.Reader;
        var pipeWriter = pipe.Writer;

        var unixDomainSocketFilePath = $"{Path.GetTempFileName()}.sock";
        var serverSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        serverSocket.Bind(new UnixDomainSocketEndPoint(unixDomainSocketFilePath));
        serverSocket.Listen(1);
        await rpcBuildService.BeginWritingMsBuildOutputToSocket(unixDomainSocketFilePath);

        var socket = await serverSocket.AcceptAsync();
        serverSocket.Close();
        File.Delete(unixDomainSocketFilePath);
        var fillPipeTask = Task.Run(async () =>
        {
            try
            {
                var buffer = new byte[4096];
                while (true)
                {
                    var bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None);
                    if (bytesRead == 0) break; // End of stream
                    await pipeWriter.WriteAsync(buffer.AsMemory(0, bytesRead));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reading from SharpIDE.MsBuildHost logger socket");
            }
            finally
            {
                await pipeWriter.CompleteAsync();
                socket.Dispose();
            }
        });
        BuildLogPipeReader = pipeReader;
        _buildLogSocket = socket;
        return fillPipeTask;
    }
}
