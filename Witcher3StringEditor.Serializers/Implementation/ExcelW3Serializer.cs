using CommunityToolkit.Diagnostics;
using Serilog;
using Syncfusion.XlsIO;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Serializers.Internal;

namespace Witcher3StringEditor.Serializers.Implementation;

/// <summary>
///     Provides Excel serialization functionality for The Witcher 3 string items
///     Implements the IExcelW3Serializer interface to handle reading from and writing to Excel files
/// </summary>
public class ExcelW3Serializer(IBackupService backupService) : IExcelW3Serializer
{
    /// <summary>
    ///     Deserializes The Witcher 3 string items from an Excel file
    /// </summary>
    /// <param name="filePath">The path to the Excel file to deserialize</param>
    /// <returns>
    ///     A task that represents the asynchronous deserialize operation.
    ///     The task result contains the deserialized The Witcher 3 string items, or an empty list if an error occurred
    /// </returns>
    public async Task<IReadOnlyList<IW3StringItem>> Deserialize(string filePath)
    {
        try
        {
            // Run the deserialization process on a background thread to prevent UI blocking
            return await Task.Run(() =>
            {
                // Create an Excel engine instance with a 'using' statement to ensure proper cleanup
                using var excelEngine = new ExcelEngine();
                
                // Open the Excel workbook from the specified file path and get the first worksheet
                var worksheet = excelEngine.Excel.Workbooks.Open(filePath).Worksheets[0];
                
                // Get the range of cells that contain data in the worksheet
                var usedRange = worksheet.UsedRange;
                
                // Export the data from the worksheet to a list of W3StringStringItem objects
                // Starting from row 1, column 1 to the last row and column in the used range
                return worksheet.ExportData<W3StringStringItem>(1, 1, usedRange.LastRow, usedRange.LastColumn);
            });
        }
        catch (Exception ex)
        {
            // Log any errors that occur during the deserialization process
            Log.Error(ex, "An error occurred while deserializing Excel worksheets file: {Path}.", filePath);
            
            // Return an empty list in case of errors
            return [];
        }
    }

    /// <summary>
    ///     Serializes The Witcher 3 string items to an Excel file
    /// </summary>
    /// <param name="w3StringItems">The Witcher 3 string items to serialize</param>
    /// <param name="context">The serialization context containing output directory and target language information</param>
    /// <returns>
    ///     A task that represents the asynchronous serialize operation.
    ///     The task result indicates whether the serialization was successful
    /// </returns>
    public async Task<bool> Serialize(IReadOnlyList<IW3StringItem> w3StringItems, W3SerializationContext context)
    {
        try
        {
            // Run the serialization process on a background thread to prevent UI blocking
            return await Task.Run(() =>
            {
                // Ensure there are items to serialize, throw exception if count is zero
                Guard.IsGreaterThan(w3StringItems.Count, 0);
                
                // Generate the file path for the Excel file based on the target language
                var filePath = Path.Combine(context.OutputDirectory,
                    $"{Enum.GetName(context.TargetLanguage)!.ToLowerInvariant()}.xlsx");
                    
                // If the file already exists, create a backup before overwriting
                if (File.Exists(filePath))
                    // Use guard to ensure the backup operation succeeds, throw exception on failure
                    Guard.IsTrue(backupService.Backup(filePath));
                    
                // Generate the Excel file with the provided string items
                GenerateExcelFile(filePath, w3StringItems);
                
                // Return true to indicate successful serialization
                return true;
            });
        }
        catch (Exception ex)
        {
            // Log any errors that occur during the serialization process
            Log.Error(ex, "An error occurred while serializing the Excel worksheets file.");
            
            // Return false to indicate serialization failure
            return false;
        }
    }

    /// <summary>
    ///     Generates an Excel file with the provided The Witcher 3 string items
    /// </summary>
    /// <param name="path">The path where the Excel file will be created</param>
    /// <param name="w3StringItems">The Witcher 3 string items to include in the Excel file</param>
    private static void GenerateExcelFile(string path, IReadOnlyList<IW3StringItem> w3StringItems)
    {
        // Create a file stream for the output file using a 'using' statement to ensure proper disposal
        using var fileStream = File.Create(path); 
        
        // Create an Excel engine instance with a 'using' statement to ensure proper cleanup
        using var excelEngine = new ExcelEngine();
        
        // Get the Excel application object from the engine
        var application = excelEngine.Excel;
        
        // Set the default Excel version to XLSX format
        application.DefaultVersion = ExcelVersion.Xlsx;
        
        // Create a new workbook with one worksheet
        var workbook = application.Workbooks.Create(1);
        
        // Get the first (and only) worksheet from the workbook
        var worksheet = workbook.Worksheets[0];
        
        // Format the worksheet with headers, styles, and column widths
        FormatWorksheet(worksheet, w3StringItems);
        
        // Write the string item data to the worksheet
        WriteDataToWorksheet(worksheet, w3StringItems);
        
        // Save the workbook to the file stream
        workbook.SaveAs(fileStream);
    }

