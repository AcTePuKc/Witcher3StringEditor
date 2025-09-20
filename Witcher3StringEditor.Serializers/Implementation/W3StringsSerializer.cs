using System.Diagnostics;
using CommandLine;
using CommunityToolkit.Diagnostics;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Serializers.Internal;

namespace Witcher3StringEditor.Serializers.Implementation;

public class W3StringsSerializer(
    IAppSettings appSettings,
    IBackupService backupService,
    CsvW3Serializer csvSerializer)
    : IW3Serializer
{
    public async Task<IReadOnlyList<IW3StringItem>> Deserialize(string filePath)
    {
        try
        {
            using var process = await ExecuteExternalProcess(appSettings.W3StringsPath,
                Parser.Default.FormatCommandLine(new W3StringsOptions
                {
                    InputFileToDecode = filePath
                }));

            Guard.IsEqualTo(process.ExitCode, 0);
            return await csvSerializer.Deserialize($"{filePath}.csv");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing W3Strings file: {Path}.", filePath);
            return [];
        }
    }

    public async Task<bool> Serialize(IReadOnlyList<IW3StringItem> w3StringItems, W3SerializationContext context)
    {
        try
        {
            Guard.IsNotEmpty(w3StringItems);
            var saveLang = Enum.GetName(context.TargetLanguage)!.ToLowerInvariant();
            var tempDirectory = Directory.CreateTempSubdirectory().FullName;
            var tempCsvPath = Path.Combine(tempDirectory, $"{saveLang}.csv");
            var tempW3StringsPath = Path.ChangeExtension(tempCsvPath, ".csv.w3strings");
            var outputW3StringsPath = Path.Combine(context.OutputDirectory, $"{saveLang}.w3strings");
            var tempContext = context with
            {
                OutputDirectory = tempDirectory
            };
            Guard.IsTrue(await csvSerializer.Serialize(w3StringItems, tempContext));
            Guard.IsTrue(await StartSerializationProcess(tempContext, tempCsvPath));
            Guard.IsTrue(ReplaceFileWithBackup(tempW3StringsPath, outputW3StringsPath));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing W3Strings.");
            return false;
        }
    }

    private bool ReplaceFileWithBackup(string sourceFilePath, string destinationFilePath)
    {
        if (File.Exists(destinationFilePath) && !backupService.Backup(destinationFilePath))
            return false;
        File.Copy(sourceFilePath, destinationFilePath, true);
        return true;
    }

    private async Task<bool> StartSerializationProcess(W3SerializationContext context, string path)
    {
        using var process = await ExecuteExternalProcess(appSettings.W3StringsPath, context.IgnoreIdSpaceCheck
            ? Parser.Default.FormatCommandLine(new W3StringsOptions
                { InputFileToEncode = path, IgnoreIdSpaceCheck = true })
            : Parser.Default.FormatCommandLine(new W3StringsOptions
                { InputFileToEncode = path, ExpectedIdSpace = context.ExpectedIdSpace }));
        return process.ExitCode == 0;
    }

    private static async Task<Process> ExecuteExternalProcess(string filename, string arguments)
    {
        var process = new Process
        {
            EnableRaisingEvents = true,
            StartInfo = new ProcessStartInfo
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = filename,
                Arguments = arguments
            }
        };
        process.ErrorDataReceived += Process_ErrorDataReceived;
        process.OutputDataReceived += Process_OutputDataReceived;
        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync();
        return process;
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Error("Error: {Data}.", e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Information("Output: {Data}.", e.Data);
    }
}