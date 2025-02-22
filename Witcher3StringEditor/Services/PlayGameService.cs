using CommunityToolkit.Diagnostics;
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
            Guard.IsTrue(File.Exists(appSettings.GameExePath));
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
            Log.Error(ex, "Failed to start the game process.\n-{0}", ex.Message);
        }
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data)) 
            Log.Error(e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Information(e.Data);
    }
}