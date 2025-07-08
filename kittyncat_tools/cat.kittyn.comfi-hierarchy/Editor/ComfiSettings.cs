using System;
using System.Collections.Generic;
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
        public bool enableMonoScriptIconDetection = true;
        
        [Tooltip("Search for icon files in script directories")]
        public bool enableAssetDatabaseIconSearch = true;
        
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
            return hiddenComponentTypes.Contains(componentType.Name);
        }
        
        /// <summary>
        /// Save any changes to the settings
        /// </summary>
        public void SaveChanges()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}