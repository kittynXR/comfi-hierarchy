# ComfiHierarchy

A clean, extensible Unity hierarchy enhancement tool with easy icon management and quality-of-life features.

## Features

- **Dynamic Icon System**: Automatically loads icons from the Icons folder
- **Easy Icon Management**: Simply drop new PNG files to add support for new components
- **Component Toggle**: Click icons to enable/disable components
- **Drag Toggle**: Drag across multiple components to toggle them
- **Tree Guide Lines**: Visual hierarchy connections
- **Row Coloring**: Alternating row colors for better readability
- **Layer/Tag Labels**: See GameObject layers and tags at a glance
- **Customizable**: Comprehensive settings window
- **Performance Optimized**: Efficient rendering with minimal overhead

## Installation

1. Copy the `com.comfi.hierarchy` folder to your Unity project's `Packages` folder
2. Unity will automatically import the package
3. Access settings via `Window > ComfiHierarchy Settings`

## Adding Custom Icons

### Method 1: Simple File Drop
1. Add your PNG icon to `Packages/com.comfi.hierarchy/Icons/Custom/`
2. Name it exactly as the component type (e.g., `MyCustomScript.png`)
3. Click "Refresh Icons" in the settings window

### Method 2: Organized Folders
Create subfolders in the Icons directory to organize your icons:
```
Icons/
├── Components/
├── Custom/
│   ├── MyProject/
│   │   ├── PlayerController.png
│   │   └── HealthManager.png
│   └── ThirdParty/
│       └── SomeAsset.png
```

### Method 3: Runtime Mapping
```csharp
// In your editor scripts
ComfiHierarchy.IconManager.AddCustomMapping("MyScript", "path/to/icon.png");
```

## Settings

- **General**: Enable/disable the entire system
- **Icons**: Configure icon display options
- **Visual Features**: Tree lines, row coloring, labels
- **Colors**: Customize all colors to match your theme
- **Advanced**: Hide specific component types, debug mode

## Tips

- Icons are loaded from all subdirectories automatically
- Use 16x16 or 32x32 PNG files for best results
- Component names must match exactly (case-sensitive)
- The system falls back to Unity's default icons if no custom icon is found

## License

This package includes icons from Icons8.com and respects their licensing terms.
Your custom icons remain your property.

## Troubleshooting

**Icons not showing up?**
- Click "Refresh Icons" in settings
- Check that PNG files are named correctly
- Ensure icons are in the Icons folder or subfolders

**Performance issues?**
- Disable features you don't need in settings
- Consider hiding frequently used but unimportant components (like MeshFilter)

**Can't find settings?**
- Window > ComfiHierarchy Settings
- Settings are saved as a ScriptableObject in your project