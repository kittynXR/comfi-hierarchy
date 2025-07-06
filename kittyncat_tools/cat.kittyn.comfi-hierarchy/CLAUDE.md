# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
ComfiHierarchy is a Unity Editor extension that enhances the hierarchy window with visual improvements and component management features. It's an editor-only package with no runtime components.

## Development Environment
- Unity 2019.4+ required
- C# language standard follows Unity's defaults
- All code must be in Editor assembly (marked with Editor platform only)
- No external package dependencies

## Architecture

### Core Classes
- **ComfiHierarchy.cs**: Main settings window, handles preference UI
- **ComfiHierarchyDrawer.cs**: Core rendering logic for hierarchy items, handles GUI events and drawing
- **ComfiSettings.cs**: ScriptableObject for persistent settings storage
- **IconManager.cs**: Dynamic icon loading system that discovers PNG files in Icons/ subdirectories

### Icon System
Icons are automatically discovered from the Icons/ folder structure:
- Components/ - Unity built-in component icons
- VRCSDK/ - VRChat SDK component icons  
- VRCCommunity/ - Community tool icons (AudioLink, VRCFury, etc.)
- Custom/ - User-added custom icons
- Misc/ - Default/fallback icons

Icon naming convention: ComponentName.png (exact match with Unity component class name)

## Key Implementation Details

1. **Hierarchy Drawing**: Uses EditorApplication.hierarchyWindowItemOnGUI callback
2. **Component Detection**: Iterates through GameObject components to find matching icons
3. **Settings Storage**: Uses EditorPrefs and ScriptableObject in Assets/ComfiSettings/
4. **Icon Loading**: Lazy loads textures on first use, caches in memory
5. **Performance**: Must minimize allocations in OnGUI calls

## Common Tasks

### Adding New Icon Categories
1. Create new folder under Icons/
2. Icons will be auto-discovered on next domain reload
3. No code changes needed

### Modifying Drawing Behavior
Edit ComfiHierarchyDrawer.cs HierarchyWindowItemOnGUI method

### Adding New Settings
1. Add property to ComfiSettings.cs
2. Add UI control in ComfiHierarchy.cs OnGUI method
3. Apply setting in ComfiHierarchyDrawer.cs

## Unity-Specific Considerations
- Always use EditorGUI/EditorGUILayout for UI
- Respect Unity's immediate mode GUI patterns
- Use EditorUtility for progress bars, dialogs
- Test with domain reloads and play mode transitions
- Icons must be imported as Editor GUI and Legacy GUI type