    /// <summary>
    ///     Formats the worksheet with headers, styles, and column widths
    /// </summary>
    /// <param name="worksheet">The worksheet to format</param>
    /// <param name="w3StringItems">The Witcher 3 string items used to determine row count</param>
    private static void FormatWorksheet(IWorksheet worksheet, IReadOnlyList<IW3StringItem> w3StringItems)
    {
        // Set the table headers in the first row of the worksheet
        SetTableHeaders(worksheet);
        
        // Apply formatting styles to the table including header formatting, cell alignment, borders, and freezing the header row
        SetTableStyles(worksheet, w3StringItems.Count);
        
        // Set appropriate column widths for better readability of the content
        SetColumnWidths(worksheet);
    }

    /// <summary>
    ///     Sets the table headers in the first row of the worksheet
    /// </summary>
    /// <param name="worksheet">The worksheet to set headers on</param>
    private static void SetTableHeaders(IWorksheet worksheet)
    {
        // Set the header value for column A (String ID)
        worksheet["A1"].Value = "StrId";
        
        // Set the header value for column B (Key in hexadecimal format)
        worksheet["B1"].Value = "KeyHex";
        
        // Set the header value for column C (Key name)
        worksheet["C1"].Value = "KeyName";
        
        // Set the header value for column D (Original text)
        worksheet["D1"].Value = "OldText";
        
        // Set the header value for column E (Translated/new text)
        worksheet["E1"].Value = "Text";
    }

    /// <summary>
    ///     Applies formatting styles to the worksheet
    /// </summary>
    /// <param name="worksheet">The worksheet to apply styles to</param>
    /// <param name="rowCount">The number of data rows in the worksheet</param>
    private static void SetTableStyles(IWorksheet worksheet, int rowCount)
    {
        // Format the header row with bold text, center alignment, and distinct colors
        FormatHeaderRow(worksheet);
        
        // Format normal data cells (ID, KeyHex, KeyName columns) with center alignment
        FormatNormalCells(worksheet, rowCount);
        
        // Apply borders to the entire table and set number format to text
        ApplyTableBorders(worksheet, rowCount);
        
        // Format text cells (OldText and Text columns) with text wrapping enabled
        FormatTextCells(worksheet, rowCount);
        
        // Freeze the header row so it remains visible when scrolling through data
        FreezeHeaderRow(worksheet);
    }

    /// <summary>
    ///     Formats the header row with bold text, center alignment, and color
    /// </summary>
    /// <param name="worksheet">The worksheet containing the header row</param>
    private static void FormatHeaderRow(IWorksheet worksheet)
    {
        // Define the header row range spanning columns A through E
        var headerRange = worksheet["A1:E1"];
        
        // Make the header text bold for better visibility and distinction
        headerRange.CellStyle.Font.Bold = true;
        
        // Center-align header content horizontally
        headerRange.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        
        // Center-align header content vertically
        headerRange.VerticalAlignment = ExcelVAlign.VAlignCenter;
        
        // Set the background color of header cells to dark grey (80% grey)
        headerRange.CellStyle.ColorIndex = ExcelKnownColors.Grey_80_percent;
        
        // Set the font color of header cells to white for better contrast against the dark background
        headerRange.CellStyle.Font.Color = ExcelKnownColors.White;
    }

    /// <summary>
    ///     Formats normal cells with center alignment
    /// </summary>
    /// <param name="worksheet">The worksheet containing the cells to format</param>
    /// <param name="rowCount">The number of data rows in the worksheet</param>
    private static void FormatNormalCells(IWorksheet worksheet, int rowCount)
    {
        // Define the range for normal data columns (A, B, C) excluding the header row
        // Start from row 2 (first data row) to rowCount + 1 (last data row)
        var normalRange = worksheet[$"A2:C{rowCount + 1}"];
        
        // Center-align content horizontally within the cells
        normalRange.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        
        // Center-align content vertically within the cells
        normalRange.VerticalAlignment = ExcelVAlign.VAlignCenter;
    }

