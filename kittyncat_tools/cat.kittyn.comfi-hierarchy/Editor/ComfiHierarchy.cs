using UnityEditor;
using UnityEngine;

namespace Comfi.Hierarchy
{
    /// <summary>
    /// Main settings window for ComfiHierarchy
    /// </summary>
    public class ComfiHierarchy : EditorWindow
    {
        private ComfiSettings _settings;
        private Vector2 _scrollPos;
        private bool _showIconPreview;
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/ComfiHierarchy/Settings", false, 2000)]
        public static void ShowWindow()
        {
            var window = GetWindow<ComfiHierarchy>("ComfiHierarchy");
            window.minSize = new Vector2(350, 400);
        }
        
        private void OnEnable()
        {
            _settings = ComfiSettings.Instance;
        }
        
        private void OnGUI()
        {
            if (_settings == null)
            {
                EditorGUILayout.HelpBox("Settings not found. Creating new settings...", MessageType.Warning);
                _settings = ComfiSettings.Instance;
                return;
            }
            
            EditorGUI.BeginChangeCheck();
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            
            // Header
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ComfiHierarchy Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // General Settings
            DrawGeneralSettings();
            EditorGUILayout.Space();
            
            // Icon Settings
            DrawIconSettings();
            EditorGUILayout.Space();
            
            // Visual Features
            DrawVisualFeatures();
            EditorGUILayout.Space();
            
            // Color Settings
            DrawColorSettings();
            EditorGUILayout.Space();
            
            // Advanced Settings
            DrawAdvancedSettings();
            
            EditorGUILayout.EndScrollView();
            
            // Footer
            EditorGUILayout.Space();
            DrawFooter();
            
            if (EditorGUI.EndChangeCheck())
            {
                _settings.SaveChanges();
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        
        private void DrawGeneralSettings()
        {
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                _settings.enabled = EditorGUILayout.Toggle("Enable ComfiHierarchy", _settings.enabled);
                
                using (new EditorGUI.DisabledScope(!_settings.enabled))
                {
                    _settings.enableComponentToggle = EditorGUILayout.Toggle("Component Toggle", _settings.enableComponentToggle);
                    _settings.enableDragToggle = EditorGUILayout.Toggle("Drag Toggle", _settings.enableDragToggle);
                    _settings.enableContextMenus = EditorGUILayout.Toggle("Context Menus", _settings.enableContextMenus);
                }
            }
        }
        
        private void DrawIconSettings()
        {
            EditorGUILayout.LabelField("Icons", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(!_settings.enabled))
            {
                _settings.showIcons = EditorGUILayout.Toggle("Show Icons", _settings.showIcons);
                
                using (new EditorGUI.DisabledScope(!_settings.showIcons))
                {
                    _settings.showGameObjectIcon = EditorGUILayout.Toggle("GameObject Icon", _settings.showGameObjectIcon);
                    _settings.showTransformIcon = EditorGUILayout.Toggle("Transform Icon", _settings.showTransformIcon);
                    _settings.showNonToggleableIcons = EditorGUILayout.Toggle("Non-Toggleable Icons", _settings.showNonToggleableIcons);
                    _settings.enableIconTooltips = EditorGUILayout.Toggle("Icon Tooltips", _settings.enableIconTooltips);
                    _settings.linkCursorOnHover = EditorGUILayout.Toggle("Link Cursor", _settings.linkCursorOnHover);
                    _settings.iconXOffset = EditorGUILayout.Slider("Icon X Offset", _settings.iconXOffset, -50f, 50f);
                }
            }
        }
        
        private void DrawVisualFeatures()
        {
            EditorGUILayout.LabelField("Visual Features", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(!_settings.enabled))
            {
                _settings.showTreeLines = EditorGUILayout.Toggle("Tree Guide Lines", _settings.showTreeLines);
                _settings.enableRowColoring = EditorGUILayout.Toggle("Row Coloring", _settings.enableRowColoring);
                _settings.showLayerLabel = EditorGUILayout.Toggle("Layer Labels", _settings.showLayerLabel);
                _settings.showTagLabel = EditorGUILayout.Toggle("Tag Labels", _settings.showTagLabel);
                
                if (_settings.showLayerLabel)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        _settings.layerLabelWidth = EditorGUILayout.Slider("Layer Label Width", _settings.layerLabelWidth, 50f, 150f);
                        _settings.showDefaultLayer = EditorGUILayout.Toggle("Show Default Layer", _settings.showDefaultLayer);
                    }
                }
                
                if (_settings.showTagLabel)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        _settings.tagLabelWidth = EditorGUILayout.Slider("Tag Label Width", _settings.tagLabelWidth, 50f, 150f);
                        _settings.showUntagged = EditorGUILayout.Toggle("Show Untagged", _settings.showUntagged);
                    }
                }
            }
        }
        
        private void DrawColorSettings()
        {
            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(!_settings.enabled))
            {
                _settings.iconTintActive = EditorGUILayout.ColorField("Active Icon Tint", _settings.iconTintActive);
                _settings.iconTintInactive = EditorGUILayout.ColorField("Inactive Icon Tint", _settings.iconTintInactive);
                _settings.iconBackgroundColor = EditorGUILayout.ColorField("Icon Background", _settings.iconBackgroundColor);
                
                if (_settings.showTreeLines)
                    _settings.treeLineColor = EditorGUILayout.ColorField("Tree Line Color", _settings.treeLineColor);
                
                if (_settings.enableRowColoring)
                {
                    _settings.rowColorOdd = EditorGUILayout.ColorField("Row Color Odd", _settings.rowColorOdd);
                    _settings.rowColorEven = EditorGUILayout.ColorField("Row Color Even", _settings.rowColorEven);
                }
            }
        }
        
        private void DrawAdvancedSettings()
        {
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                // Hidden component types
                EditorGUILayout.LabelField("Hidden Component Types", EditorStyles.miniBoldLabel);
                for (int i = 0; i < _settings.hiddenComponentTypes.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        _settings.hiddenComponentTypes[i] = EditorGUILayout.TextField(_settings.hiddenComponentTypes[i]);
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            _settings.hiddenComponentTypes.RemoveAt(i);
                            i--;
                        }
                    }
                }
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add Hidden Type"))
                    {
                        _settings.hiddenComponentTypes.Add("");
                    }
                }
                
                EditorGUILayout.Space();
                _settings.debugMode = EditorGUILayout.Toggle("Debug Mode", _settings.debugMode);
            }
        }
        
        private void DrawFooter()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh Icons"))
                {
                    IconManager.RefreshIcons();
                    EditorApplication.RepaintHierarchyWindow();
                }
                
                if (GUILayout.Button("Reset Settings"))
                {
                    if (EditorUtility.DisplayDialog("Reset Settings", 
                        "Are you sure you want to reset all settings to default?", 
                        "Reset", "Cancel"))
                    {
                        ResetSettings();
                    }
                }
                
                if (GUILayout.Button("Icon Preview"))
                {
                    _showIconPreview = !_showIconPreview;
                }
            }
            
            if (_showIconPreview)
            {
                DrawIconPreview();
            }
        }
        
        private void DrawIconPreview()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Loaded Icons", EditorStyles.boldLabel);
            
            var iconNames = IconManager.GetLoadedIconNames();
            int columns = Mathf.Max(1, (int)(position.width / 100));
            int current = 0;
            
            EditorGUILayout.BeginHorizontal();
            foreach (var iconName in iconNames)
            {
                var icon = IconManager.GetIcon(iconName);
                if (icon != null)
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(90)))
                    {
                        GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));
                        EditorGUILayout.LabelField(iconName, EditorStyles.miniLabel);
                    }
                    
                    current++;
                    if (current >= columns)
                    {
                        current = 0;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void ResetSettings()
        {
            _settings.enabled = true;
            _settings.showIcons = true;
            _settings.showGameObjectIcon = true;
            _settings.showTransformIcon = false;
            _settings.showNonToggleableIcons = true;
            _settings.enableIconTooltips = true;
            _settings.linkCursorOnHover = true;
            _settings.iconXOffset = 0f;
            _settings.enableComponentToggle = true;
            _settings.enableDragToggle = true;
            _settings.enableContextMenus = true;
            _settings.showTreeLines = false;
            _settings.enableRowColoring = true;
            _settings.showLayerLabel = true;
            _settings.showTagLabel = true;
            _settings.treeLineColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            _settings.iconTintActive = Color.white;
            _settings.iconTintInactive = new Color(1f, 1f, 1f, 0.5f);
            _settings.rowColorOdd = new Color(0f, 0f, 0f, 0.05f);
            _settings.rowColorEven = new Color(0f, 0f, 0f, 0.1f);
            _settings.iconBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            _settings.layerLabelWidth = 75f;
            _settings.tagLabelWidth = 75f;
            _settings.showDefaultLayer = false;
            _settings.showUntagged = false;
            _settings.hiddenComponentTypes.Clear();
            _settings.hiddenComponentTypes.Add("MeshFilter");
            _settings.debugMode = false;
            _settings.SaveChanges();
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}