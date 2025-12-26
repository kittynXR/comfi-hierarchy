using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Comfi.Hierarchy
{
    /// <summary>
    /// Handles drawing in the Unity hierarchy window
    /// </summary>
    [InitializeOnLoad]
    public static class ComfiHierarchyDrawer
    {
        private static ComfiSettings Settings => ComfiSettings.Instance;
        
        private static readonly HashSet<UnityEngine.Object> _dragToggledObjects = new HashSet<UnityEngine.Object>();
        private static bool _dragToggleState;
        private static bool _dragToggleGameObjectOnly;
        private static readonly int DragToggleControlId = "ComfiHierarchyDragToggle".GetHashCode();
        private static readonly List<bool> s_HasNextSiblingTemp = new List<bool>(16);
        private static readonly List<Component> s_ComponentBuffer = new List<Component>(16);
        private static readonly GUIContent s_TempContent = new GUIContent();
        private static readonly GUIContent s_TooltipContent = new GUIContent(string.Empty, string.Empty);
        private static GUIStyle s_LabelStyle;
        private static MethodInfo s_DisplayObjectContextMenu;
        private static readonly UnityEngine.Object[] s_ContextMenuSingle = new UnityEngine.Object[1];
        private static readonly string[] s_LayerNames = new string[32];
        
        static ComfiHierarchyDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            // Cache non-public context menu method
            s_DisplayObjectContextMenu = typeof(EditorUtility).GetMethod(
                "DisplayObjectContextMenu",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(Rect), typeof(UnityEngine.Object[]), typeof(int) },
                null);
        }
        
        private static void OnHierarchyGUI(int instanceId, Rect rect)
        {
            if (!Settings.enabled) return;

            // Ensure any drag toggle control is properly released on mouse up
            if (Event.current.rawType == EventType.MouseUp && GUIUtility.hotControl == DragToggleControlId)
            {
                GUIUtility.hotControl = 0;
                _dragToggledObjects.Clear();
                _dragToggleGameObjectOnly = false;
            }
            
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null) return;
            
            // Draw row coloring first (background)
            if (Settings.enableRowColoring)
            {
                DrawRowColoring(rect);
            }
            
            // Draw tree lines
            if (Settings.showTreeLines)
            {
                DrawTreeLines(go, rect);
            }
            
            // Calculate base positioning
            s_TempContent.text = go.name;
            float nameWidth = GUI.skin.label.CalcSize(s_TempContent).x + 18;
            float currentX = rect.x + nameWidth;
            
            // Draw labels on the left side (after name)
            if (Settings.showTagLabel || Settings.showLayerLabel)
            {
                currentX = DrawLabels(go, rect, currentX);
            }
            
            // Calculate remaining space for icons
            Rect iconArea = new Rect(rect);
            iconArea.x = currentX + Settings.iconXOffset;
            iconArea.width = rect.xMax - iconArea.x - 16; // Leave space for visibility toggle
            
            // Draw component icons
            if (Settings.showIcons)
            {
                DrawComponentIcons(go, iconArea);
            }
        }
        
        private static void DrawRowColoring(Rect rect)
        {
            if (Event.current.type != EventType.Repaint) return;
            
            float viewWidth = EditorGUIUtility.currentViewWidth;
            Rect fullRect = new Rect(0, rect.y, viewWidth, rect.height);
            int rowIndex = Mathf.FloorToInt(rect.y / rect.height);
            Color color = (rowIndex & 1) == 0 ? Settings.rowColorEven : Settings.rowColorOdd;
            EditorGUI.DrawRect(fullRect, color);
        }
        
        private static void DrawTreeLines(GameObject go, Rect rect)
        {
            if (Event.current.type != EventType.Repaint) return;
            
            Transform transform = go.transform;
            Transform parent = transform.parent;
            
            if (parent == null) return;
            
            // Calculate depth and hierarchy info
            s_HasNextSiblingTemp.Clear();
            Transform current = transform;
            
            while (parent != null)
            {
                s_HasNextSiblingTemp.Insert(0, current.GetSiblingIndex() < parent.childCount - 1);
                current = parent;
                parent = parent.parent;
            }
            
            // Draw lines
            Handles.color = Settings.treeLineColor;
            float indent = 14f;
            // Calculate proper base position based on hierarchy depth
            float baseX = rect.x - (s_HasNextSiblingTemp.Count * indent) + indent - 7f;
            
            // Vertical lines for ancestors
            for (int i = 0; i < s_HasNextSiblingTemp.Count - 1; i++)
            {
                if (s_HasNextSiblingTemp[i])
                {
                    float x = baseX + (i * indent);
                    Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.y + rect.height));
                }
            }
            
            // Lines for current item
            bool isLastChild = transform.GetSiblingIndex() == transform.parent.childCount - 1;
            float currentX = baseX + ((s_HasNextSiblingTemp.Count - 1) * indent);
            float centerY = rect.y + rect.height / 2f;
            
            // Horizontal line
            Handles.DrawLine(new Vector3(currentX, centerY), new Vector3(currentX + 8, centerY));
            
            // Vertical line
            if (!isLastChild)
            {
                Handles.DrawLine(new Vector3(currentX, rect.y), new Vector3(currentX, rect.y + rect.height));
            }
            else
            {
                Handles.DrawLine(new Vector3(currentX, rect.y), new Vector3(currentX, centerY));
            }
        }
        
        private static void DrawComponentIcons(GameObject go, Rect iconArea)
        {
            const float iconSize = 16f;
            const float iconSpacing = 2f;
            float currentX = iconArea.xMax - iconSize;
            
            // Draw GameObject icon
            if (Settings.showGameObjectIcon)
            {
                Rect iconRect = new Rect(currentX, iconArea.y, iconSize, iconSize);
                DrawIcon(iconRect, go, IconManager.GetGameObjectIcon(go), true);
                currentX -= iconSize + iconSpacing;
            }
            
            // Draw component icons
            s_ComponentBuffer.Clear();
            go.GetComponents(s_ComponentBuffer);
            bool firstComponent = true;
            
            foreach (var component in s_ComponentBuffer)
            {
                if (component == null)
                {
                    // Missing script
                    Rect iconRect = new Rect(currentX, iconArea.y, iconSize, iconSize);
                    DrawIcon(iconRect, null, IconManager.GetIcon("Missing"), false);
                    currentX -= iconSize + iconSpacing;
                    continue;
                }
                
                // Skip transform if not showing
                if (firstComponent)
                {
                    firstComponent = false;
                    if (!Settings.showTransformIcon) continue;
                }
                
                // Check if component type is hidden
                if (Settings.IsComponentHidden(component.GetType())) continue;
                
                // Skip non-toggleable if not showing
                if (!Settings.showNonToggleableIcons && !IsToggleable(component)) continue;
                
                // Draw the icon
                Rect iconRect2 = new Rect(currentX, iconArea.y, iconSize, iconSize);
                bool toggleable = IsToggleable(component);
                var icon = IconManager.GetIcon(component.GetType());
                DrawIcon(iconRect2, component, icon, toggleable);
                currentX -= iconSize + iconSpacing;
                
                // Stop if we run out of space
                if (currentX < iconArea.x) break;
            }
            s_ComponentBuffer.Clear();
        }
        
        private static void DrawIcon(Rect rect, UnityEngine.Object target, Texture2D icon, bool isToggleable)
        {
            if (icon == null) return;
            
            bool isEnabled = true;
            
            if (target is GameObject go)
            {
                isEnabled = go.activeSelf;
            }
            else if (target is Behaviour behaviour)
            {
                isEnabled = behaviour.enabled;
            }
            else if (target is Renderer renderer)
            {
                isEnabled = renderer.enabled;
            }
            else if (target is Collider collider)
            {
                isEnabled = collider.enabled;
            }
            
            // Handle interaction
            if (isToggleable && Settings.enableComponentToggle && target != null)
            {
                HandleIconInteraction(rect, target, ref isEnabled);
            }
            
            // Draw icon with tint
            Color tint = isEnabled ? Settings.iconTintActive : Settings.iconTintInactive;
            var prevColor = GUI.color;
            GUI.color = tint;
            
            // Background
            if (Settings.iconBackgroundColor.a > 0)
            {
                EditorGUI.DrawRect(rect, Settings.iconBackgroundColor);
            }
            
            // Icon
            GUI.DrawTexture(rect, icon);
            GUI.color = prevColor;
            
            // Tooltip
            if (Settings.enableIconTooltips && target != null)
            {
                s_TooltipContent.tooltip = target.GetType().Name;
                GUI.Label(rect, s_TooltipContent);
            }
            
            // Cursor
            if (Settings.linkCursorOnHover && rect.Contains(Event.current.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            }
        }
        
        private static void HandleIconInteraction(Rect rect, UnityEngine.Object target, ref bool isEnabled)
        {
            Event e = Event.current;

            // Release drag toggle on mouse up anywhere
            if (e.type == EventType.MouseUp && GUIUtility.hotControl == DragToggleControlId)
            {
                GUIUtility.hotControl = 0;
                _dragToggledObjects.Clear();
                _dragToggleGameObjectOnly = false;
            }
            
            // Left click toggle
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                e.Use();
                _dragToggleState = !isEnabled;
                _dragToggledObjects.Clear();
                _dragToggleGameObjectOnly = target is GameObject;
                
                if (Settings.enableDragToggle)
                {
                    GUIUtility.hotControl = DragToggleControlId;
                }
                
                ToggleComponent(target, _dragToggleState);
                _dragToggledObjects.Add(target);
            }
            
            // Drag toggle
            if (Settings.enableDragToggle && 
                GUIUtility.hotControl == DragToggleControlId && 
                rect.Contains(e.mousePosition) &&
                !_dragToggledObjects.Contains(target))
            {
                if (_dragToggleGameObjectOnly && !(target is GameObject))
                {
                    return;
                }
                ToggleComponent(target, _dragToggleState);
                _dragToggledObjects.Add(target);
            }
            
            // Context menu
            if (Settings.enableContextMenus && e.type == EventType.MouseDown && e.button == 1 && rect.Contains(e.mousePosition))
            {
                e.Use();
                ShowComponentContextMenu(target);
            }
        }
        
        private static void ToggleComponent(UnityEngine.Object target, bool state)
        {
            Undo.RecordObject(target, "Toggle Component");
            
            if (target is GameObject go)
            {
                go.SetActive(state);
            }
            else if (target is Behaviour behaviour)
            {
                behaviour.enabled = state;
            }
            else if (target is Renderer renderer)
            {
                renderer.enabled = state;
            }
            else if (target is Collider collider)
            {
                collider.enabled = state;
            }
            
            EditorUtility.SetDirty(target);
        }
        
        private static void ShowComponentContextMenu(UnityEngine.Object target)
        {
            if (s_DisplayObjectContextMenu != null)
            {
                s_ContextMenuSingle[0] = target;
                s_DisplayObjectContextMenu.Invoke(null, new object[] { new Rect(Event.current.mousePosition, Vector2.zero), s_ContextMenuSingle, 0 });
            }
        }
        
        private static float DrawLabels(GameObject go, Rect originalRect, float startX)
        {
            float currentX = startX;
            
            // Tag label (leftmost)
            if (Settings.showTagLabel && (Settings.showUntagged || !go.CompareTag("Untagged")))
            {
                Rect labelRect = new Rect(currentX, originalRect.y, Settings.tagLabelWidth, originalRect.height);
                DrawLabel(labelRect, go.tag);
                currentX += Settings.tagLabelWidth + 4;
            }
            
            // Layer label (after tag)
            if (Settings.showLayerLabel && (Settings.showDefaultLayer || go.layer != 0))
            {
                string layerName = GetCachedLayerName(go.layer);
                Rect labelRect = new Rect(currentX, originalRect.y, Settings.layerLabelWidth, originalRect.height);
                DrawLabel(labelRect, layerName);
                currentX += Settings.layerLabelWidth + 4;
            }
            
            return currentX;
        }
        
        private static void DrawLabel(Rect rect, string text)
        {
            if (s_LabelStyle == null)
            {
                s_LabelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 9,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(4, 4, 0, 0)
                };
                s_LabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            }
            
            // Optional: Add a subtle background
            var bgColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            EditorGUI.DrawRect(rect, bgColor);
            
            GUI.Label(rect, text, s_LabelStyle);
        }
        
        private static bool IsToggleable(Component component)
        {
            return component is Behaviour || component is Renderer || component is Collider;
        }

        private static string GetCachedLayerName(int layer)
        {
            if (layer < 0 || layer >= s_LayerNames.Length) return string.Empty;
            var name = s_LayerNames[layer];
            if (string.IsNullOrEmpty(name))
            {
                name = LayerMask.LayerToName(layer);
                s_LayerNames[layer] = name;
            }
            return name;
        }
    }
}
