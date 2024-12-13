using System.IO;

namespace Witcher3StringEditor.Core.Helper
{
    public static class FileHelper
    {
        /// <summary>
        /// Creates a temporary directory with a random name under the system's temp path.
        /// </summary>
        /// <returns>The full path of the created directory.</returns>
        public static string CreateRandomTempDirectory()
        {
            string tempPath;

            do
            {
                // Generate a random directory name using Guid to ensure uniqueness.
                var randomDirName = Guid.NewGuid().ToString("N"); // Remove hyphens for a cleaner name.
                tempPath = Path.Combine(Path.GetTempPath(), randomDirName);
            }
            while (Directory.Exists(tempPath)); // Ensure the directory does not already exist.

            // Create the directory.
            Directory.CreateDirectory(tempPath);

            return tempPath;
        }
    }
}