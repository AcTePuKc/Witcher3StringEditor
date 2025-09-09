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

public class W3Serializer(IAppSettings appSettings, IBackupService backupService) : IW3Serializer
{
    public async Task<IReadOnlyList<IW3Item>> Deserialize(string path)
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

    public async Task<bool> Serialize(IReadOnlyList<IW3Item> w3Items, W3SerializationContext context)
    {
        return context.TargetFileType switch
        {
            W3FileType.Csv => await SerializeCsv(w3Items, context),
            W3FileType.W3Strings => await SerializeW3Strings(w3Items, context),
            W3FileType.Excel => await SerializeExcel(w3Items, context),
            _ => throw new NotSupportedException($"The file type {context.TargetFileType} is not supported.")
        };
    }

    private static async Task<IReadOnlyList<IW3Item>> DeserializeCsv(string path)
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

            return items;
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

    private async Task<IReadOnlyList<IW3Item>> DeserializeW3Strings(string path)
    {
        try
        {
            using var process = await ExecuteExternalProcess(appSettings.W3StringsPath,
                Parser.Default.FormatCommandLine(new W3StringsOptions
                {
                    InputFileToDecode = path
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

    private async Task<bool> SerializeCsv(IReadOnlyCollection<IW3Item> w3Items, W3SerializationContext context)
    {
        try
        {
            var saveLang = Enum.GetName(context.TargetLanguage)!.ToLowerInvariant();
            var lang = context.TargetLanguage
                is not W3Language.Ar
                and not W3Language.Br
                and not W3Language.Cn
                and not W3Language.Esmx
                and not W3Language.Kr
                and not W3Language.Tr
                ? saveLang
                : "cleartext";
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $";meta[language={lang}]");
            stringBuilder.AppendLine("; id      |key(hex)|key(str)| text");
            foreach (var item in w3Items)
                stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                    $"{item.StrId}|{item.KeyHex}|{item.KeyName}|{item.Text}");
            var csvPath = Path.Combine(context.OutputDirectory, $"{saveLang}.csv");
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

    private async Task<bool> SerializeExcel(IReadOnlyList<IW3Item> w3Items, W3SerializationContext context)
    {
        try
        {
            return await Task.Run(() =>
            {
                var saveLang = Enum.GetName(context.TargetFileType)!.ToLowerInvariant();
                var path = Path.Combine(context.OutputDirectory, $"{saveLang}.xlsx");
                if (File.Exists(path))
                    Guard.IsTrue(backupService.Backup(path));
                Guard.IsGreaterThan(w3Items.Count, 0);
                using var fileStream = File.Create(path);
                using var excelEngine = new ExcelEngine();
                var application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
                var workbook = application.Workbooks.Create(1);
                var worksheet = workbook.Worksheets[0];
                SetTableHeaders(worksheet);
                SetTableStyles(worksheet, w3Items.Count);
                SetColumnWidths(worksheet);
                WriteDataToWorksheet(worksheet, w3Items);
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

    private static void WriteDataToWorksheet(IWorksheet worksheet, IReadOnlyList<IW3Item> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var rowIndex = i + 2;
            worksheet[$"A{rowIndex}"].Value = items[i].StrId;
            worksheet[$"B{rowIndex}"].Value = items[i].KeyHex;
            worksheet[$"C{rowIndex}"].Value = items[i].KeyName;
            worksheet[$"D{rowIndex}"].Value = items[i].OldText;
            worksheet[$"E{rowIndex}"].Value = items[i].Text;
        }
    }

    private async Task<bool> SerializeW3Strings(IReadOnlyList<IW3Item> w3Items, W3SerializationContext context)
    {
        try
        {
            var saveLang = Enum.GetName(context.TargetFileType)!.ToLowerInvariant();
            var tempDirectory = Directory.CreateTempSubdirectory().FullName;
            var tempCsvPath = Path.Combine(tempDirectory, $"{saveLang}.csv");
            var tempW3StringsPath = Path.ChangeExtension(tempCsvPath, ".csv.w3strings");
            var outputW3StringsPath = Path.Combine(context.OutputDirectory, $"{saveLang}.w3strings");
            context.OutputDirectory = tempDirectory;
            Guard.IsTrue(await SerializeCsv(w3Items, context));
            Guard.IsTrue(await StartSerializationProcess(context, tempCsvPath));
            Guard.IsTrue(CopyTempFilesWithBackup(tempW3StringsPath, outputW3StringsPath));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing W3Strings.");
            return false;
        }
    }

    private async Task<bool> StartSerializationProcess(W3SerializationContext context, string path)
    {
        try
        {
            using var process = await ExecuteExternalProcess(appSettings.W3StringsPath, context.IgnoreIdSpaceCheck
                ? Parser.Default.FormatCommandLine(new W3StringsOptions { InputFileToEncode = path, IgnoreIdSpaceCheck = true })
                : Parser.Default.FormatCommandLine(new W3StringsOptions { InputFileToEncode = path, ExpectedIdSpace = context.ExpectedIdSpace }));
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