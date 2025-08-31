using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Comfi.Hierarchy
{
    /// <summary>
    /// Settings for ComfiHierarchy stored as a ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ComfiHierarchySettings", menuName = "ComfiHierarchy/Settings")]
    public class ComfiSettings : ScriptableObject
    {
        private static ComfiSettings _instance;
        [NonSerialized] private HashSet<string> _hiddenComponentTypesSet;
        [NonSerialized] private HashSet<string> _hiddenComponentTypesShortSet;

        private const int LatestVersion = 1;
        [SerializeField] private int settingsVersion = 0;
        
        public static ComfiSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadOrCreateSettings();
                }
                return _instance;
            }
        }
        
        [Header("General")]
        public bool enabled = true;
        
        [Header("Icons")]
        public bool showIcons = true;
        public bool showGameObjectIcon = true;
        public bool showTransformIcon = false;
        public bool showNonToggleableIcons = true;
        public bool enableIconTooltips = true;
        public bool linkCursorOnHover = true;
        public float iconXOffset = 0f;
        
        [Header("Component Interaction")]
        public bool enableComponentToggle = true;
        public bool enableDragToggle = true;
        public bool enableContextMenus = true;
        
        [Header("Visual Features")]
        public bool showTreeLines = true;
        public bool enableRowColoring = true;
        public bool showLayerLabel = true;
        public bool showTagLabel = true;
        
        [Header("Colors")]
        public Color treeLineColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public Color iconTintActive = Color.white;
        public Color iconTintInactive = new Color(1f, 1f, 1f, 0.5f);
        public Color rowColorOdd = new Color(0f, 0f, 0f, 0.05f);
        public Color rowColorEven = new Color(0f, 0f, 0f, 0.1f);
        public Color iconBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        [Header("Labels")]
        public float layerLabelWidth = 75f;
        public float tagLabelWidth = 75f;
        public bool showDefaultLayer = false;
        public bool showUntagged = false;
        
        [Header("Advanced")]
        public List<string> hiddenComponentTypes = new List<string> { "MeshFilter" };
        public bool debugMode = false;
        
        [Header("Third-Party Script Icons")]
        [Tooltip("Enable enhanced detection of custom icons from third-party scripts")]
        public bool enableThirdPartyIconDetection = true;
        
        [Tooltip("Enable MonoScript-based icon detection (may impact performance)")]
        public bool enableMonoScriptIconDetection = false;
        
        [Tooltip("Search for icon files in script directories")]
        public bool enableAssetDatabaseIconSearch = false;
        
        [Tooltip("Enable plugin-based icon detection for VRC/community tools")]
        public bool enablePluginIconDetection = true;
        
        private static void LoadOrCreateSettings()
        {
            // Try to load from Resources first
            _instance = Resources.Load<ComfiSettings>("ComfiHierarchySettings");
            
            if (_instance == null)
            {
                // Try to find in project
                var guids = AssetDatabase.FindAssets("t:ComfiSettings");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _instance = AssetDatabase.LoadAssetAtPath<ComfiSettings>(path);
                }
            }
            
            if (_instance == null)
            {
                // Create new settings
                _instance = CreateInstance<ComfiSettings>();
                
                // Ensure Resources folder exists
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                
                // Save to Resources
                AssetDatabase.CreateAsset(_instance, "Assets/Resources/ComfiHierarchySettings.asset");
                AssetDatabase.SaveAssets();
                
                Debug.Log("[ComfiHierarchy] Created new settings file at Assets/Resources/ComfiHierarchySettings.asset");
            }

            // Run migrations and ensure latest defaults
            _instance.RunMigrationsIfNeeded();
        }
        
        public static void ReloadSettings()
        {
            _instance = null;
            LoadOrCreateSettings();
        }
        
        /// <summary>
        /// Check if a component type should be hidden
        /// </summary>
        public bool IsComponentHidden(Type componentType)
        {
            if (componentType == null) return false;
            EnsureHiddenSets();
            // Prefer fully-qualified name match; fall back to short name for backward compatibility
            return _hiddenComponentTypesSet.Contains(componentType.FullName) || _hiddenComponentTypesShortSet.Contains(componentType.Name);
        }
        
        /// <summary>
        /// Save any changes to the settings
        /// </summary>
        public void SaveChanges()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            // Rebuild cache if list changed
            _hiddenComponentTypesSet = null;
            _hiddenComponentTypesShortSet = null;
        }

        private void OnValidate()
        {
            // Rebuild cache when values change in the inspector
            _hiddenComponentTypesSet = null;
            _hiddenComponentTypesShortSet = null;
        }

        private void EnsureHiddenSets()
        {
            if (_hiddenComponentTypesSet == null || _hiddenComponentTypesShortSet == null || _hiddenComponentTypesSet.Count != hiddenComponentTypes.Count)
            {
                _hiddenComponentTypesSet = new HashSet<string>(hiddenComponentTypes);
                _hiddenComponentTypesShortSet = new HashSet<string>();
                foreach (var s in hiddenComponentTypes)
                {
                    if (string.IsNullOrEmpty(s)) continue;
                    var idx = s.LastIndexOf('.');
                    var shortName = idx >= 0 && idx < s.Length - 1 ? s.Substring(idx + 1) : s;
                    _hiddenComponentTypesShortSet.Add(shortName);
                }
            }
        }

        private void RunMigrationsIfNeeded()
        {
            if (settingsVersion >= LatestVersion) return;

            bool changed = false;

            // v1: Migrate hidden types to fully qualified names where possible
            changed |= MigrateHiddenTypesToFqn();

            // Do not override user icon-detection preferences on migration.
            // Defaults for new settings are applied via field initializers above.

            settingsVersion = LatestVersion;
            if (changed)
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        private bool MigrateHiddenTypesToFqn()
        {
            bool changed = false;
            for (int i = 0; i < hiddenComponentTypes.Count; i++)
            {
                var name = hiddenComponentTypes[i];
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (name.Contains(".")) continue; // assume already FQN

                string fqn = TryResolveFullName(name);
                if (!string.IsNullOrEmpty(fqn))
                {
                    hiddenComponentTypes[i] = fqn;
                    changed = true;
                }
            }
            return changed;
        }

        private static string TryResolveFullName(string shortName)
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                Type resolved = null;
                foreach (var asm in assemblies)
                {
                    Type match = null;
                    try
                    {
                        match = asm.GetTypes().FirstOrDefault(t => t.Name == shortName);
                    }
                    catch { /* ignore reflection type load issues */ }
                    if (match == null) continue;
                    if (resolved == null)
                    {
                        resolved = match;
                    }
                    else if (resolved != match)
                    {
                        // ambiguous, abort migration for this entry
                        return null;
                    }
                }
                return resolved?.FullName;
            }
            catch
            {
                return null;
            }
        }
    }
}
