using CommandLine;
using Serilog;
using Syncfusion.Data.Extensions;
using System.Diagnostics;
using System.IO;
using System.Text;
using Witcher3StringEditor.Core.Common;
using Witcher3StringEditor.Core.Implements;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Core;

public class W3Serializer(string wstrings)
{
    public async Task<IEnumerable<IW3Item>> Deserialize(string path)
    {
        if (Path.GetExtension(path) == ".csv") return DeserializeCsv(path);

        if (Path.GetExtension(path) != ".w3strings") return [];
        var folder = CreateRandomTempDirectory();
        var filename = Path.GetFileName(path);
        var newPath = Path.Combine(folder, filename);
        File.Copy(path, newPath);
        return await DeserializeW3Strings(newPath);
    }

    private static IEnumerable<IW3Item> DeserializeCsv(string path)
    {
        return from line in File.ReadAllLines(path)
               where !line.StartsWith(';')
               select line.Split("|")
            into parts
               where parts.Length == 4
               select new W3Item
               {
                   StrId = parts[0]
                       .Trim(),
                   KeyHex = parts[1],
                   KeyName = parts[2],
                   Text = parts[3]
               };
    }

    private async Task<IEnumerable<IW3Item>> DeserializeW3Strings(string path)
    {
        using var process = new Process();
        process.EnableRaisingEvents = true;
        process.StartInfo = new ProcessStartInfo
        {
            FileName = wstrings,
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
        return process.ExitCode == 0 ? DeserializeCsv($"{path}.csv") : [];
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null) Log.Error(e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null) Log.Information(e.Data);
    }

    public async Task<bool> Serialize(IW3Job w3Job)
    {
        if (w3Job.FileType == FileType.w3Strings) return await SerializeW3Strings(w3Job);

        return await SerializeCsv(w3Job);
    }

    private static async Task<bool> SerializeCsv(IW3Job w3Job, string folder)
    {
        var stringBuilder = new StringBuilder();
        var lang = w3Job.Language
            is not W3Language.ar
            and not W3Language.br
            and not W3Language.cn
            and not W3Language.esmx
            and not W3Language.kr
            and not W3Language.tr
            ? Enum.GetName(w3Job.Language) ?? "en"
            : "cleartext";
        stringBuilder.AppendLine($";meta[language={lang}]");
        stringBuilder.AppendLine("; id      |key(hex)|key(str)| text");
        w3Job.W3Items.ForEach(x => stringBuilder.AppendLine($"{x.StrId}|{x.KeyHex}|{x.KeyName}|{x.Text}"));
        var csvPath = $"{Path.Combine(folder, Enum.GetName(w3Job.Language) ?? "en")}.csv";
        if (File.Exists(csvPath))
            BackupManger.Instance.Backup(csvPath);
        await File.WriteAllTextAsync(csvPath, stringBuilder.ToString());
        return true;
    }

    private static async Task<bool> SerializeCsv(IW3Job w3Job)
    {
        return await SerializeCsv(w3Job, w3Job.Path);
    }

    private async Task<bool> SerializeW3Strings(IW3Job w3Job)
    {
        var tempFolder = CreateRandomTempDirectory();
        var csvPath = $"{Path.Combine(tempFolder, Enum.GetName(w3Job.Language) ?? "en")}.csv";
        var w3StringsPath = $"{Path.Combine(w3Job.Path, Enum.GetName(w3Job.Language) ?? "en")}.w3strings";

        if (!await SerializeCsv(w3Job, tempFolder)) return false;
        using var process = new Process();
        process.EnableRaisingEvents = true;
        process.StartInfo = new ProcessStartInfo
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            FileName = wstrings,
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
        if (process.ExitCode != 0) return false;
        var tempW3StringsPath = $"{csvPath}.w3strings";
        if (File.Exists(w3StringsPath))
            BackupManger.Instance.Backup(w3StringsPath);
        File.Copy(tempW3StringsPath, w3StringsPath, true);
        return true;
    }

    private static string CreateRandomTempDirectory()
    {
        string tempPath;

        do
        {
            // Generate a random directory name using Guid to ensure uniqueness.
            var randomDirName = Guid.NewGuid().ToString("N"); // Remove hyphens for a cleaner name.
            tempPath = Path.Combine(Path.GetTempPath(), randomDirName);
        } while (Directory.Exists(tempPath)); // Ensure the directory does not already exist.

        // Create the directory.
        Directory.CreateDirectory(tempPath);

        return tempPath;
    }
}