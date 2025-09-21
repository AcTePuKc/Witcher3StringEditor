using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Serializers.Abstractions;

namespace Witcher3StringEditor.Serializers;

/// <summary>
///     Coordinates serialization operations for W3 string items across different file formats
///     Acts as a facade that delegates serialization requests to the appropriate specific serializer
///     based on file extension (for deserialization) or target file type (for serialization)
/// </summary>
public class W3SerializerCoordinator(
    ICsvW3Serializer csvW3Serializer,
    IExcelW3Serializer excelW3Serializer,
    IW3StringsSerializer w3StringsSerializer) : IW3Serializer
{
    /// <summary>
    ///     Deserializes W3 string items from a file
    ///     Determines the appropriate serializer based on the file extension
    /// </summary>
    /// <param name="filePath">The path to the file to deserialize</param>
    /// <returns>
    ///     A task that represents the asynchronous deserialize operation.
    ///     The task result contains the deserialized W3 string items
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the file format is not supported</exception>
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

    /// <summary>
    ///     Serializes W3 string items to a file
    ///     Determines the appropriate serializer based on the target file type specified in the context
    /// </summary>
    /// <param name="w3StringItems">The W3 string items to serialize</param>
    /// <param name="context">The serialization context containing the target file type and other parameters</param>
    /// <returns>
    ///     A task that represents the asynchronous serialize operation.
    ///     The task result indicates whether the serialization was successful
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the target file type is not supported</exception>
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