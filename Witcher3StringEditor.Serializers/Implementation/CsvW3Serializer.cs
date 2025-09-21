using System.Globalization;
using System.Text;
using CommunityToolkit.Diagnostics;
using Serilog;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Serializers.Internal;

namespace Witcher3StringEditor.Serializers.Implementation;

/// <summary>
///     Provides CSV serialization functionality for The Witcher 3 string items
///     Implements the ICsvW3Serializer interface to handle reading from and writing to CSV files
/// </summary>
public class CsvW3Serializer(IBackupService backupService) : ICsvW3Serializer
{
    /// <summary>
    ///     Deserializes The Witcher 3 string items from a CSV file
    /// </summary>
    /// <param name="filePath">The path to the CSV file to deserialize</param>
    /// <returns>
    ///     A task that represents the asynchronous deserialize operation.
    ///     The task result contains the deserialized The Witcher 3 string items, or an empty list if an error occurred
    /// </returns>
    public async Task<IReadOnlyList<IW3StringItem>> Deserialize(string filePath)
    {
        try
        {
            var w3StringItems = new List<W3StringStringItem>();
            await foreach (var line in File.ReadLinesAsync(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';'))
                    continue;

                var item = ParseCsvLine(line);
                if (item != null)
                    w3StringItems.Add(item);
            }

            return w3StringItems;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deserializing the CSV file: {Path}.", filePath);
            return [];
        }
    }

    /// <summary>
    ///     Serializes The Witcher 3 string items to a CSV file
    /// </summary>
    /// <param name="w3StringItems">The The Witcher 3 string items to serialize</param>
    /// <param name="context">The serialization context containing output directory and target language information</param>
    /// <returns>
    ///     A task that represents the asynchronous serialize operation.
    ///     The task result indicates whether the serialization was successful
    /// </returns>
    public async Task<bool> Serialize(IReadOnlyList<IW3StringItem> w3StringItems, W3SerializationContext context)
    {
        try
        {
            var languageName = Enum.GetName(context.TargetLanguage)!.ToLowerInvariant();
            var csvLanguageIdentifier = context.TargetLanguage switch
            {
                W3Language.Ar
                    or W3Language.Br
                    or W3Language.Cn
                    or W3Language.Esmx
                    or W3Language.Kr
                    or W3Language.Tr => "cleartext",
                _ => languageName
            };
            await WriteFileWithBackup(Path.Combine(context.OutputDirectory, $"{languageName}.csv"),
                BuildCsvContent(w3StringItems, csvLanguageIdentifier));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing the CSV file.");
            return false;
        }
    }

    /// <summary>
    ///     Writes content to a file with backup creation if the file already exists
    /// </summary>
    /// <param name="filePath">The path to the file to write</param>
    /// <param name="content">The content to write to the file</param>
    /// <returns>A task that represents the asynchronous write operation</returns>
    private async Task WriteFileWithBackup(string filePath, string content)
    {
        if (File.Exists(filePath))
            Guard.IsTrue(backupService.Backup(filePath));
        await File.WriteAllTextAsync(filePath, content);
    }

    /// <summary>
    ///     Parses a single line from a CSV file into a W3StringStringItem
    /// </summary>
    /// <param name="line">The line to parse</param>
    /// <returns>A W3StringStringItem if parsing was successful, otherwise null</returns>
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

    /// <summary>
    ///     Builds the CSV content from a collection of The Witcher 3 string items
    /// </summary>
    /// <param name="w3StringItems">The The Witcher 3 string items to include in the CSV content</param>
    /// <param name="lang">The language identifier for the CSV metadata</param>
    /// <returns>The complete CSV content as a string</returns>
    private static string BuildCsvContent(IReadOnlyCollection<IW3StringItem> w3StringItems, string lang)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $";meta[language={lang}]");
        stringBuilder.AppendLine("; id      |key(hex)|key(str)| text");
        foreach (var w3StringItem in w3StringItems)
            stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                $"{w3StringItem.StrId}|{w3StringItem.KeyHex}|{w3StringItem.KeyName}|{w3StringItem.Text}");
        return stringBuilder.ToString();
    }
}