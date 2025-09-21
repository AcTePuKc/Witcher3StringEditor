using System.Diagnostics;
using System.IO;
using CommunityToolkit.Diagnostics;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Services;

/// <summary>
///     Provides functionality to play the game
///     Implements the IPlayGameService interface to handle starting the game process
/// </summary>
internal class PlayGameService(IAppSettings appSettings) : IPlayGameService
{
    /// <summary>
    ///     Starts the game process using the configured game executable path
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
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
            Log.Information("Game process exited with code {ExitCode}.", process.ExitCode);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start the game process.");
        }
    }

    /// <summary>
    ///     Handles the error data received event from the game process
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">A DataReceivedEventArgs that contains the event data</param>
    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        // Log any error output from the game process
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Error("Error: {Data}.", e.Data);
    }

    /// <summary>
    ///     Handles the output data received event from the game process
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">A DataReceivedEventArgs that contains the event data</param>
    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        // Log any standard output from the game process
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Information("Output: {Data}.", e.Data);
    }
}