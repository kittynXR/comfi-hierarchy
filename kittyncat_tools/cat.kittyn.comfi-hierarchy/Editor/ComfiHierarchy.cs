using UnityEditor;
using UnityEngine;
using Kittyn.Tools;

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
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🐟 ComfiHierarchy Settings", false, 2000)]
        public static void ShowWindowMenu()
        {
            ShowWindow();
        }
        
        public static void ShowWindow()
        {
            var window = GetWindow<ComfiHierarchy>(KittynLocalization.Get("comfi_hierarchy.window_title"));
            window.minSize = new Vector2(350, 400);
        }
        
        // Legacy menu item for compatibility
        private static void ShowWindowLegacy()
        {
            ShowWindow();
        }
        
        private void OnEnable()
        {
            _settings = ComfiSettings.Instance;
        }
        
        private void OnGUI()
        {
            if (_settings == null)
            {
                EditorGUILayout.HelpBox(KittynLocalization.Get("comfi_hierarchy.settings_not_found"), MessageType.Warning);
                _settings = ComfiSettings.Instance;
                return;
            }
            
            EditorGUI.BeginChangeCheck();
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            
            // Header
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(KittynLocalization.Get("comfi_hierarchy.settings_title"), EditorStyles.boldLabel);
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
            EditorGUILayout.LabelField(KittynLocalization.Get("common.general"), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                _settings.enabled = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.enable_comfi"), _settings.enabled);
                
                using (new EditorGUI.DisabledScope(!_settings.enabled))
                {
                    _settings.enableComponentToggle = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.component_toggle"), _settings.enableComponentToggle);
                    _settings.enableDragToggle = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.drag_toggle"), _settings.enableDragToggle);
                    _settings.enableContextMenus = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.context_menus"), _settings.enableContextMenus);
                }
            }
        }
        
        private void DrawIconSettings()
        {
            EditorGUILayout.LabelField(KittynLocalization.Get("comfi_hierarchy.icons"), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(!_settings.enabled))
            {
                _settings.showIcons = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.show_icons"), _settings.showIcons);
                
                using (new EditorGUI.DisabledScope(!_settings.showIcons))
                {
                    _settings.showGameObjectIcon = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.gameobject_icon"), _settings.showGameObjectIcon);
                    _settings.showTransformIcon = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.transform_icon"), _settings.showTransformIcon);
                    _settings.showNonToggleableIcons = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.non_toggleable_icons"), _settings.showNonToggleableIcons);
                    _settings.enableIconTooltips = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.icon_tooltips"), _settings.enableIconTooltips);
                    _settings.linkCursorOnHover = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.link_cursor"), _settings.linkCursorOnHover);
                    _settings.iconXOffset = EditorGUILayout.Slider(KittynLocalization.Get("comfi_hierarchy.icon_x_offset"), _settings.iconXOffset, -50f, 50f);
                }
            }
        }
        
        private void DrawVisualFeatures()
        {
            EditorGUILayout.LabelField(KittynLocalization.Get("comfi_hierarchy.visual_features"), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(!_settings.enabled))
            {
                _settings.showTreeLines = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.tree_guide_lines"), _settings.showTreeLines);
                _settings.enableRowColoring = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.row_coloring"), _settings.enableRowColoring);
                _settings.showLayerLabel = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.layer_labels"), _settings.showLayerLabel);
                _settings.showTagLabel = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.tag_labels"), _settings.showTagLabel);
                
                if (_settings.showLayerLabel)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        _settings.layerLabelWidth = EditorGUILayout.Slider(KittynLocalization.Get("comfi_hierarchy.layer_label_width"), _settings.layerLabelWidth, 50f, 150f);
                        _settings.showDefaultLayer = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.show_default_layer"), _settings.showDefaultLayer);
                    }
                }
                
                if (_settings.showTagLabel)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        _settings.tagLabelWidth = EditorGUILayout.Slider(KittynLocalization.Get("comfi_hierarchy.tag_label_width"), _settings.tagLabelWidth, 50f, 150f);
                        _settings.showUntagged = EditorGUILayout.Toggle(KittynLocalization.Get("comfi_hierarchy.show_untagged"), _settings.showUntagged);
                    }
                }
            }
        }
        
        private void DrawColorSettings()
        {
            EditorGUILayout.LabelField(KittynLocalization.Get("common.colors"), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(!_settings.enabled))
            {
                _settings.iconTintActive = EditorGUILayout.ColorField(KittynLocalization.Get("comfi_hierarchy.active_icon_tint"), _settings.iconTintActive);
                _settings.iconTintInactive = EditorGUILayout.ColorField(KittynLocalization.Get("comfi_hierarchy.inactive_icon_tint"), _settings.iconTintInactive);
                _settings.iconBackgroundColor = EditorGUILayout.ColorField(KittynLocalization.Get("comfi_hierarchy.icon_background"), _settings.iconBackgroundColor);
                
                if (_settings.showTreeLines)
                    _settings.treeLineColor = EditorGUILayout.ColorField(KittynLocalization.Get("comfi_hierarchy.tree_line_color"), _settings.treeLineColor);
                
                if (_settings.enableRowColoring)
                {
                    _settings.rowColorOdd = EditorGUILayout.ColorField(KittynLocalization.Get("comfi_hierarchy.row_color_odd"), _settings.rowColorOdd);
                    _settings.rowColorEven = EditorGUILayout.ColorField(KittynLocalization.Get("comfi_hierarchy.row_color_even"), _settings.rowColorEven);
                }
            }
        }
        
        private void DrawAdvancedSettings()
        {
            EditorGUILayout.LabelField(KittynLocalization.Get("common.advanced"), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                // Hidden component types
                EditorGUILayout.LabelField(KittynLocalization.Get("comfi_hierarchy.hidden_component_types"), EditorStyles.miniBoldLabel);
                for (int i = 0; i < _settings.hiddenComponentTypes.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        _settings.hiddenComponentTypes[i] = EditorGUILayout.TextField(_settings.hiddenComponentTypes[i]);
                        if (GUILayout.Button(KittynLocalization.Get("common.delete"), GUILayout.Width(20)))
                        {
                            _settings.hiddenComponentTypes.RemoveAt(i);
                            i--;
                        }
                    }
                }
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(KittynLocalization.Get("comfi_hierarchy.add_hidden_type")))
                    {
                        _settings.hiddenComponentTypes.Add("");
                    }
                }
                
                EditorGUILayout.Space();
                _settings.debugMode = EditorGUILayout.Toggle(KittynLocalization.Get("common.debug_mode"), _settings.debugMode);
            }
        }
        
        private void DrawFooter()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(KittynLocalization.Get("common.refresh")))
                {
                    IconManager.RefreshIcons();
                    EditorApplication.RepaintHierarchyWindow();
                }
                
                if (GUILayout.Button(KittynLocalization.Get("common.reset")))
                {
                    if (EditorUtility.DisplayDialog(KittynLocalization.Get("common.reset"), 
                        KittynLocalization.Get("messages.confirm_reset"), 
                        KittynLocalization.Get("common.reset"), KittynLocalization.Get("common.cancel")))
                    {
                        ResetSettings();
                    }
                }
                
                if (GUILayout.Button(KittynLocalization.Get("common.preview")))
                {
                    _showIconPreview = !_showIconPreview;
                }
            }
            
            // Language selector
            EditorGUILayout.Space();
            KittynLanguageSelector.DrawLanguageSelector();
            
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