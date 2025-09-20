using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Serializers.Implementation;

namespace Witcher3StringEditor.Serializers;

public class W3SerializerCoordinator(
    CsvW3Serializer csvW3Serializer,
    ExcelW3Serializer excelW3Serializer,
    W3StringsSerializer w3StringsSerializer) : IW3Serializer
{
    public async Task<IReadOnlyList<IW3StringItem>> Deserialize(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".csv" => await csvW3Serializer.Deserialize(filePath),
            ".xlsx" => await excelW3Serializer.Deserialize(filePath),
            ".w3strings" => await w3StringsSerializer.Deserialize(filePath),
            _ => throw new NotSupportedException($"File format not supported: {filePath}")
        };
    }

    public async Task<bool> Serialize(IReadOnlyList<IW3StringItem> w3StringItems, W3SerializationContext context)
    {
        return context.TargetFileType switch
        {
            W3FileType.Csv => await csvW3Serializer.Serialize(w3StringItems, context),
            W3FileType.Excel => await excelW3Serializer.Serialize(w3StringItems, context),
            W3FileType.W3Strings => await w3StringsSerializer.Serialize(w3StringItems, context),
            _ => throw new NotSupportedException($"File type not supported: {context.TargetFileType}")
        };
    }
}