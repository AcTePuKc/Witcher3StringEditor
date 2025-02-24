using CommandLine;
using CommunityToolkit.Diagnostics;
using Serilog;
using Syncfusion.XlsIO;
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
            if (Path.GetExtension(path) == ".csv") return await DeserializeCsv(path);
            if (Path.GetExtension(path) == ".xlsx") return DeserializeExcel(path);
            Guard.IsTrue(Path.GetExtension(path) == ".w3strings");
            var newPath = Path.Combine(Directory.CreateTempSubdirectory().FullName, Path.GetFileName(path));
            File.Copy(path, newPath);
            return await DeserializeW3Strings(newPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing the file: {0}.", path);
            return [];
        }
    }

    public async Task<bool> Serialize(IW3Job w3Job)
    {
        return w3Job.W3FileType switch
        {
            W3FileType.csv => await SerializeCsv(w3Job),
            W3FileType.w3Strings => await SerializeW3Strings(w3Job),
            W3FileType.excel => SerializeExcel(w3Job),
            _ => throw new NotSupportedException($"The file type {w3Job.W3FileType} is not supported.")
        };
    }

    private static async Task<IEnumerable<IW3Item>> DeserializeCsv(string path)
    {
        try
        {
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
            Log.Error(ex, "An error occurred while deserializing the CSV file: {0}.", path);
            return [];
        }
    }

    private static List<W3Item> DeserializeExcel(string path)
    {
        try
        {
            using var excelEngine = new ExcelEngine();
            var worksheet = excelEngine.Excel.Workbooks.Open(path).Worksheets[0];
            var usedRange = worksheet.UsedRange;
            return worksheet.ExportData<W3Item>(1, 1, usedRange.LastRow, usedRange.LastColumn);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing Excel worksheets file: {0}.", path);
            return [];
        }
    }

    private async Task<IEnumerable<IW3Item>> DeserializeW3Strings(string path)
    {
        try
        {
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
            Log.Error(ex, "An error occurred while deserializing W3Strings file: {0}.", path);
            return [];
        }
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Error("Error: {0}", e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Information("Output: {0}", e.Data);
    }

    private async Task<bool> SerializeCsv(IW3Job w3Job, string folder)
    {
        try
        {
            var saveLang = Enum.GetName(w3Job.Language);
            var stringBuilder = new StringBuilder();
            var lang = w3Job.Language
                is not W3Language.ar
                and not W3Language.br
                and not W3Language.cn
                and not W3Language.esmx
                and not W3Language.kr
                and not W3Language.tr
                ? saveLang
                : "cleartext";
            stringBuilder.AppendLine($";meta[language={lang}]");
            stringBuilder.AppendLine("; id      |key(hex)|key(str)| text");
            foreach (var item in w3Job.W3Items)
                stringBuilder.AppendLine($"{item.StrId}|{item.KeyHex}|{item.KeyName}|{item.Text}");
            var csvPath = Path.Combine(folder, $"{saveLang}.csv");
            if (File.Exists(csvPath))
                Guard.IsTrue(backupService.Backup(csvPath));
            await File.WriteAllTextAsync(csvPath, stringBuilder.ToString());
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing the CSV file.");
            return false;
        }
    }

    private async Task<bool> SerializeCsv(IW3Job w3Job)
    {
        return await SerializeCsv(w3Job, w3Job.Path);
    }

    private bool SerializeExcel(IW3Job w3Job)
    {
        try
        {
            var saveLang = Enum.GetName(w3Job.Language);
            var path = Path.Combine(w3Job.Path, $"{saveLang}.xlsx");
            if (File.Exists(path))
                Guard.IsTrue(backupService.Backup(path));
            var items = w3Job.W3Items.ToArray();
            using var fileStream = File.Create(path);
            using var excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Xlsx;
            var workbook = application.Workbooks.Create(1);
            var worksheet = workbook.Worksheets[0];
            var tableRange = worksheet[$"A1:E{items.Length + 1}"];
            tableRange.Borders.LineStyle = ExcelLineStyle.Thin;
            tableRange.Borders[ExcelBordersIndex.DiagonalUp].ShowDiagonalLine = false;
            tableRange.Borders[ExcelBordersIndex.DiagonalDown].ShowDiagonalLine = false;
            tableRange.NumberFormat = "@";
            var headerRange = worksheet["A1:E1"];
            headerRange.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            headerRange.VerticalAlignment = ExcelVAlign.VAlignCenter;
            headerRange.CellStyle.ColorIndex = ExcelKnownColors.Light_blue;
            headerRange.CellStyle.Font.Bold = true;
            headerRange.CellStyle.Font.Color = ExcelKnownColors.White;
            var normalRange = worksheet[$"A2:C{items.Length + 1}"];
            normalRange.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            normalRange.VerticalAlignment = ExcelVAlign.VAlignCenter;
            worksheet[$"A1:B{items.Length + 1}"].ColumnWidth = 15;
            worksheet[$"C1:C{items.Length + 1}"].ColumnWidth = 30;
            var textRange = worksheet[$"D2:E{items.Length + 1}"];
            textRange.WrapText = true;
            textRange.ColumnWidth = 50;
            worksheet[$"A1"].Value = "StrId";
            worksheet[$"B1"].Value = "Key(Hex)";
            worksheet[$"C1"].Value = "Key(Name)";
            worksheet[$"D1"].Value = "Text(Old)";
            worksheet[$"E1"].Value = "Text";
            for (int i = 0; i < items.Length; i++)
            {
                var rowIndex = i + 2;
                worksheet[$"A{rowIndex}"].Value = items[i].StrId;
                worksheet[$"B{rowIndex}"].Value = items[i].KeyHex;
                worksheet[$"C{rowIndex}"].Value = items[i].KeyName;
                worksheet[$"D{rowIndex}"].Value = items[i].OldText;
                worksheet[$"E{rowIndex}"].Value = items[i].Text;
            }
            workbook.SaveAs(fileStream);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing the Excel worksheets file.");
            return false;
        }
    }

    private async Task<bool> SerializeW3Strings(IW3Job w3Job)
    {
        try
        {
            var (tempFolder, csvPath, w3StringsPath) = GenerateW3StringsFilePaths(w3Job);
            Guard.IsTrue(await SerializeCsv(w3Job, tempFolder));
            Guard.IsTrue(await StartSerializationProcess(w3Job, csvPath));
            Guard.IsTrue(CopyTempFilesWithBackup(Path.ChangeExtension(csvPath, ".w3strings"), w3StringsPath));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing W3Strings.");
            return false;
        }
    }

    private static (string tempFolder, string csvPath, string w3StringsPath) GenerateW3StringsFilePaths(IW3Job w3Job)
    {
        var saveLang = Enum.GetName(w3Job.Language);
        var tempFolder = Directory.CreateTempSubdirectory().FullName;
        return (tempFolder, Path.Combine(tempFolder, $"{saveLang}.csv"),
            Path.Combine(w3Job.Path, $"{saveLang}.w3strings"));
    }

    private async Task<bool> StartSerializationProcess(IW3Job w3Job, string csvPath)
    {
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
            Log.Error(ex, "Failed to copy file from {0} to {1}.", tempPath, path);
        }

        return true;
    }
}