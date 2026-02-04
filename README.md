# 🎬 Top Bar Scene Loader for Unity 6

<img width="680" height="337" alt="Image" src="https://github.com/user-attachments/assets/7c638520-04a8-438d-8574-a7a31aa07628" />

A simple editor extension that adds a scene selector dropdown to Unity's main toolbar (next to Play/Pause/Stop buttons).

![Unity](https://img.shields.io/badge/Unity-6000.3+-black?logo=unity)
![License](https://img.shields.io/badge/License-MIT-green)

## ✨ Features

- **Quick scene switching** directly from the toolbar
- **Priority scenes** from `Assets/Scenes/` folder shown at the top with ★ icon
- **Current scene display** with dirty indicator (`*` when unsaved)
- **Auto-save prompt** when switching scenes with unsaved changes
- **Organized menu** — other scenes grouped by folders
- **Ping in Project** — quickly locate current scene in Project window

## 📸 Preview

```
┌─────────────────────────────────┐
│  🎬 MainMenu*  ▼                │  ← Toolbar button (shows current scene)
├─────────────────────────────────┤
│  ▶ MainMenu *                   │  ← Current scene (disabled)
│  ─────────────────────────────  │
│  ★ MainMenu           ✓         │  ← Priority scenes (Assets/Scenes/)
│  ★ GameScene                    │
│  ★ Settings                     │
│  ─────────────────────────────  │
│  Levels/Level1                  │  ← Other scenes (by folder)
│  Levels/Level2                  │
│  Tests/TestScene                │
│  ─────────────────────────────  │
│  📍 Ping in Project             │
└─────────────────────────────────┘
```

## 📦 Installation

### Option 1: Copy Script
1. Download `ScenesToolbarDropdown.cs`
2. Place it in your project's `Assets/Editor/` folder
3. Wait for Unity to recompile

### Option 2: Unity Package Manager (Git URL)
```
https://github.com/AntonMontana/unity-top-bar-scene-loader.git
```

## 🚀 Usage

### First Time Setup
1. After installation, look for the **three dots menu (⋮)** on Unity's main toolbar
2. Find and enable **"SceneSelector/Dropdown"**
3. The dropdown will now appear in your toolbar
<img width="689" height="277" alt="Image" src="https://github.com/user-attachments/assets/2c9303a4-a7ee-4223-9386-71b97e190f14" />

### Switching Scenes
1. Click the **🎬 SceneName** dropdown in the toolbar
2. Select any scene from the menu
3. If current scene has unsaved changes, you'll be prompted to save

### Priority Scenes
Scenes located in `Assets/Scenes/` folder are marked with ★ and displayed at the top of the menu for quick access.

To change the priority folder, modify this line in the script:
```csharp
private const string PriorityScenesFolder = "Assets/Scenes/";
```

## ⚙️ Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| `PriorityScenesFolder` | `"Assets/Scenes/"` | Folder for priority scenes (shown with ★) |
| Update interval | `0.5s` | How often the button text refreshes |

## 📋 Requirements

- **Unity 6000.3+** (Unity 6)
- Uses `UnityEditor.Toolbars` API (MainToolbarElement)

## 🔧 API Reference

The extension uses Unity 6's new `MainToolbarElement` attribute:

```csharp
[MainToolbarElement("SceneSelector/Dropdown", defaultDockPosition = MainToolbarDockPosition.Middle)]
public static MainToolbarElement SceneSelectorDropdown()
```

### Key Methods

| Method | Description |
|--------|-------------|
| `SceneSelectorDropdown()` | Creates the toolbar element |
| `RefreshContent()` | Updates button text with current scene name |
| `ShowSceneMenu(Rect)` | Displays the scene selection dropdown |
| `OpenScene(string)` | Opens a scene with save prompt |

## 🐛 Troubleshooting

### Dropdown not appearing
1. Check that the script is in an `Editor` folder
2. Look for compile errors in Console
3. Enable it manually via toolbar's three-dot menu

### Text not updating
- The button refreshes every 0.5 seconds
- Call `MainToolbar.Refresh("SceneSelector/Dropdown")` forces immediate refresh

### Scenes not showing
- Ensure scenes are saved as `.unity` files
- Check that scenes are inside `Assets/` folder

## 📄 License

MIT License - feel free to use in any project.

## 📝 Changelog

### v1.0.0
- Initial release
- Scene dropdown in main toolbar
- Priority scenes support
- Auto-save prompt
- Ping in Project feature
