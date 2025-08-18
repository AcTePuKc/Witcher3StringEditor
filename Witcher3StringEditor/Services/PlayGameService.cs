using System.Diagnostics;
using System.IO;
using CommunityToolkit.Diagnostics;
using Serilog;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Services;

internal class PlayGameService(IAppSettings appSettings) : IPlayGameService
{
    public async Task PlayGame()
    {
        try
        {
            Log.Information("Starting the game process.");
            using var process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = new ProcessStartInfo
            {
                FileName = appSettings.GameExePath,
                WorkingDirectory = Path.GetDirectoryName(appSettings.GameExePath),
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();
            Guard.IsEqualTo(process.ExitCode, 0);
            Log.Information("Game process exited with code {0}.", process.ExitCode);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start the game process.");
        }
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Error("Error: {0}.", e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Information("Output: {0}.", e.Data);
    }
}