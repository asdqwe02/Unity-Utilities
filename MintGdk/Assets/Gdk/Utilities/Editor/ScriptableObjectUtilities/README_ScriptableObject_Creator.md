# ScriptableObject Creator Utility

A Unity Editor utility that provides right-click context menu options for creating ScriptableObject assets directly from script files.

## 🔧 Features

- **Right-Click Creation**: Create ScriptableObject assets directly from script files via context menu
- **Automatic Detection**: Automatically detects scripts that inherit from ScriptableObject and have CreateAssetMenu attribute
- **Metadata Integration**: Uses CreateAssetMenu attribute metadata (fileName, menuName, order) for asset creation
- **Smart Naming**: Uses the fileName from CreateAssetMenu or falls back to class name
- **Intelligent Placement**: Places assets in appropriate folders, avoiding Editor directories
- **Unique Names**: Automatically generates unique asset names to prevent conflicts
- **Info Display**: Shows detailed information about the ScriptableObject configuration

## 🚀 How to Use

### Basic Usage

1. **Right-click on a script file** that inherits from ScriptableObject and has the `[CreateAssetMenu]` attribute
2. **Select "Create ScriptableObject Asset"** from the context menu
3. **The asset is created automatically** using the metadata from the CreateAssetMenu attribute

### Context Menu Options

The utility provides several context menu options when right-clicking on eligible script files:

- **"Create ScriptableObject Asset"** - Creates the asset using CreateAssetMenu metadata
- **"Create ScriptableObject from Selected Script"** - Alternative menu item for the same functionality
- **"ScriptableObject Info"** - Shows detailed information about the script's CreateAssetMenu configuration

## 📋 Requirements

For a script to be eligible for automatic asset creation, it must:

1. **Inherit from ScriptableObject**
2. **Have the CreateAssetMenu attribute**

### Example Script

```csharp
[CreateAssetMenu(fileName = "NewGameSettings", menuName = "Game/Settings", order = 1)]
public class GameSettings : ScriptableObject
{
    [SerializeField] private string gameName;
    [SerializeField] private float gameVersion;
    // ... other fields
}
```

## 🎯 Asset Creation Logic

### File Naming

The utility determines the asset name using this priority:

1. **CreateAssetMenu.fileName** - If specified in the attribute
2. **Class Name** - If no fileName is specified, uses `{ClassName}.asset`

### Asset Placement

The utility intelligently chooses where to place the created asset:

1. **Same Directory as Script** - Default placement
2. **Parent Directory** - If script is in an Editor folder, tries to place in parent
3. **Assets Root** - Fallback if other options aren't suitable

### Automatic Features

- **Unique Names**: Uses `AssetDatabase.GenerateUniqueAssetPath()` to prevent conflicts
- **Asset Extension**: Automatically adds `.asset` extension if not present
- **Auto-Selection**: Automatically selects the created asset in the Project window
- **Path Normalization**: Handles cross-platform path separators correctly

## 📖 Examples

### Example 1: Game Settings

```csharp
[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject
{
    public string gameName;
    public float gameVersion;
}
```

**Result**: Creates `GameSettings.asset` in the same folder as the script

### Example 2: Audio Configuration

```csharp
[CreateAssetMenu(fileName = "NewAudioConfig", menuName = "Audio/Configuration", order = 10)]
public class AudioConfiguration : ScriptableObject
{
    public AudioClip[] backgroundMusic;
    public float masterVolume = 1.0f;
}
```

**Result**: Creates `NewAudioConfig.asset` using the specified fileName

### Example 3: Default Naming

```csharp
[CreateAssetMenu(menuName = "Data/Player Stats")]
public class PlayerStats : ScriptableObject
{
    public int health;
    public int experience;
}
```

**Result**: Creates `PlayerStats.asset` using the class name as no fileName was specified

## 🔍 ScriptableObject Info Dialog

The utility includes an info dialog that shows:

- **Class Name** and Namespace
- **Menu Name** from CreateAssetMenu
- **File Name** that will be used for asset creation
- **Order** value from CreateAssetMenu
- **Preview** of the final asset name

Access this via right-click → "ScriptableObject Info"

## ⚙️ Technical Details

### Validation

The utility performs several checks before showing context menu options:

- Verifies single script file selection
- Confirms `.cs` file extension
- Checks ScriptableObject inheritance
- Validates CreateAssetMenu attribute presence

### Error Handling

- Graceful handling of compilation errors
- Clear error messages for invalid scripts
- Fallback paths for asset placement
- Protection against asset creation failures

### Performance

- Efficient reflection-based type analysis
- Minimal impact on Project window performance
- Only analyzes scripts when context menu is accessed

## 🐛 Troubleshooting

### Context Menu Not Appearing

- Ensure the script inherits from ScriptableObject
- Verify the CreateAssetMenu attribute is present
- Check that only one script file is selected
- Confirm the file has a `.cs` extension

### Asset Creation Failed

- Check for compilation errors in the script
- Verify write permissions to the target directory
- Ensure the ScriptableObject class is not abstract
- Check Unity console for specific error messages

### Wrong Asset Location

- Script in Editor folder → Asset placed in parent directory or Assets root
- No write permissions → Falls back to Assets root
- Path conflicts → Unique name generated automatically

---

*This utility streamlines ScriptableObject asset creation and integrates seamlessly with Unity's existing CreateAssetMenu workflow.*