    /// <summary>
    ///     Applies borders to the entire table
    /// </summary>
    /// <param name="worksheet">The worksheet containing the table</param>
    /// <param name="rowCount">The number of data rows in the worksheet</param>
    private static void ApplyTableBorders(IWorksheet worksheet, int rowCount)
    {
        // Define the complete table range including headers (row 1) and all data rows
        // Columns span from A to E, rows span from 1 (header) to rowCount + 1 (last data row)
        var tableRange = worksheet[$"A1:E{rowCount + 1}"];
        
        // Apply thin line style to all borders in the table range
        tableRange.Borders.LineStyle = ExcelLineStyle.Thin;
        
        // Disable diagonal up borderline (not typically used in data tables)
        tableRange.Borders[ExcelBordersIndex.DiagonalUp].ShowDiagonalLine = false;
        
        // Disable diagonal down borderline (not typically used in data tables)
        tableRange.Borders[ExcelBordersIndex.DiagonalDown].ShowDiagonalLine = false;
        
        // Set number format to "@" (text format) to prevent Excel from auto-formatting values
        // This ensures all data is treated as text, preserving leading zeros and preventing scientific notation
        tableRange.NumberFormat = "@";
    }

    /// <summary>
    ///     Formats text cells to enable text wrapping
    /// </summary>
    /// <param name="worksheet">The worksheet containing the text cells</param>
    /// <param name="rowCount">The number of data rows in the worksheet</param>
    private static void FormatTextCells(IWorksheet worksheet, int rowCount)
    {
        // Define the range for text columns (D and E) excluding the header row
        // Start from row 2 (first data row) to rowCount + 1 (last data row)
        var textRange = worksheet[$"D2:E{rowCount + 1}"];
        
        // Enable text wrapping for the text cells to ensure long text is fully visible
        textRange.WrapText = true;
    }

    /// <summary>
    ///     Freezes the header row so it remains visible when scrolling
    /// </summary>
    /// <param name="worksheet">The worksheet to freeze the header row on</param>
    private static void FreezeHeaderRow(IWorksheet worksheet)
    {
        // Freeze the header row (row 1) so that it remains visible when scrolling through the data
        // The range A2:E2 specifies the area below the header row where the freeze will be applied
        worksheet["A2:E2"].FreezePanes();
    }

    /// <summary>
    ///     Sets appropriate column widths for optimal viewing
    /// </summary>
    /// <param name="worksheet">The worksheet to set column widths on</param>
    private static void SetColumnWidths(IWorksheet worksheet)
    {
        // Set column width for columns A and B to 15 units
        // These columns typically contain ID and hex key values which are relatively short
        worksheet["A:B"].ColumnWidth = 15;
        
        // Set column width for column C to 30 units
        // This column typically contains the key name which may be longer than ID/hex values
        worksheet["C:C"].ColumnWidth = 30;
        
        // Set column width for columns D and E to 50 units
        // These columns typically contain text content (old and new) which can be quite long
        worksheet["D:E"].ColumnWidth = 50;
    }

    /// <summary>
    ///     Writes The Witcher 3 string items data to the worksheet
    /// </summary>
    /// <param name="worksheet">The worksheet to write data to</param>
    /// <param name="w3StringItems">The Witcher 3 string items to write</param>
    private static void WriteDataToWorksheet(IWorksheet worksheet, IReadOnlyList<IW3StringItem> w3StringItems)
    {
        // Iterate through each string item to write data to the worksheet
        // Start from row 2 since row 1 contains headers
        for (var i = 0; i < w3StringItems.Count; i++)
        {
            // Calculate the actual row index (1-based) for the current item
            // Row 1 is for headers, so data starts from row 2
            var rowIndex = i + 2;
            
            // Write each property of the string item to the corresponding cell in the row:
            // Column A: StrId
            worksheet[$"A{rowIndex}"].Value = w3StringItems[i].StrId;
            
            // Column B: KeyHex
            worksheet[$"B{rowIndex}"].Value = w3StringItems[i].KeyHex;
            
            // Column C: KeyName
            worksheet[$"C{rowIndex}"].Value = w3StringItems[i].KeyName;
            
            // Column D: OldText
            worksheet[$"D{rowIndex}"].Value = w3StringItems[i].OldText;
            
            // Column E: Text
            worksheet[$"E{rowIndex}"].Value = w3StringItems[i].Text;
        }
    }
}