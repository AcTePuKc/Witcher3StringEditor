# Witcher3StringEditor

A powerful tool for editing string resources in *The Witcher 3: Wild Hunt*, allowing users to modify in-game text such as dialogues, quest descriptions, and UI elements with ease.

## Features

- **String Editing**: Add, modify, or delete string entries in Witcher 3 string files.
- **Automatic Backups**: Files are automatically backed up when saved, with no manual intervention required. Backups are stored securely and can be managed via the backup dialog.
- **File Management**: Open, save, and handle string files with support for drag-and-drop operations. Supported formats include:
  - Witcher3 Strings File (`.w3strings`)
  - CSV Files (`.csv`)
  - Excel Worksheets (`.xlsx`)
- **Recent Files**: Track and quickly access recently opened files through the "Recent" dialog.
- **Localization**: The application automatically adapts to your system's language settings, selecting the closest matching language for the interface.
- **Translation Helper**: Built-in translation tool to assist with localizing string entries, with support for batch translation and character limits (max 1,000 characters per translation).
- **Update Checks**: Automatic update checks to ensure you're using the latest version.
- **Integration**: Directly launch the game from the tool (with correct game path setup in settings).
- **Search & Pagination**: Efficiently find entries with search functionality and handle large files with paginated display.

## Installation

1. Download the latest release from the [Nexus Mods page](https://www.nexusmods.com/witcher3/mods/10032).
2. Extract the zip file to your desired location.
3. Run `Witcher3StringEditor.exe` to launch the application.

## Required Dependencies

- **w3strings Encoder**: This tool requires the `w3strings.exe` utility for proper encoding/decoding of Witcher 3 string files. Download it from the [official Nexus Mods page](https://www.nexusmods.com/witcher3/mods/1055).

## First-Run Setup

On your first launch, you'll be prompted to:
1. **Set the path to `w3strings.exe`** (downloaded from the [w3strings Encoder page](https://www.nexusmods.com/witcher3/mods/1055))—this is required for proper file handling.
2. Optionally set the path to `witcher3.exe` to enable direct game launching from the tool.

## Usage

### Basic Operations
- **Open a File**: Click the "Open" button, use the "Recent" menu, or drag-and-drop a supported file (`.w3strings`, `.csv`, `.xlsx`) into the application. A prompt will warn you if opening a new file will overwrite unsaved changes.
- **Edit Entries**: Select a string entry in the table to modify its content. Use the "Add" or "Delete" buttons to manage entries.
- **Save Changes**: Click "Save" to persist modifications—**backups are created automatically** (stored in `%AppData%\Witcher3StringEditor\Backup`) to prevent data loss.
- **Manage Backups**: Access the backup dialog to view, restore, or delete auto-generated backups. Restoring a backup will overwrite the current file (with confirmation).

### Advanced Features
- **Translation Tool**: Select an entry and use the "Translate" button to open the translation tool. Note that:
  - Translations are limited to 1,000 characters.
  - Switching translation modes during an active translation will interrupt the process.
  - Existing translations can be overwritten with confirmation.
- **Settings**: Customize preferences in the Settings dialog, including:
  - Encoder path (`w3strings.exe`)
  - Game executable path (`witcher3.exe`)
  - Preferred file type for new/saved files
- **Log Viewer**: Access the log dialog to review operation history, including timestamps and status messages.
- **Nexus Mods Integration**: Click the "Nexus Mods" button to visit the [mod page](https://www.nexusmods.com/witcher3/mods/10032) for updates, support, and resources.

## Screenshots

![Main Window](https://staticdelivery.nexusmods.com/mods/952/images/10032/10032-1755524172-1319856400.png)  
*Main interface showing string entries in a table view with search and pagination*

![Backup Dialog](https://staticdelivery.nexusmods.com/mods/952/images/10032/10032-1739770257-1910199133.png)  
*Manage auto-generated backups with restore/delete options*

![Translation Tool](https://staticdelivery.nexusmods.com/mods/952/images/10032/10032-1755524172-877884616.png)  
*Built-in translation helper with source/target language support*

## Dependencies

- .NET Framework (version specified in release notes)
- Syncfusion WPF Controls (for data grid functionality)
- iNKORE.UI.WPF.Modern (for modern UI components)
- CommunityToolkit.Mvvm (for MVVM pattern implementation)
- [w3strings Encoder](https://www.nexusmods.com/witcher3/mods/1055) (required for `.w3strings` file handling)

## Development

To build the project locally:
1. Clone the repository.
2. Open `Witcher3StringEditor.sln` in Visual Studio.
3. Restore NuGet packages.
4. Build the solution (supports Debug/Release configurations).

**Note**: The `.gitignore` file excludes Visual Studio temporary files, build outputs, and sensitive data (e.g., `NexusApiKey.txt`), ensuring clean version control.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Thanks to the Witcher 3 modding community for research and resources.
- Uses [Syncfusion](https://www.syncfusion.com/) controls for data visualization.
- Uses [iNKORE.UI.WPF.Modern](https://github.com/inkore-net/UI.WPF.Modern) for modern UI elements.
- Relies on the [w3strings Encoder](https://www.nexusmods.com/witcher3/mods/1055) for `.w3strings` file processing.

## Support

For issues, feature requests, or questions, please visit the [Nexus Mods page](https://www.nexusmods.com/witcher3/mods/10032) where you can report problems, leave comments, and get support from the community and developers.