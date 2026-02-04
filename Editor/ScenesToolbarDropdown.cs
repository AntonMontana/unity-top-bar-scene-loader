using UnityEditor;
using UnityEditor.Toolbars;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Scene Selector Dropdown for Unity 6 Main Toolbar.
/// Displays all project scenes in a dropdown menu next to Play/Pause/Stop buttons.
/// Scenes from the priority folder are shown at the top with ★ icon.
/// </summary>
public class ScenesToolbarDropdown
{
    // Scenes in this folder will be shown at the top of the menu with ★ icon
    private const string PriorityScenesFolder = "Assets/Scenes/";
    
    private static MainToolbarDropdown _dropdown;
    private static string _lastSceneName = "";
    private static bool _initialized;

    /// <summary>
    /// Creates the toolbar dropdown element.
    /// Called automatically by Unity's MainToolbar system.
    /// </summary>
    [MainToolbarElement("SceneSelector/Dropdown", defaultDockPosition = MainToolbarDockPosition.Middle)]
    public static MainToolbarElement SceneSelectorDropdown()
    {
        var content = new MainToolbarContent(text: GetCurrentSceneName());
        _dropdown = new MainToolbarDropdown(content, ShowSceneMenu);
        
        // Subscribe to scene events only once
        if (!_initialized)
        {
            EditorSceneManager.sceneOpened += (s, m) => RefreshContent();
            EditorSceneManager.sceneClosed += s => RefreshContent();
            EditorSceneManager.sceneSaved += s => RefreshContent();
            EditorApplication.update += OnEditorUpdate;
            _initialized = true;
        }
        
        return _dropdown;
    }

    private static double _lastUpdateTime;
    
    /// <summary>
    /// Periodically checks if scene name changed (for dirty flag updates).
    /// </summary>
    private static void OnEditorUpdate()
    {
        // Check every 0.5 seconds to update dirty indicator (*)
        if (EditorApplication.timeSinceStartup - _lastUpdateTime > 0.5)
        {
            _lastUpdateTime = EditorApplication.timeSinceStartup;
            
            string currentName = GetCurrentSceneName();
            if (currentName != _lastSceneName)
            {
                RefreshContent();
            }
        }
    }

    /// <summary>
    /// Updates the dropdown button text with current scene name.
    /// </summary>
    private static void RefreshContent()
    {
        _lastSceneName = GetCurrentSceneName();
        
        if (_dropdown != null)
        {
            // MainToolbarContent is a struct, so we need to copy, modify, and reassign
            var newContent = _dropdown.content;
            newContent.text = _lastSceneName;
            _dropdown.content = newContent;
        }
        
        // Force toolbar to repaint
        MainToolbar.Refresh("SceneSelector/Dropdown");
    }

    /// <summary>
    /// Returns formatted current scene name with dirty indicator.
    /// </summary>
    private static string GetCurrentSceneName()
    {
        var activeScene = SceneManager.GetActiveScene();
        string sceneName = string.IsNullOrEmpty(activeScene.name) ? "No Scene" : activeScene.name;
        
        // Add * if scene has unsaved changes
        if (activeScene.isDirty)
        {
            sceneName += "*";
        }

        return $"🎬 {sceneName}";
    }

