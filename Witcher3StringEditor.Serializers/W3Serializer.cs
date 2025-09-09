using System.Diagnostics;
using System.Globalization;
using System.Text;
using CommandLine;
using CommunityToolkit.Diagnostics;
using Serilog;
using Syncfusion.XlsIO;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Serializers.Internal;

namespace Witcher3StringEditor.Serializers;

public class W3Serializer(IAppSettings appSettings, IBackupService backupService)
    : IW3Serializer
{
    public async Task<IEnumerable<IW3Item>> Deserialize(string path)
    {
        try
        {
            if (Path.GetExtension(path) == ".csv") return await DeserializeCsv(path);
            if (Path.GetExtension(path) == ".xlsx") return await DeserializeExcel(path);
            Guard.IsTrue(Path.GetExtension(path) == ".w3strings");
            var newPath = Path.Combine(Directory.CreateTempSubdirectory().FullName, Path.GetFileName(path));
            File.Copy(path, newPath);
            return await DeserializeW3Strings(newPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing the file: {Path}.", path);
            return [];
        }
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

    private static async Task<IEnumerable<IW3Item>> DeserializeCsv(string path)
    {
        try
        {
            var items = new List<W3Item>();
            await foreach (var line in File.ReadLinesAsync(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';'))
                    continue;
                var parts = line.Split('|');
                if (parts.Length != 4) continue;

                items.Add(new W3Item
                {
                    StrId = parts[0].Trim(),
                    KeyHex = parts[1].Trim(),
                    KeyName = parts[2].Trim(),
                    Text = parts[3].Trim()
                });
            }

            return items.AsReadOnly();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing the CSV file: {Path}.", path);
            return [];
        }
    }

    private static async Task<List<W3Item>> DeserializeExcel(string path)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var excelEngine = new ExcelEngine();
                var worksheet = excelEngine.Excel.Workbooks.Open(path).Worksheets[0];
                var usedRange = worksheet.UsedRange;
                return worksheet.ExportData<W3Item>(1, 1, usedRange.LastRow, usedRange.LastColumn);
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing Excel worksheets file: {Path}.", path);
            return [];
        }
    }

    private async Task<IEnumerable<IW3Item>> DeserializeW3Strings(string path)
    {
        try
        {
            using var process = await ExecuteExternalProcess(appSettings.W3StringsPath,
                Parser.Default.FormatCommandLine(new W3Options
                {
                    Decode = path
                }));

            Guard.IsEqualTo(process.ExitCode, 0);
            return await DeserializeCsv($"{path}.csv");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing W3Strings file: {Path}.", path);
            return [];
        }
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

    private async Task<bool> SerializeCsv(IW3Job w3Job, string folder)
    {
        try
        {
            var saveLang = Enum.GetName(w3Job.Language);
            var lang = w3Job.Language
                is not W3Language.ar
                and not W3Language.br
                and not W3Language.cn
                and not W3Language.esmx
                and not W3Language.kr
                and not W3Language.tr
                ? saveLang
                : "cleartext";
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $";meta[language={lang}]");
            stringBuilder.AppendLine("; id      |key(hex)|key(str)| text");
            foreach (var item in w3Job.W3Items)
                stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                    $"{item.StrId}|{item.KeyHex}|{item.KeyName}|{item.Text}");
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

    private async Task<bool> SerializeExcel(IW3Job w3Job)
    {
        try
        {
            return await Task.Run(() =>
            {
                var saveLang = Enum.GetName(w3Job.Language);
                var path = Path.Combine(w3Job.Path, $"{saveLang}.xlsx");
                if (File.Exists(path))
                    Guard.IsTrue(backupService.Backup(path));
                var items = w3Job.W3Items.ToArray();
                Guard.IsGreaterThan(items.Length, 0);
                using var fileStream = File.Create(path);
                using var excelEngine = new ExcelEngine();
                var application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
                var workbook = application.Workbooks.Create(1);
                var worksheet = workbook.Worksheets[0];
                SetTableHeaders(worksheet);
                SetTableStyles(worksheet, items.Length);
                SetColumnWidths(worksheet);
                WriteDataToWorksheet(worksheet, items);
                workbook.SaveAs(fileStream);
                return true;
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing the Excel worksheets file.");
            return false;
        }
    }

    private static void SetTableHeaders(IWorksheet worksheet)
    {
        worksheet["A1"].Value = "StrId";
        worksheet["B1"].Value = "KeyHex";
        worksheet["C1"].Value = "KeyName";
        worksheet["D1"].Value = "OldText";
        worksheet["E1"].Value = "Text";
    }

    private static void SetTableStyles(IWorksheet worksheet, int rowCount)
    {
        var headerRange = worksheet["A1:E1"];
        headerRange.CellStyle.Font.Bold = true;
        headerRange.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        headerRange.VerticalAlignment = ExcelVAlign.VAlignCenter;
        headerRange.CellStyle.ColorIndex = ExcelKnownColors.Grey_80_percent;
        headerRange.CellStyle.Font.Color = ExcelKnownColors.White;
        var normalRange = worksheet[$"A2:C{rowCount + 1}"];
        normalRange.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        normalRange.VerticalAlignment = ExcelVAlign.VAlignCenter;
        var tableRange = worksheet[$"A1:E{rowCount + 1}"];
        tableRange.Borders.LineStyle = ExcelLineStyle.Thin;
        tableRange.Borders[ExcelBordersIndex.DiagonalUp].ShowDiagonalLine = false;
        tableRange.Borders[ExcelBordersIndex.DiagonalDown].ShowDiagonalLine = false;
        tableRange.NumberFormat = "@";
        var textRange = worksheet[$"D2:E{rowCount + 1}"];
        textRange.WrapText = true;
        worksheet["A2:E2"].FreezePanes();
    }

    private static void SetColumnWidths(IWorksheet worksheet)
    {
        worksheet["A:B"].ColumnWidth = 15;
        worksheet["C:C"].ColumnWidth = 30;
        worksheet["D:E"].ColumnWidth = 50;
    }

    private static void WriteDataToWorksheet(IWorksheet worksheet, IW3Item[] items)
    {
        for (var i = 0; i < items.Length; i++)
        {
            var rowIndex = i + 2;
            worksheet[$"A{rowIndex}"].Value = items[i].StrId;
            worksheet[$"B{rowIndex}"].Value = items[i].KeyHex;
            worksheet[$"C{rowIndex}"].Value = items[i].KeyName;
            worksheet[$"D{rowIndex}"].Value = items[i].OldText;
            worksheet[$"E{rowIndex}"].Value = items[i].Text;
        }
    }

    private async Task<bool> SerializeW3Strings(IW3Job w3Job)
    {
        try
        {
            var (tempFolder, csvPath, tempW3StringsPath, w3StringsPath) = GenerateW3StringsFilePaths(w3Job);
            Guard.IsTrue(await SerializeCsv(w3Job, tempFolder));
            Guard.IsTrue(await StartSerializationProcess(w3Job, csvPath));
            Guard.IsTrue(CopyTempFilesWithBackup(tempW3StringsPath, w3StringsPath));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing W3Strings.");
            return false;
        }
    }

    private static (string tempFolder, string csvPath, string tempW3StringsPath, string w3StringsPath)
        GenerateW3StringsFilePaths(IW3Job w3Job)
    {
        var saveLang = Enum.GetName(w3Job.Language);
        var tempFolder = Directory.CreateTempSubdirectory().FullName;
        var csvPath = Path.Combine(tempFolder, $"{saveLang}.csv");
        return (tempFolder, csvPath, tempW3StringsPath: Path.ChangeExtension(csvPath, ".csv.w3strings"),
            Path.Combine(w3Job.Path, $"{saveLang}.w3strings"));
    }

    private async Task<bool> StartSerializationProcess(IW3Job w3Job, string csvPath)
    {
        try
        {
            using var process = await ExecuteExternalProcess(appSettings.W3StringsPath, w3Job.IsIgnoreIdSpaceCheck
                ? Parser.Default.FormatCommandLine(new W3Options { Encode = csvPath, IsIgnoreIdSpaceCheck = true })
                : Parser.Default.FormatCommandLine(new W3Options { Encode = csvPath, IdSpace = w3Job.IdSpace }));
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start the serialization process.");
            return false;
        }
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
            Log.Error(ex, "Failed to copy file from {TempPath} to {Path}.", tempPath, path);
        }

        return true;
    }
}