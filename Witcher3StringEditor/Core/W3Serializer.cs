using CommandLine;
using Serilog;
using Syncfusion.Data.Extensions;
using System.Diagnostics;
using System.IO;
using System.Text;
using Witcher3StringEditor.Core.Helper;
using Witcher3StringEditor.Core.Implements;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Core
{
    public static class W3Serializer
    {
        public static async Task<IEnumerable<IW3Item>> Deserialize(string path)
        {
            if (Path.GetExtension(path) == ".csv")
            {
                return DeserializeCsv(path);
            }
            else if (Path.GetExtension(path) == ".w3strings")
            {
                var folder = FileHelper.CreateRandomTempDirectory();
                var filename = Path.GetFileName(path);
                var newPath = Path.Combine(folder, filename);
                File.Copy(path, newPath);
                return await DeserializeW3Strings(newPath);
            }
            else
            {
                return [];
            }
        }

        private static IEnumerable<IW3Item> DeserializeCsv(string path)
        {
            return (from line in File.ReadAllLines(path)
                    where !line.StartsWith(';')
                    select line.Split("|")
                into parts
                    where parts.Length == 4
                    select new W3Item()
                    {
                        StrID = parts[0]
                            .Trim(),
                        KeyHex = parts[1],
                        KeyName = parts[2],
                        Text = parts[3]
                    });
        }

        private static async Task<IEnumerable<IW3Item>> DeserializeW3Strings(string path)
        {
            using var process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = new ProcessStartInfo
            {
                FileName = ConfigureManger.Load().W3StringsPath,
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
            if (e.Data != null)
            {
                Log.Error(e.Data);
            }
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Log.Information(e.Data);
            }
        }

        public static async Task<bool> Serialize(IW3Job w3Job)
        {
            if (w3Job.FileType == Common.FileType.w3Strings)
            {
                return await SerializeW3Strings(w3Job);
            }
            else
            {
                return await SerializeCsv(w3Job);
            }
        }

        private static async Task<bool> SerializeCsv(IW3Job w3Job, string folder)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($";meta[language={LanguageHelper.Get(w3Job.Language)}]");
            stringBuilder.AppendLine($"; id      |key(hex)|key(str)| text");
            w3Job.W3Items.ForEach(x => stringBuilder.AppendLine($"{x.StrId}|{x.KeyHex}|{x.KeyName}|{x.Text}"));
            var csvPath = $"{Path.Combine(folder, Enum.GetName(w3Job.Language) ?? "en")}.csv";
            if (File.Exists(csvPath))
                BackupManger.Backup(csvPath);
            await File.WriteAllTextAsync(csvPath, stringBuilder.ToString());
            return true;
        }

        private static async Task<bool> SerializeCsv(IW3Job w3Job) => await SerializeCsv(w3Job, w3Job.Path);

        private static async Task<bool> SerializeW3Strings(IW3Job w3Job)
        {
            var tempFolder = FileHelper.CreateRandomTempDirectory();
            var csvPath = $"{Path.Combine(tempFolder, Enum.GetName(w3Job.Language) ?? "en")}.csv";
            var w3StringsPath = $"{Path.Combine(w3Job.Path, Enum.GetName(w3Job.Language) ?? "en")}.w3strings";

            if (await SerializeCsv(w3Job, tempFolder))
            {
                using var process = new Process();
                process.EnableRaisingEvents = true;
                process.StartInfo = new ProcessStartInfo
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = ConfigureManger.Load().W3StringsPath,
                    Arguments = w3Job.IsIgnoreIdSpaceCheck
                        ? Parser.Default.FormatCommandLine(new W3Options() { Encode = csvPath, IsIgnoreIdSpaceCheck = true })
                        : Parser.Default.FormatCommandLine(new W3Options() { Encode = csvPath, IdSpace = w3Job.IdSpace })
                };
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.OutputDataReceived += Process_OutputDataReceived;
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                await process.WaitForExitAsync();
                if (process.ExitCode == 0)
                {
                    var tempW3StringsPath = $"{csvPath}.w3strings";
                    if (File.Exists(w3StringsPath))
                        BackupManger.Backup(w3StringsPath);
                    File.Copy(tempW3StringsPath, w3StringsPath, true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}