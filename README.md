# cátte — Comfi Hierarchy

QoL Hierarchy UI for VRChat Creators — Customizable

![ComfiHierarchy Enhancement](./screenshots/comfi-hierarchy-hero-image.png)

## Installation

### VRChat Creator Companion (Recommended)

1. Add the repository to VCC: `https://comfi-hierarchy.kittyn.cat/index.json`
2. Find "cátte — Comfi Hierarchy" in the package list
3. Click "Add" to install to your project

### Manual Installation

1. Download the latest release from [Releases](https://github.com/kittynXR/comfi-hierarchy/releases)
2. Extract the zip file to your Unity project's `Packages` folder

## Requirements

- Unity 2019.4.31f1 or later
- VRChat SDK 3.7.0 or later

## Features

### 🎨 Visual Enhancements
- **Custom Component Icons** - Intelligent icon system with 200+ icons for Unity components, VRChat SDK, and popular community tools
- **Row Coloring** - Alternating hierarchy row colors for better visual separation
- **Tree Guide Lines** - Visual lines showing parent-child relationships
- **Layer & Tag Labels** - Display GameObject layers and tags directly in hierarchy
- **Icon Tooltips** - Hover over icons to see component names

### 🔧 Smart Icon System
- **Automatic Icon Matching** - Wildcard system finds best icon match for components
- **Enhanced Third-Party Detection** - Automatically discovers custom icons from third-party scripts
- **Organized Categories** - Icons grouped by Unity, VRChat SDK, Community tools, and Custom
- **Intelligent Fallback Chain** - Multiple fallback methods ensure icons are found
- **Dynamic Loading** - Icons loaded on-demand for optimal performance

### ⚙️ Customization Options
- **Toggle Individual Features** - Enable/disable icons, tree lines, row coloring, and labels
- **Color Customization** - Customize colors for active/inactive icons, tree lines, and row backgrounds
- **Icon Positioning** - Adjust icon X-offset for perfect alignment
- **Label Width Control** - Customize layer/tag label width
- **Component Filtering** - Hide specific component types from icon display
- **Third-Party Icon Detection** - Configure advanced detection methods for performance vs accuracy

## Usage

### Getting Started

1. **Access Settings**: Go to `Tools > ⚙️🎨 kittyn.cat 🐟 > 🐟 ComfiHierarchy Settings`
2. **Enable Features**: Toggle the visual enhancements you want to use
3. **Customize Colors**: Adjust colors to match your preferred Unity theme
4. **Set Icon Position**: Use the X-offset slider to position icons perfectly

### Common Workflows

#### Setting Up for VRChat Development
```
1. Enable "Show Icons" for component identification
2. Enable "Show Tree Lines" for complex avatar hierarchies
3. Enable "Show Row Coloring" for better visual organization
4. Adjust colors to match your Unity theme (Dark/Light)
```

#### Customizing for Your Workflow
```
1. Use "Hide Components" to filter out unwanted component icons
2. Adjust "Label Width" to show more/less layer and tag information
3. Use "Icon Preview" to see all available icons
4. Enable "Debug Mode" for troubleshooting icon loading
```

#### Third-Party Icon Detection Settings
```
1. Enable "Third-Party Icon Detection" for enhanced script icon support
2. Toggle "MonoScript Icon Detection" for Unity's native script icons
3. Enable "AssetDatabase Icon Search" to find icons in script directories
4. Use "Plugin Icon Detection" for VRChat community tool support
5. Run "Test Third-Party Icon Detection" to verify detection is working
```

### Advanced Features

#### Icon System & Selection Priority

ComfiHierarchy uses an intelligent fallback system to find the best icon for each component:

1. **Custom Icons (Exact Match)** - Direct match from Icons/ folder (e.g., `ModularAvatarBoneProxy.png`)
2. **Custom Icons (Prefix Match)** - Progressive prefix matching (e.g., `ModularAvatarBone` → `ModularAvatar`)
3. **Third-Party Script Detection** - Enhanced detection for custom script icons:
   - **MonoScript Icons** - Uses Unity's native script icon system
   - **Directory Search** - Looks for icons in the same folder as the script
   - **Plugin Detection** - Specialized detection for VRChat community tools
4. **Unity Built-in Icons** - Unity's native component icons
5. **Unity Type Thumbnails** - Unity's generic type icons
6. **Default Fallback** - ComfiHierarchy's default icon

#### Third-Party Icon Detection

The enhanced detection system automatically finds custom icons from third-party scripts:

- **MonoScript-based Detection**: Uses Unity's icon system to find script-specific icons
- **AssetDatabase Search**: Searches for icon files in script directories
- **Plugin-specific Detection**: Handles VRChat community tools (ModularAvatar, VRCFury, etc.)
- **Performance Controls**: Configurable settings to balance detection accuracy vs performance

#### Testing & Debugging

- **Test Menu**: `Tools > ⚙️🎨 kittyn.cat 🐟 > 🧪 Test Third-Party Icon Detection`
- **Debug Mode**: Enable in settings to see detailed icon loading information
- **Icon Preview**: View all loaded icons and their sources

#### Supported Tools & Components
- **Unity Built-in**: Transform, Renderer, Collider, Rigidbody, etc.
- **VRChat SDK**: VRCPhysBone, VRCContactReceiver, VRCStation, etc.
- **Community Tools**: ModularAvatar, VRCFury, AudioLink, Constraint Track, etc.
- **Custom Icons**: Add your own icons to the Custom/ folder

## Screenshots

![Settings Window](./screenshots/interface/comfi-hierarchy-settings-window.png)
*ComfiHierarchy Settings Window with all customization options*

![Icon Preview](./screenshots/interface/comfi-hierarchy-icon-preview.png)
*Icon Preview showing all available component icons*

![Before and After Comparison](./screenshots/before-after/comfi-hierarchy-comparison.png)
*Unity Hierarchy before and after ComfiHierarchy enhancements*

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- [Issues](https://github.com/kittynXR/comfi-hierarchy/issues)
- [Discussions](https://github.com/kittynXR/comfi-hierarchy/discussions)