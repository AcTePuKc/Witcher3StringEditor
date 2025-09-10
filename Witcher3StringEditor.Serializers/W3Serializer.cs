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
    public async Task<IReadOnlyList<IW3StringItem>> Deserialize(string filePath)
    {
        return Path.GetExtension(filePath) switch
        {
            ".csv" => await DeserializeCsv(filePath),
            ".xlsx" => await DeserializeExcel(filePath),
            ".w3strings" => await DeserializeW3Strings(CreateTemporaryCopyOfFile(filePath)),
            _ => []
        };
    }

    public async Task<bool> Serialize(IReadOnlyList<IW3StringItem> w3Items, W3SerializationContext context)
    {
        return context.TargetFileType switch
        {
            W3FileType.Csv => await SerializeCsv(w3Items, context),
            W3FileType.W3Strings => await SerializeW3Strings(w3Items, context),
            W3FileType.Excel => await SerializeExcel(w3Items, context),
            _ => throw new NotSupportedException($"The file type {context.TargetFileType} is not supported.")
        };
    }

    private static string CreateTemporaryCopyOfFile(string sourceFilePath)
    {
        var randomFileName = Path.GetRandomFileName();
        File.Copy(sourceFilePath, randomFileName);
        return randomFileName;
    }

    private static async Task<IReadOnlyList<IW3StringItem>> DeserializeCsv(string filePath)
    {
        try
        {
            var items = new List<W3StringStringItem>();
            await foreach (var line in File.ReadLinesAsync(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';'))
                    continue;

                var item = ParseCsvLine(line);
                if (item != null)
                    items.Add(item);
            }

            return items;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing the CSV file: {Path}.", filePath);
            return [];
        }
    }

    private static W3StringStringItem? ParseCsvLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';'))
            return null;
        var parts = line.Split('|');
        if (parts.Length != 4) return null;

        return new W3StringStringItem
        {
            StrId = parts[0].Trim(),
            KeyHex = parts[1].Trim(),
            KeyName = parts[2].Trim(),
            Text = parts[3].Trim()
        };
    }

    private static async Task<List<W3StringStringItem>> DeserializeExcel(string filePath)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var excelEngine = new ExcelEngine();
                var worksheet = excelEngine.Excel.Workbooks.Open(filePath).Worksheets[0];
                var usedRange = worksheet.UsedRange;
                return worksheet.ExportData<W3StringStringItem>(1, 1, usedRange.LastRow, usedRange.LastColumn);
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing Excel worksheets file: {Path}.", filePath);
            return [];
        }
    }

    private async Task<IReadOnlyList<IW3StringItem>> DeserializeW3Strings(string filePath)
    {
        try
        {
            using var process = await ExecuteExternalProcess(appSettings.W3StringsPath,
                Parser.Default.FormatCommandLine(new W3StringsOptions
                {
                    InputFileToDecode = filePath
                }));

            Guard.IsEqualTo(process.ExitCode, 0);
            return await DeserializeCsv($"{filePath}.csv");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing W3Strings file: {Path}.", filePath);
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

    private async Task<bool> SerializeCsv(IReadOnlyCollection<IW3StringItem> w3Items, W3SerializationContext context)
    {
        try
        {
            var languageName = Enum.GetName(context.TargetLanguage)!.ToLowerInvariant();
            var csvLanguageIdentifier = context.TargetLanguage switch
            {
                W3Language.Ar or W3Language.Br or W3Language.Cn or W3Language.Esmx or W3Language.Kr or W3Language.Tr => "cleartext",
                _ => languageName
            };
            await WriteFileWithBackup(Path.Combine(context.OutputDirectory, $"{languageName}.csv"),
                BuildCsvContent(w3Items, csvLanguageIdentifier));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing the CSV file.");
            return false;
        }
    }

    private async Task WriteFileWithBackup(string filePath, string content)
    {
        if (File.Exists(filePath))
            Guard.IsTrue(backupService.Backup(filePath));
        await File.WriteAllTextAsync(filePath, content);
    }

    private static string BuildCsvContent(IReadOnlyCollection<IW3StringItem> w3Items, string lang)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $";meta[language={lang}]");
        stringBuilder.AppendLine("; id      |key(hex)|key(str)| text");
        foreach (var item in w3Items)
            stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                $"{item.StrId}|{item.KeyHex}|{item.KeyName}|{item.Text}");
        return stringBuilder.ToString();
    }

    private async Task<bool> SerializeExcel(IReadOnlyList<IW3StringItem> w3Items, W3SerializationContext context)
    {
        try
        {
            return await Task.Run(() =>
            {
                Guard.IsGreaterThan(w3Items.Count, 0);
                var filePath = Path.Combine(context.OutputDirectory,
                    $"{Enum.GetName(context.TargetLanguage)!.ToLowerInvariant()}.xlsx");
                if (File.Exists(filePath))
                    Guard.IsTrue(backupService.Backup(filePath));
                GenerateExcelFile(filePath, w3Items);
                return true;
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing the Excel worksheets file.");
            return false;
        }
    }

    private static void GenerateExcelFile(string path, IReadOnlyList<IW3StringItem> w3Items)
    {
        using var fileStream = File.Create(path);
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;
        var workbook = application.Workbooks.Create(1);
        var worksheet = workbook.Worksheets[0];
        FormatWorksheet(worksheet, w3Items);
        WriteDataToWorksheet(worksheet, w3Items);
        workbook.SaveAs(fileStream);
    }

    private static void FormatWorksheet(IWorksheet worksheet, IReadOnlyList<IW3StringItem> w3Items)
    {
        SetTableHeaders(worksheet);
        SetTableStyles(worksheet, w3Items.Count);
        SetColumnWidths(worksheet);
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

    private static void WriteDataToWorksheet(IWorksheet worksheet, IReadOnlyList<IW3StringItem> items)
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

    private async Task<bool> SerializeW3Strings(IReadOnlyList<IW3StringItem> w3Items, W3SerializationContext context)
    {
        try
        {
            Guard.IsNotEmpty(w3Items);
            var saveLang = Enum.GetName(context.TargetLanguage)!.ToLowerInvariant();
            var tempDirectory = Directory.CreateTempSubdirectory().FullName;
            var tempCsvPath = Path.Combine(tempDirectory, $"{saveLang}.csv");
            var tempW3StringsPath = Path.ChangeExtension(tempCsvPath, ".csv.w3strings");
            var outputW3StringsPath = Path.Combine(context.OutputDirectory, $"{saveLang}.w3strings");
            var tempContext = context with
            {
                OutputDirectory = tempDirectory
            };
            Guard.IsTrue(await SerializeCsv(w3Items, tempContext));
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
        File.Copy(sourceFilePath, destinationFilePath);
        return true;
    }

    private async Task<bool> StartSerializationProcess(W3SerializationContext context, string path)
    {
        using var process = await ExecuteExternalProcess(appSettings.W3StringsPath, context.IgnoreIdSpaceCheck
            ? Parser.Default.FormatCommandLine(new W3StringsOptions { InputFileToEncode = path, IgnoreIdSpaceCheck = true })
            : Parser.Default.FormatCommandLine(new W3StringsOptions { InputFileToEncode = path, ExpectedIdSpace = context.ExpectedIdSpace }));
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
}