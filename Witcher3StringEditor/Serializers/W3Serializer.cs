using CommandLine;
using CommunityToolkit.Diagnostics;
using MiniExcelLibs;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Text;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Interfaces;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Serializers;

internal class W3Serializer(IAppSettings appSettings, IBackupService backupService) : IW3Serializer
{
    public async Task<IEnumerable<IW3Item>> Deserialize(string path)
    {
        try
        {
            Guard.IsTrue(File.Exists(path));
            if (Path.GetExtension(path) == ".csv") return await DeserializeCsv(path);
            if (Path.GetExtension(path) == ".xlsx") return await DeserializeExcel(path);
            Guard.IsTrue((Path.GetExtension(path) == ".w3strings"));
            var newPath = Path.Combine(Directory.CreateTempSubdirectory().FullName, Path.GetFileName(path));
            File.Copy(path, newPath);
            return await DeserializeW3Strings(newPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing the file: {0}\n-{1}", path, ex.Message);
            return [];
        }
    }

    private static async Task<IEnumerable<IW3Item>> DeserializeCsv(string path)
    {
        try
        {
            Guard.IsTrue(File.Exists(path));
            return from line in await File.ReadAllLinesAsync(path)
                   where !line.StartsWith(';')
                   select line.Split("|")
                into parts
                   where parts.Length == 4
                   select new W3Item
                   {
                       StrId = parts[0],
                       KeyHex = parts[1],
                       KeyName = parts[2],
                       OldText = parts[3],
                       Text = parts[3]
                   };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing the CSV file: {0}\n-{1}", path, ex.Message);
            return [];
        }
    }

    private static async Task<IEnumerable<IW3Item>> DeserializeExcel(string path)
    {
        try
        {
            Guard.IsTrue(File.Exists(path));
            return await MiniExcel.QueryAsync<W3Item>(path);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing Excel worksheets file: {0}\n-{1}", path, ex.Message);
            return [];
        }
    }

    private async Task<IEnumerable<IW3Item>> DeserializeW3Strings(string path)
    {
        try
        {
            Guard.IsTrue(File.Exists(path));
            Guard.IsTrue(File.Exists(appSettings.W3StringsPath));
            using var process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = new ProcessStartInfo
            {
                FileName = appSettings.W3StringsPath,
                Arguments = Parser.Default.FormatCommandLine(new W3Options
                {
                    Decode = path
                }),
                UseShellExecute = false,
                CreateNoWindow = true,
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
            return await DeserializeCsv($"{path}.csv");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing W3Strings file: {0}\n-{1}", path, ex.Message);
            return [];
        }
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            Log.Error("Error: {0}", e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            Log.Information("Output: {0}", e.Data);
    }

    public async Task<bool> Serialize(IW3Job w3Job)
    {
        return w3Job.W3FileType switch
        {
            W3FileType.csv => await SerializeCsv(w3Job),
            W3FileType.w3Strings => await SerializeW3Strings(w3Job),
            W3FileType.excel => await SerializeExcel(w3Job),
            _ => throw new NotSupportedException($"The file type {w3Job.W3FileType} is not supported.")
        };
    }

    private async Task<bool> SerializeCsv(IW3Job w3Job, string folder)
    {
        try
        {
            Guard.IsTrue(Directory.Exists(folder));
            Guard.IsGreaterThan(w3Job.W3Items.Count(), 0);
            var saveLang = Enum.GetName(w3Job.Language);
            Guard.IsNotNullOrWhiteSpace(saveLang);
            var stringBuilder = new StringBuilder();
            var lang = w3Job.Language
                is not W3Language.ar
                and not W3Language.br
                and not W3Language.cn
                and not W3Language.esmx
                and not W3Language.kr
                and not W3Language.tr
                ? saveLang : "cleartext";
            stringBuilder.AppendLine($";meta[language={lang}]");
            stringBuilder.AppendLine("; id      |key(hex)|key(str)| text");
            foreach (var item in w3Job.W3Items)
                stringBuilder.AppendLine($"{item.StrId}|{item.KeyHex}|{item.KeyName}|{item.Text}");
            var csvPath = $"{Path.Combine(folder, saveLang)}.csv";
            if (File.Exists(csvPath))
                Guard.IsTrue(backupService.Backup(csvPath));
            await File.WriteAllTextAsync(csvPath, stringBuilder.ToString());
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing the CSV file.\n-{0}", ex.Message);
            return false;
        }
    }

    private async Task<bool> SerializeCsv(IW3Job w3Job) => await SerializeCsv(w3Job, w3Job.Path);

    private async Task<bool> SerializeExcel(IW3Job w3Job)
    {
        try
        {
            Guard.IsTrue(Directory.Exists(w3Job.Path));
            Guard.IsGreaterThan(w3Job.W3Items.Count(), 0);
            var saveLang = Enum.GetName(w3Job.Language);
            Guard.IsNotNullOrWhiteSpace(saveLang);
            var path = $"{Path.Combine(w3Job.Path, saveLang)}.xlsx";
            if (File.Exists(path) && !backupService.Backup(path)) return false;
            await MiniExcel.SaveAsAsync(path, w3Job.W3Items.Cast<W3Item>(), overwriteFile: true);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing the Excel worksheets file.\n-{0}", ex.Message);
            return false;
        }
    }

    private async Task<bool> SerializeW3Strings(IW3Job w3Job)
    {
        try
        {
            Guard.IsTrue(Directory.Exists(w3Job.Path));
            Guard.IsGreaterThan(w3Job.W3Items.Count(), 0);
            Guard.IsTrue(File.Exists(appSettings.W3StringsPath));
            var tempFolder = Directory.CreateTempSubdirectory().FullName;
            var (csvPath, w3StringsPath) = GenerateW3StringsFilePaths(w3Job, tempFolder);
            Guard.IsTrue(await SerializeCsv(w3Job, tempFolder));
            Guard.IsTrue(await StartSerializationProcess(w3Job, csvPath));
            Guard.IsTrue(CopyTempFilesWithBackup(Path.ChangeExtension(csvPath, ".w3strings"), w3StringsPath));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing W3Strings.\n-{0}", ex.Message);
            return false;
        }
    }

    private static (string csvPath, string w3StringsPath) GenerateW3StringsFilePaths(IW3Job w3Job, string tempFolder)
    {
        Guard.IsTrue(Directory.Exists(w3Job.Path));
        Guard.IsTrue(Directory.Exists(tempFolder));
        var saveLang = Enum.GetName(w3Job.Language);
        Guard.IsNotNullOrWhiteSpace(saveLang);
        return (Path.Combine(tempFolder, $"{saveLang}.csv"), Path.Combine(w3Job.Path, $"{saveLang}.w3strings"));
    }

    private async Task<bool> StartSerializationProcess(IW3Job w3Job, string csvPath)
    {
        Guard.IsTrue(File.Exists(appSettings.W3StringsPath));
        using var process = new Process();
        process.EnableRaisingEvents = true;
        process.StartInfo = new ProcessStartInfo
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            FileName = appSettings.W3StringsPath,
            Arguments = w3Job.IsIgnoreIdSpaceCheck
                ? Parser.Default.FormatCommandLine(new W3Options { Encode = csvPath, IsIgnoreIdSpaceCheck = true })
                : Parser.Default.FormatCommandLine(new W3Options { Encode = csvPath, IdSpace = w3Job.IdSpace })
        };
        process.ErrorDataReceived += Process_ErrorDataReceived;
        process.OutputDataReceived += Process_OutputDataReceived;
        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync();
        return process.ExitCode == 0;
    }

    private bool CopyTempFilesWithBackup(string tempPath, string path)
    {
        if (File.Exists(path) && !backupService.Backup(path))
            return false;
        try
        {
            File.Copy(tempPath, path, true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to copy file from {0} to {1}.\n-{2}", tempPath, path, ex.Message);
        }
        return true;
    }
}