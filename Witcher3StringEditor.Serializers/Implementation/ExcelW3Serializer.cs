using CommunityToolkit.Diagnostics;
using Serilog;
using Syncfusion.XlsIO;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Serializers.Internal;

namespace Witcher3StringEditor.Serializers.Implementation;

/// <summary>
///     Provides Excel serialization functionality for W3 string items
///     Implements the IExcelW3Serializer interface to handle reading from and writing to Excel files
/// </summary>
public class ExcelW3Serializer(IBackupService backupService) : IExcelW3Serializer
{
    /// <summary>
    ///     Deserializes W3 string items from an Excel file
    /// </summary>
    /// <param name="filePath">The path to the Excel file to deserialize</param>
    /// <returns>
    ///     A task that represents the asynchronous deserialize operation.
    ///     The task result contains the deserialized W3 string items, or an empty list if an error occurred
    /// </returns>
    public async Task<IReadOnlyList<IW3StringItem>> Deserialize(string filePath)
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

    /// <summary>
    ///     Serializes W3 string items to an Excel file
    /// </summary>
    /// <param name="w3StringItems">The W3 string items to serialize</param>
    /// <param name="context">The serialization context containing output directory and target language information</param>
    /// <returns>
    ///     A task that represents the asynchronous serialize operation.
    ///     The task result indicates whether the serialization was successful
    /// </returns>
    public async Task<bool> Serialize(IReadOnlyList<IW3StringItem> w3StringItems, W3SerializationContext context)
    {
        try
        {
            return await Task.Run(() =>
            {
                Guard.IsGreaterThan(w3StringItems.Count, 0);
                var filePath = Path.Combine(context.OutputDirectory,
                    $"{Enum.GetName(context.TargetLanguage)!.ToLowerInvariant()}.xlsx");
                if (File.Exists(filePath))
                    Guard.IsTrue(backupService.Backup(filePath));
                GenerateExcelFile(filePath, w3StringItems);
                return true;
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing the Excel worksheets file.");
            return false;
        }
    }

    /// <summary>
    ///     Generates an Excel file with the provided W3 string items
    /// </summary>
    /// <param name="path">The path where the Excel file will be created</param>
    /// <param name="w3StringItems">The W3 string items to include in the Excel file</param>
    private static void GenerateExcelFile(string path, IReadOnlyList<IW3StringItem> w3StringItems)
    {
        using var fileStream = File.Create(path);
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;
        var workbook = application.Workbooks.Create(1);
        var worksheet = workbook.Worksheets[0];
        FormatWorksheet(worksheet, w3StringItems);
        WriteDataToWorksheet(worksheet, w3StringItems);
        workbook.SaveAs(fileStream);
    }

    /// <summary>
    ///     Formats the worksheet with headers, styles, and column widths
    /// </summary>
    /// <param name="worksheet">The worksheet to format</param>
    /// <param name="w3StringItems">The W3 string items used to determine row count</param>
    private static void FormatWorksheet(IWorksheet worksheet, IReadOnlyList<IW3StringItem> w3StringItems)
    {
        SetTableHeaders(worksheet);
        SetTableStyles(worksheet, w3StringItems.Count);
        SetColumnWidths(worksheet);
    }

    /// <summary>
    ///     Sets the table headers in the first row of the worksheet
    /// </summary>
    /// <param name="worksheet">The worksheet to set headers on</param>
    private static void SetTableHeaders(IWorksheet worksheet)
    {
        worksheet["A1"].Value = "StrId";
        worksheet["B1"].Value = "KeyHex";
        worksheet["C1"].Value = "KeyName";
        worksheet["D1"].Value = "OldText";
        worksheet["E1"].Value = "Text";
    }

    /// <summary>
    ///     Applies formatting styles to the worksheet
    /// </summary>
    /// <param name="worksheet">The worksheet to apply styles to</param>
    /// <param name="rowCount">The number of data rows in the worksheet</param>
    private static void SetTableStyles(IWorksheet worksheet, int rowCount)
    {
        FormatHeaderRow(worksheet);
        FormatNormalCells(worksheet, rowCount);
        ApplyTableBorders(worksheet, rowCount);
        FormatTextCells(worksheet, rowCount);
        FreezeHeaderRow(worksheet);
    }

    /// <summary>
    ///     Formats the header row with bold text, center alignment, and color
    /// </summary>
    /// <param name="worksheet">The worksheet containing the header row</param>
    private static void FormatHeaderRow(IWorksheet worksheet)
    {
        var headerRange = worksheet["A1:E1"];
        headerRange.CellStyle.Font.Bold = true;
        headerRange.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        headerRange.VerticalAlignment = ExcelVAlign.VAlignCenter;
        headerRange.CellStyle.ColorIndex = ExcelKnownColors.Grey_80_percent;
        headerRange.CellStyle.Font.Color = ExcelKnownColors.White;
    }

    /// <summary>
    ///     Formats normal cells with center alignment
    /// </summary>
    /// <param name="worksheet">The worksheet containing the cells to format</param>
    /// <param name="rowCount">The number of data rows in the worksheet</param>
    private static void FormatNormalCells(IWorksheet worksheet, int rowCount)
    {
        var normalRange = worksheet[$"A2:C{rowCount + 1}"];
        normalRange.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        normalRange.VerticalAlignment = ExcelVAlign.VAlignCenter;
    }

    /// <summary>
    ///     Applies borders to the entire table
    /// </summary>
    /// <param name="worksheet">The worksheet containing the table</param>
    /// <param name="rowCount">The number of data rows in the worksheet</param>
    private static void ApplyTableBorders(IWorksheet worksheet, int rowCount)
    {
        var tableRange = worksheet[$"A1:E{rowCount + 1}"];
        tableRange.Borders.LineStyle = ExcelLineStyle.Thin;
        tableRange.Borders[ExcelBordersIndex.DiagonalUp].ShowDiagonalLine = false;
        tableRange.Borders[ExcelBordersIndex.DiagonalDown].ShowDiagonalLine = false;
        tableRange.NumberFormat = "@";
    }

    /// <summary>
    ///     Formats text cells to enable text wrapping
    /// </summary>
    /// <param name="worksheet">The worksheet containing the text cells</param>
    /// <param name="rowCount">The number of data rows in the worksheet</param>
    private static void FormatTextCells(IWorksheet worksheet, int rowCount)
    {
        var textRange = worksheet[$"D2:E{rowCount + 1}"];
        textRange.WrapText = true;
    }

    /// <summary>
    ///     Freezes the header row so it remains visible when scrolling
    /// </summary>
    /// <param name="worksheet">The worksheet to freeze the header row on</param>
    private static void FreezeHeaderRow(IWorksheet worksheet)
    {
        worksheet["A2:E2"].FreezePanes();
    }

    /// <summary>
    ///     Sets appropriate column widths for optimal viewing
    /// </summary>
    /// <param name="worksheet">The worksheet to set column widths on</param>
    private static void SetColumnWidths(IWorksheet worksheet)
    {
        worksheet["A:B"].ColumnWidth = 15;
        worksheet["C:C"].ColumnWidth = 30;
        worksheet["D:E"].ColumnWidth = 50;
    }

    /// <summary>
    ///     Writes the W3 string items data to the worksheet
    /// </summary>
    /// <param name="worksheet">The worksheet to write data to</param>
    /// <param name="w3StringItems">The W3 string items to write</param>
    private static void WriteDataToWorksheet(IWorksheet worksheet, IReadOnlyList<IW3StringItem> w3StringItems)
    {
        for (var i = 0; i < w3StringItems.Count; i++)
        {
            var rowIndex = i + 2;
            worksheet[$"A{rowIndex}"].Value = w3StringItems[i].StrId;
            worksheet[$"B{rowIndex}"].Value = w3StringItems[i].KeyHex;
            worksheet[$"C{rowIndex}"].Value = w3StringItems[i].KeyName;
            worksheet[$"D{rowIndex}"].Value = w3StringItems[i].OldText;
            worksheet[$"E{rowIndex}"].Value = w3StringItems[i].Text;
        }
    }
}