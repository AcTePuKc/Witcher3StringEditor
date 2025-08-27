using System.Diagnostics;
using System.IO;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Witcher3StringEditor.Shared.Abstractions;

namespace Witcher3StringEditor.Services;

internal class PlayGameService(IAppSettings appSettings, ILogger<PlayGameService> logger) : IPlayGameService
{
    public async Task PlayGame()
    {
        try
        {
            logger.LogInformation("Starting the game process.");
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
            logger.LogInformation("Game process exited with code {ExitCode}.", process.ExitCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start the game process.");
        }
    }

    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
            logger.LogError("Error: {Data}.", e.Data);
    }

    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
            logger.LogInformation("Output: {Data}.", e.Data);
    }
}