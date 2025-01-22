using Serilog;
using System.Diagnostics;
using System.IO;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Services;

internal class PlayGameService(IAppSettings appSettings) : IPlayGameService
{
    public async Task PlayGame()
    {
        try
        {
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
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to start the game process: {ex.Message}");
        }
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null) Log.Error(e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null) Log.Information(e.Data);
    }
}