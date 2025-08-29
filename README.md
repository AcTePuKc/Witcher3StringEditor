Witcher3StringEditor
 
A powerful tool for editing string resources in The Witcher 3: Wild Hunt, allowing users to modify in-game text such as dialogues, quest descriptions, and UI elements with ease.
 
Features
 
- String Editing: Add, modify, or delete string entries in Witcher 3 string files.
- Automatic Backups: Files are automatically backed up when saved (no manual intervention needed), with backups manageable via the backup dialog.
- File Management: Open/save string files with drag-and-drop support; compatible with  .w3strings  (Witcher 3 native),  .csv , and  .xlsx  formats.
- Recent Files: Track and quickly access recently opened files via the "Recent" dialog.
- Localization: Auto-adapts to system language settings for the interface.
- Translation Helper: Built-in tool for localizing entries (batch translation support, max 1,000 characters per translation).
- Update Checks: Automatic checks to ensure you’re using the latest version.
- Game Integration: Launch The Witcher 3 directly from the tool (requires setting  witcher3.exe  path in settings).
- Search & Pagination: Efficiently find entries with search, and handle large files via paginated display.
 
Installation
 
1. Download the latest release from the Nexus Mods page.
2. Extract the zip file to your desired location.
3. Run  Witcher3StringEditor.exe  to launch the application.
 
Required External Dependencies (For End Users)
 
These are mandatory tools/libraries you must install/configure to use the software:
 
1. .NET 8 Runtime
- Required for the application to launch and run.
- Download the matching version for your system architecture (x64/x86) from the Microsoft Official Page.
2. w3strings Encoder
- Required for encoding/decoding Witcher 3’s  .w3strings  files.
- Download from the official Nexus Mods page, then specify its path during the tool’s first-run setup.
 
First-Run Setup
 
On your first launch, you’ll be prompted to:
 
1. Set the path to  w3strings.exe  (from the w3strings Encoder download) – this is mandatory for  .w3strings  file handling.
2. Optionally set the path to  witcher3.exe  to enable direct game launching.
 
Usage
 
Basic Operations
 
- Open a File: Click "Open", use the "Recent" menu, or drag-and-drop a supported file ( .w3strings / .csv / .xlsx ). A prompt warns if unsaved changes will be overwritten.
- Edit Entries: Select an entry in the table to modify; use "Add" or "Delete" to manage entries.
- Save Changes: Click "Save" to persist edits – backups are auto-created (stored in  %AppData%\Witcher3StringEditor\Backup ).
- Manage Backups: Access the backup dialog to view, restore, or delete backups (restoring overwrites the current file with confirmation).
 
Advanced Features
 
- Translation Tool: Select an entry and click "Translate" to use the tool. Note: Switching modes mid-translation interrupts progress; overwriting existing translations requires confirmation.
- Settings: Customize preferences (encoder path, game path, preferred save file type) in the Settings dialog.
- Log Viewer: Check operation history (with timestamps) via the log dialog.
- Nexus Mods Integration: Click "Nexus Mods" to visit the tool’s mod page for updates.
 
Screenshots
 
Main Window
Main interface with string entries, search, and pagination
 
Backup Dialog
Backup management with restore/delete options
 
Translation Tool
Built-in translation helper with source/target language support
 
License
 
This project is licensed under the MIT License – see the LICENSE file for details.
 
Support
 
For issues, feature requests, or questions, visit the Nexus Mods page to report problems, leave comments, or get community support.
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

**Note**: The `.gitignore` file excludes Visual Studio temporary files, build outputs, and sensitive data , ensuring clean version control.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Thanks to the Witcher 3 modding community for research and resources.
- Uses [Syncfusion](https://www.syncfusion.com/) controls for data visualization.
- Uses [iNKORE.UI.WPF.Modern](https://github.com/inkore-net/UI.WPF.Modern) for modern UI elements.
- Relies on the [w3strings Encoder](https://www.nexusmods.com/witcher3/mods/1055) for `.w3strings` file processing.

## Support

For issues, feature requests, or questions, please visit the [Nexus Mods page](https://www.nexusmods.com/witcher3/mods/10032) where you can report problems, leave comments, and get support from the community and developers.