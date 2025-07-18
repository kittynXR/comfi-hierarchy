using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Comfi.Hierarchy
{
    /// <summary>
    /// Manages icon loading and caching for ComfiHierarchy
    /// </summary>
    public static class IconManager
    {
        private static readonly Dictionary<string, Texture2D> _iconCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<Type, Texture2D> _typeCache = new Dictionary<Type, Texture2D>();
        private static readonly HashSet<string> _scannedPaths = new HashSet<string>();
        
        private static Texture2D _defaultIcon;
        private static Texture2D _missingIcon;
        private static string _iconFolderPath;
        
        private const string CUSTOM_FOLDER = "Icons/Custom";
        private const string DEFAULT_ICON_NAME = "Default";
        private const string MISSING_ICON_NAME = "Missing";
        
        static IconManager()
        {
            RefreshIcons();
        }
        
        /// <summary>
        /// Get icon for a specific component type
        /// </summary>
        public static Texture2D GetIcon<T>() where T : Component
        {
            return GetIcon(typeof(T));
        }
        
        /// <summary>
        /// Get icon for a specific type
        /// </summary>
        public static Texture2D GetIcon(Type type)
        {
            if (type == null) return _missingIcon ?? _defaultIcon;
            
            // Check type cache first
            if (_typeCache.TryGetValue(type, out var cachedIcon))
                return cachedIcon;
            
            // Try to find icon by type name - exact match first, then prefix matching
            var icon = GetIconWithPrefixMatching(type.Name);
            if (icon != null && icon != _defaultIcon)
            {
                _typeCache[type] = icon;
                return icon;
            }
            
            // Try enhanced third-party script icon detection
            var thirdPartyIcon = GetIconFromThirdPartyScript(type);
            if (thirdPartyIcon != null)
            {
                _typeCache[type] = thirdPartyIcon;
                return thirdPartyIcon;
            }
            
            // Try Unity's built-in ObjectContent icon (works for most Unity components)
            var content = EditorGUIUtility.ObjectContent(null, type);
            if (content != null && content.image != null)
            {
                var unityIcon = content.image as Texture2D;
                if (unityIcon != null)
                {
                    _typeCache[type] = unityIcon;
                    return unityIcon;
                }
            }
            
            // Fallback to Unity's type thumbnail
            var builtInIcon = AssetPreview.GetMiniTypeThumbnail(type) as Texture2D;
            if (builtInIcon != null)
            {
                _typeCache[type] = builtInIcon;
                return builtInIcon;
            }
            
            // Final fallback to default icon
            _typeCache[type] = _defaultIcon;
            return _defaultIcon;
        }
        
        /// <summary>
        /// Get icon by name
        /// </summary>
        public static Texture2D GetIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
                return _defaultIcon;
                
            if (_iconCache.TryGetValue(iconName, out var icon))
                return icon;
                
            return _defaultIcon;
        }
        
        /// <summary>
        /// Get icon for GameObject
        /// </summary>
        public static Texture2D GetGameObjectIcon(GameObject go)
        {
            if (go == null) return _defaultIcon;
            
            // Check for custom GameObject icon
            var customIcon = GetIcon("GameObject");
            if (customIcon != null && customIcon != _defaultIcon)
                return customIcon;
                
            // Use Unity's icon system
            var icon = EditorGUIUtility.GetIconForObject(go);
            if (icon != null) return icon as Texture2D;
            
            // Fallback to type icon
            return AssetPreview.GetMiniTypeThumbnail(typeof(GameObject)) as Texture2D ?? _defaultIcon;
        }
        
        /// <summary>
        /// Refresh all icons from disk
        /// </summary>
        public static void RefreshIcons()
        {
            _iconCache.Clear();
            _typeCache.Clear();
            _scannedPaths.Clear();
            
            // Find the icon folder dynamically
            FindIconFolder();
            
            if (!string.IsNullOrEmpty(_iconFolderPath))
            {
                // Load icons from all subdirectories
                LoadIconsFromPath(_iconFolderPath);
                
                if (ComfiSettings.Instance.debugMode)
                {
                    Debug.Log($"[ComfiHierarchy] Icon folder: {_iconFolderPath}");
                }
            }
            else
            {
                Debug.LogWarning("[ComfiHierarchy] Could not find Icons folder. Please ensure the Icons folder exists in the ComfiHierarchy directory.");
            }
            
            // Load special icons
            _defaultIcon = GetIcon(DEFAULT_ICON_NAME) ?? GenerateSolidColorTexture(Color.gray);
            _missingIcon = GetIcon(MISSING_ICON_NAME) ?? _defaultIcon;
            
            if (ComfiSettings.Instance.debugMode)
            {
                Debug.Log($"[ComfiHierarchy] Loaded {_iconCache.Count} icons from {_iconFolderPath}");
            }
            
            if (ComfiSettings.Instance.debugMode)
            {
                Debug.Log($"[ComfiHierarchy] Loaded icons: {string.Join(", ", _iconCache.Keys)}");
            }
        }
        
        /// <summary>
        /// Add custom icon mapping
        /// </summary>
        public static void AddCustomMapping(string typeName, string iconPath)
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (icon != null)
            {
                _iconCache[typeName] = icon;
            }
        }
        
        private static void FindIconFolder()
        {
            // Try multiple possible locations
            string[] possiblePaths = new[]
            {
                "Packages/cat.kittyn.comfi-hierarchy/Icons",
                "Packages/com.comfi.hierarchy/Icons",
                "Assets/cat.kittyn.comfi-hierarchy/Icons",
                "Assets/ComfiHierarchy/Icons",
                "Assets/Plugins/ComfiHierarchy/Icons",
                "Assets/Editor/ComfiHierarchy/Icons"
            };
            
            foreach (var path in possiblePaths)
            {
                if (AssetDatabase.IsValidFolder(path))
                {
                    _iconFolderPath = path;
                    return;
                }
            }
            
            // If not found in standard locations, search for it
            var iconFolders = AssetDatabase.FindAssets("t:Folder Icons");
            foreach (var guid in iconFolders)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("ComfiHierarchy") || path.Contains("comfi.hierarchy") || path.Contains("comfi-hierarchy"))
                {
                    _iconFolderPath = path;
                    return;
                }
            }
            
            // Last resort - search for a specific icon file
            var defaultIconGuids = AssetDatabase.FindAssets("Default t:Texture2D");
            foreach (var guid in defaultIconGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("ComfiHierarchy") || path.Contains("comfi.hierarchy") || path.Contains("comfi-hierarchy"))
                {
                    _iconFolderPath = Path.GetDirectoryName(Path.GetDirectoryName(path)); // Go up from Misc/Default.png to Icons/
                    if (_iconFolderPath != null)
                    {
                        _iconFolderPath = _iconFolderPath.Replace('\\', '/');
                        return;
                    }
                }
            }
        }
        
        private static void LoadIconsFromPath(string path)
        {
            if (_scannedPaths.Contains(path)) return;
            _scannedPaths.Add(path);
            
            if (!AssetDatabase.IsValidFolder(path)) return;
            
            if (ComfiSettings.Instance.debugMode)
            {
                Debug.Log($"[ComfiHierarchy] Scanning folder: {path}");
            }
            
            // Find all PNG files
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });
            
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                
                // Skip .meta files and non-PNG files
                if (assetPath.EndsWith(".meta") || !assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    continue;
                
                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                
                if (icon != null)
                {
                    // Use filename without extension as key
                    var iconName = Path.GetFileNameWithoutExtension(assetPath);
                    _iconCache[iconName] = icon;
                    
                    if (ComfiSettings.Instance.debugMode)
                    {
                        Debug.Log($"[ComfiHierarchy] Loaded icon: {iconName} from {assetPath}");
                    }
                }
            }
            
            // Recursively load from subdirectories
            var subdirs = AssetDatabase.GetSubFolders(path);
            foreach (var subdir in subdirs)
            {
                LoadIconsFromPath(subdir);
            }
        }
        
        private static Texture2D GenerateSolidColorTexture(Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Get all loaded icon names
        /// </summary>
        public static IEnumerable<string> GetLoadedIconNames()
        {
            return _iconCache.Keys.OrderBy(k => k);
        }
        
        /// <summary>
        /// Check if an icon exists for the given name
        /// </summary>
        public static bool HasIcon(string iconName)
        {
            return _iconCache.ContainsKey(iconName);
        }
        
        /// <summary>
        /// Test third-party icon detection for debugging
        /// </summary>
        [UnityEditor.MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🧪 Test Third-Party Icon Detection")]
        public static void TestThirdPartyIconDetection()
        {
            Debug.Log("[ComfiHierarchy] Testing third-party icon detection...");
            
            // Test with common third-party types if available
            var testTypes = new[]
            {
                "ModularAvatarBoneProxy",
                "VRCFuryComponent", 
                "AudioLinkController",
                "VRCAvatarDescriptor",
                "VRCPhysBone"
            };
            
            foreach (var typeName in testTypes)
            {
                var type = System.Type.GetType(typeName);
                if (type == null)
                {
                    // Try to find the type in all loaded assemblies
                    foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(typeName);
                        if (type != null) break;
                    }
                }
                
                if (type != null)
                {
                    var icon = GetIconFromThirdPartyScript(type);
                    var status = icon != null ? "✓ Found custom icon" : "✗ No custom icon";
                    Debug.Log($"[ComfiHierarchy] {typeName}: {status}");
                }
                else
                {
                    Debug.Log($"[ComfiHierarchy] {typeName}: Type not found (not installed)");
                }
            }
        }
        
        /// <summary>
        /// Enhanced third-party script icon detection
        /// </summary>
        private static Texture2D GetIconFromThirdPartyScript(Type type)
        {
            if (type == null || !ComfiSettings.Instance.enableThirdPartyIconDetection) return null;
            
            // Try MonoScript-based detection first
            if (ComfiSettings.Instance.enableMonoScriptIconDetection)
            {
                var monoScriptIcon = GetIconFromMonoScript(type);
                if (monoScriptIcon != null) return monoScriptIcon;
            }
            
            // Try AssetDatabase search with type matching
            if (ComfiSettings.Instance.enableAssetDatabaseIconSearch)
            {
                var assetDbIcon = GetIconFromAssetDatabase(type);
                if (assetDbIcon != null) return assetDbIcon;
            }
            
            // Try plugin-based script detection
            if (ComfiSettings.Instance.enablePluginIconDetection)
            {
                var pluginIcon = GetIconFromPlugin(type);
                if (pluginIcon != null) return pluginIcon;
            }
            
            return null;
        }
        
        /// <summary>
        /// Get icon from MonoScript using Unity's icon system
        /// </summary>
        private static Texture2D GetIconFromMonoScript(Type type)
        {
            try
            {
                // Find the MonoScript asset for this type
                MonoScript monoScript = null;
                
                // Try ScriptableObject approach first
                if (typeof(ScriptableObject).IsAssignableFrom(type))
                {
                    var instance = ScriptableObject.CreateInstance(type);
                    if (instance != null)
                    {
                        monoScript = MonoScript.FromScriptableObject(instance);
                        ScriptableObject.DestroyImmediate(instance);
                    }
                }
                
                // Try MonoBehaviour approach
                if (monoScript == null && typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    var instance = new GameObject().AddComponent(type) as MonoBehaviour;
                    if (instance != null)
                    {
                        monoScript = MonoScript.FromMonoBehaviour(instance);
                        UnityEngine.Object.DestroyImmediate(instance.gameObject);
                    }
                }
                
                // Fallback to searching all MonoScripts
                if (monoScript == null)
                {
                    var scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
                    monoScript = scripts.FirstOrDefault(s => s.GetClass() == type);
                }
                
                if (monoScript != null)
                {
                    // Try to get the icon directly from the MonoScript
                    var icon = EditorGUIUtility.GetIconForObject(monoScript) as Texture2D;
                    if (icon != null && icon.name != "DefaultAsset Icon")
                    {
                        return icon;
                    }
                    
                    // Try MonoImporter.GetIcon() for custom script icons
                    var assetPath = AssetDatabase.GetAssetPath(monoScript);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        var importer = AssetImporter.GetAtPath(assetPath) as MonoImporter;
                        if (importer != null)
                        {
                            var importerIcon = importer.GetIcon();
                            if (importerIcon != null)
                            {
                                return importerIcon as Texture2D;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors during MonoScript detection (common during compilation)
            }
            
            return null;
        }
        
        /// <summary>
        /// Enhanced AssetDatabase search with type matching
        /// </summary>
        private static Texture2D GetIconFromAssetDatabase(Type type)
        {
            try
            {
                // Search for script files matching the type name
                var scriptGuids = AssetDatabase.FindAssets($"{type.Name} t:MonoScript");
                
                foreach (var guid in scriptGuids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                    
                    if (script != null && script.GetClass() == type)
                    {
                        // Check if there's a custom icon associated with this script
                        var icon = EditorGUIUtility.GetIconForObject(script) as Texture2D;
                        if (icon != null && icon.name != "DefaultAsset Icon" && icon.name != "cs Script Icon")
                        {
                            return icon;
                        }
                        
                        // Look for icon files in the same directory
                        var directory = Path.GetDirectoryName(assetPath);
                        var iconPath = Path.Combine(directory, $"{type.Name}.png");
                        iconPath = iconPath.Replace('\\', '/');
                        
                        var iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                        if (iconTexture != null)
                        {
                            return iconTexture;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors during AssetDatabase search
            }
            
            return null;
        }
        
        /// <summary>
        /// Get icon from plugin-based scripts
        /// </summary>
        private static Texture2D GetIconFromPlugin(Type type)
        {
            try
            {
                // For plugin-based scripts, check if they have custom icons
                var assembly = type.Assembly;
                if (assembly != null)
                {
                    var assemblyName = assembly.GetName().Name;
                    
                    // Common plugin patterns that might have custom icons
                    if (assemblyName.Contains("VRC") || assemblyName.Contains("Modular") || 
                        assemblyName.Contains("Fury") || assemblyName.Contains("AudioLink"))
                    {
                        // Search for icon files matching common plugin icon patterns
                        var iconGuids = AssetDatabase.FindAssets($"{type.Name} t:Texture2D");
                        
                        foreach (var guid in iconGuids)
                        {
                            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                            if (assetPath.Contains("Icon") || assetPath.Contains("icon"))
                            {
                                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                                if (icon != null)
                                {
                                    return icon;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors during plugin detection
            }
            
            return null;
        }
        
        /// <summary>
        /// Get icon with automatic prefix matching
        /// </summary>
        private static Texture2D GetIconWithPrefixMatching(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
            
            // First try exact match
            if (_iconCache.TryGetValue(typeName, out var icon))
                return icon;
            
            // Progressive prefix matching - remove one word at a time from the end
            // For example: ModularAvatarBoneProxy -> ModularAvatarBone -> ModularAvatar -> Modular
            var currentName = typeName;
            
            while (currentName.Length > 0)
            {
                // Try current prefix
                if (_iconCache.TryGetValue(currentName, out icon))
                    return icon;
                
                // Find the last uppercase letter (except the first character)
                int lastUpperIndex = -1;
                for (int i = currentName.Length - 1; i > 0; i--)
                {
                    if (char.IsUpper(currentName[i]))
                    {
                        lastUpperIndex = i;
                        break;
                    }
                }
                
                // If we found an uppercase letter, truncate there
                if (lastUpperIndex > 0)
                {
                    currentName = currentName.Substring(0, lastUpperIndex);
                }
                else
                {
                    // No more uppercase letters found, we're done
                    break;
                }
            }
            
            return null;
        }
    }
}