    /// <summary>
    /// Displays the scene selection dropdown menu.
    /// </summary>
    /// <param name="buttonRect">Position of the toolbar button for menu alignment</param>
    private static void ShowSceneMenu(Rect buttonRect)
    {
        var menu = new GenericMenu();
        
        var priorityScenes = new List<string>();
        var otherScenes = new List<string>();
        
        // Find all scenes in project
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        string currentScenePath = SceneManager.GetActiveScene().path;
        var activeScene = SceneManager.GetActiveScene();

        // Separate priority scenes from others
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".unity"))
                continue;

            if (path.StartsWith(PriorityScenesFolder))
                priorityScenes.Add(path);
            else
                otherScenes.Add(path);
        }

        // Sort alphabetically
        priorityScenes = priorityScenes.OrderBy(Path.GetFileNameWithoutExtension).ToList();
        otherScenes = otherScenes.OrderBy(Path.GetFileNameWithoutExtension).ToList();

        // Current scene header (always at top, disabled)
        string currentName = string.IsNullOrEmpty(activeScene.name) ? "No Scene" : activeScene.name;
        string dirtyMark = activeScene.isDirty ? " *" : "";
        menu.AddDisabledItem(new GUIContent($"▶ {currentName}{dirtyMark}"));
        menu.AddSeparator("");

        // Priority scenes (from Assets/Scenes/) - shown at root level with ★
        foreach (string scenePath in priorityScenes)
        {
            string name = Path.GetFileNameWithoutExtension(scenePath);
            bool isCurrent = scenePath == currentScenePath;

            if (isCurrent)
            {
                // Current scene is checked but not clickable
                menu.AddItem(new GUIContent($"★ {name}"), true, () => { });
            }
            else
            {
                string path = scenePath; // Capture for closure
                menu.AddItem(new GUIContent($"★ {name}"), false, () => OpenScene(path));
            }
        }

        // Separator between priority and other scenes
        if (priorityScenes.Count > 0 && otherScenes.Count > 0)
        {
            menu.AddSeparator("");
        }

        // Other scenes - grouped by folder in submenus
        if (otherScenes.Count > 0)
        {
            var groupedScenes = otherScenes
                .GroupBy(p => Path.GetDirectoryName(p))
                .OrderBy(g => g.Key);

            foreach (var group in groupedScenes)
            {
                string folder = group.Key.Replace("\\", "/");
                if (folder.StartsWith("Assets/"))
                {
                    folder = folder.Substring("Assets/".Length);
                }
                    
                if (string.IsNullOrEmpty(folder)) folder = "Other";

                foreach (string scenePath in group.OrderBy(Path.GetFileNameWithoutExtension))
                {
                    string name = Path.GetFileNameWithoutExtension(scenePath);
                    bool isCurrent = scenePath == currentScenePath;
                    string menuPath = $"{folder}/{name}";

                    if (isCurrent)
                    {
                        menu.AddItem(new GUIContent(menuPath), true, () => { });
                    }
                    else
                    {
                        string path = scenePath; // Capture for closure
                        menu.AddItem(new GUIContent(menuPath), false, () => OpenScene(path));
                    }
                }
            }
        }

        // Show message if no scenes found
        if (priorityScenes.Count == 0 && otherScenes.Count == 0)
        {
            menu.AddDisabledItem(new GUIContent("No scenes found"));
        }

        // Utility: ping current scene in Project window
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("📍 Ping in Project"), false, () =>
        {
            if (!string.IsNullOrEmpty(currentScenePath))
            {
                var obj = AssetDatabase.LoadAssetAtPath<SceneAsset>(currentScenePath);
                if (obj != null)
                {
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeObject = obj;
                }
            }
        });

        // Show menu below the button
        menu.DropDown(buttonRect);
    }

    /// <summary>
    /// Opens a scene with save prompt if current scene has unsaved changes.
    /// </summary>
    /// <param name="scenePath">Full path to the scene asset</param>
    private static void OpenScene(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath)) return;

        var activeScene = SceneManager.GetActiveScene();
        
        // Prompt to save if current scene has unsaved changes
        if (activeScene.isDirty)
        {
            int result = EditorUtility.DisplayDialogComplex(
                "Scene Has Been Modified",
                $"Do you want to save changes to '{activeScene.name}' before opening another scene?",
                "Save", "Don't Save", "Cancel"
            );

            switch (result)
            {
                case 0: // Save
                    EditorSceneManager.SaveScene(activeScene);
                    break;
                case 1: // Don't Save
                    break;
                case 2: // Cancel
                    return;
            }
        }

        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }
}