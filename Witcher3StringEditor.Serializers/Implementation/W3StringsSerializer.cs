using System.Diagnostics;
using CommandLine;
using CommunityToolkit.Diagnostics;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Serializers.Internal;

namespace Witcher3StringEditor.Serializers.Implementation;

/// <summary>
///     Provides W3Strings serialization functionality for The Witcher 3 string items
///     Implements the IW3StringsSerializer interface to handle reading from and writing to W3Strings files
///     This serializer uses an external tool (W3Strings encoder/decoder) to perform the actual serialization
/// </summary>
public class W3StringsSerializer(
    IAppSettings appSettings,
    IBackupService backupService,
    ICsvW3Serializer csvSerializer)
    : IW3StringsSerializer
{
    /// <summary>
    ///     Deserializes The Witcher 3 string items from a W3Strings file
    ///     This method uses an external tool to decode the W3Strings file into a CSV format,
    ///     then uses the CSV serializer to read the data
    /// </summary>
    /// <param name="filePath">The path to the W3Strings file to deserialize</param>
    /// <returns>
    ///     A task that represents the asynchronous deserialize operation.
    ///     The task result contains the deserialized The Witcher 3 string items, or an empty list if an error occurred
    /// </returns>
    public async Task<IReadOnlyList<IW3StringItem>> Deserialize(string filePath)
    {
        try
        {
            // Execute the external W3Strings decoder tool with the file to decode
            using var process = await ExecuteExternalProcess(appSettings.W3StringsPath,
                Parser.Default.FormatCommandLine(new W3StringsOptions
                {
                    InputFileToDecode = filePath
                }));

            // Ensure the process completed successfully (exit code 0)
            Guard.IsEqualTo(process.ExitCode, 0);
            
            // Use the CSV serializer to read the decoded data from the generated CSV file
            return await csvSerializer.Deserialize($"{filePath}.csv");
        }
        catch (Exception ex)
        {
            // Log any errors that occur during deserialization
            Log.Error(ex, "An error occurred while deserializing W3Strings file: {Path}.", filePath);
            
            // Return an empty list in case of errors
            return [];
        }
    }

    /// <summary>
    ///     Serializes The Witcher 3 string items to a W3Strings file
    ///     This method first creates a temporary CSV file, then uses an external tool to encode it into a W3Strings file
    /// </summary>
    /// <param name="w3StringItems">The Witcher 3 string items to serialize</param>
    /// <param name="context">
    ///     The serialization context containing output directory, target language,
    ///     and other serialization parameters
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous serialize operation.
    ///     The task result indicates whether the serialization was successful
    /// </returns>
    public async Task<bool> Serialize(IReadOnlyList<IW3StringItem> w3StringItems, W3SerializationContext context)
    {
        try
        {
            // Ensure there are items to serialize
            Guard.IsNotEmpty(w3StringItems);
            
            // Get the lowercase name of the target language for file naming
            var saveLang = Enum.GetName(context.TargetLanguage)!.ToLowerInvariant();
            
            // Create a temporary directory for intermediate files
            var tempDirectory = Directory.CreateTempSubdirectory().FullName;
            
            // Define paths for temporary CSV and W3Strings files
            var tempCsvPath = Path.Combine(tempDirectory, $"{saveLang}.csv");
            var tempW3StringsPath = Path.ChangeExtension(tempCsvPath, ".csv.w3strings");
            
            // Define the final output path for the W3Strings file
            var outputW3StringsPath = Path.Combine(context.OutputDirectory, $"{saveLang}.w3strings");
            
            // Create a temporary context with the temp directory as output
            var tempContext = context with
            {
                OutputDirectory = tempDirectory
            };
            
            // Serialize the items to a temporary CSV file
            Guard.IsTrue(await csvSerializer.Serialize(w3StringItems, tempContext));
            
            // Encode the temporary CSV file to W3Strings format
            Guard.IsTrue(await StartSerializationProcess(tempContext, tempCsvPath));
            
            // Replace the destination file with backup if needed
            Guard.IsTrue(ReplaceFileWithBackup(tempW3StringsPath, outputW3StringsPath));
            
            // Return true to indicate successful serialization
            return true;
        }
        catch (Exception ex)
        {
            // Log any errors that occur during serialization
            Log.Error(ex, "An error occurred while serializing W3Strings.");
            
            // Return false to indicate serialization failure
            return false;
        }
    }

    /// <summary>
    ///     Replaces a destination file with a source file, creating a backup of the destination file if it exists
    /// </summary>
    /// <param name="sourceFilePath">The path to the source file</param>
    /// <param name="destinationFilePath">The path to the destination file</param>
    /// <returns>True if the replacement was successful, false otherwise</returns>
    private bool ReplaceFileWithBackup(string sourceFilePath, string destinationFilePath)
    {
        // Check if the destination file exists and create a backup if needed
        if (File.Exists(destinationFilePath) && !backupService.Backup(destinationFilePath))
            // Return false if backup creation failed
            return false;
            
        // Copy the source file to the destination, overwriting if necessary
        File.Copy(sourceFilePath, destinationFilePath, true);
        
        // Return true to indicate successful replacement
        return true;
    }

    /// <summary>
    ///     Starts the serialization process by executing the external W3Strings encoder tool
    /// </summary>
    /// <param name="context">The serialization context containing serialization parameters</param>
    /// <param name="path">The path to the temporary CSV file to encode</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result indicates whether the process completed successfully (exit code 0)
    /// </returns>
    private async Task<bool> StartSerializationProcess(W3SerializationContext context, string path)
    {
        // Execute the external W3Strings encoder tool with appropriate arguments based on context
        using var process = await ExecuteExternalProcess(appSettings.W3StringsPath, context.IgnoreIdSpaceCheck
            // If ignoring ID space check, pass the ignore flag
            ? Parser.Default.FormatCommandLine(new W3StringsOptions
                { InputFileToEncode = path, IgnoreIdSpaceCheck = true })
            // Otherwise, pass the expected ID space
            : Parser.Default.FormatCommandLine(new W3StringsOptions
                { InputFileToEncode = path, ExpectedIdSpace = context.ExpectedIdSpace }));
                
        // Return true if the process completed successfully (exit code 0)
        return process.ExitCode == 0;
    }

    /// <summary>
    ///     Executes an external process with the specified filename and arguments
    ///     Captures and logs both standard output and error output
    /// </summary>
    /// <param name="filename">The filename of the executable to run</param>
    /// <param name="arguments">The arguments to pass to the executable</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains the completed Process object
    /// </returns>
    private static async Task<Process> ExecuteExternalProcess(string filename, string arguments)
    {
        // Create a new process with the specified filename and arguments
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
        
        // Attach event handlers for error and output data
        process.ErrorDataReceived += Process_ErrorDataReceived;
        process.OutputDataReceived += Process_OutputDataReceived;
        
        // Start the process
        process.Start();
        
        // Begin asynchronous reading of error and output streams
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        
        // Wait for the process to exit
        await process.WaitForExitAsync();
        
        // Return the completed process
        return process;
    }

    /// <summary>
    ///     Handles the ErrorDataReceived event of the external process
    ///     Logs error output from the process
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The DataReceivedEventArgs instance containing the event data</param>
    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        // Log error data if it's not null or whitespace
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Error("Error: {Data}.", e.Data);
    }

    /// <summary>
    ///     Handles the OutputDataReceived event of the external process
    ///     Logs standard output from the process
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The DataReceivedEventArgs instance containing the event data</param>
    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        // Log output data if it's not null or whitespace
        if (!string.IsNullOrWhiteSpace(e.Data))
            Log.Information("Output: {Data}.", e.Data);
    }
}