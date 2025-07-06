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
        private static readonly int DragToggleControlId = "ComfiHierarchyDragToggle".GetHashCode();
        
        static ComfiHierarchyDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }
        
        private static void OnHierarchyGUI(int instanceId, Rect rect)
        {
            if (!Settings.enabled) return;
            
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
            float nameWidth = GUI.skin.label.CalcSize(new GUIContent(go.name)).x + 18;
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
            
            Rect fullRect = new Rect(0, rect.y, rect.width + rect.x, rect.height);
            Color color = (rect.y / rect.height) % 2 == 0 ? Settings.rowColorEven : Settings.rowColorOdd;
            EditorGUI.DrawRect(fullRect, color);
        }
        
        private static void DrawTreeLines(GameObject go, Rect rect)
        {
            if (Event.current.type != EventType.Repaint) return;
            
            Transform transform = go.transform;
            Transform parent = transform.parent;
            
            if (parent == null) return;
            
            // Calculate depth and hierarchy info
            List<bool> hasNextSibling = new List<bool>();
            Transform current = transform;
            
            while (parent != null)
            {
                hasNextSibling.Insert(0, current.GetSiblingIndex() < parent.childCount - 1);
                current = parent;
                parent = parent.parent;
            }
            
            // Draw lines
            Handles.color = Settings.treeLineColor;
            float indent = 14f;
            // Calculate proper base position based on hierarchy depth
            float baseX = rect.x - (hasNextSibling.Count * indent) + indent - 7f;
            
            // Vertical lines for ancestors
            for (int i = 0; i < hasNextSibling.Count - 1; i++)
            {
                if (hasNextSibling[i])
                {
                    float x = baseX + (i * indent);
                    Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.y + rect.height));
                }
            }
            
            // Lines for current item
            bool isLastChild = transform.GetSiblingIndex() == transform.parent.childCount - 1;
            float currentX = baseX + ((hasNextSibling.Count - 1) * indent);
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
            Component[] components = go.GetComponents<Component>();
            bool firstComponent = true;
            
            foreach (var component in components)
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
                var icon = IconManager.GetIcon(component.GetType());
                DrawIcon(iconRect2, component, icon, IsToggleable(component));
                currentX -= iconSize + iconSpacing;
                
                // Stop if we run out of space
                if (currentX < iconArea.x) break;
            }
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
            GUI.color = tint;
            
            // Background
            if (Settings.iconBackgroundColor.a > 0)
            {
                EditorGUI.DrawRect(rect, Settings.iconBackgroundColor);
            }
            
            // Icon
            GUI.DrawTexture(rect, icon);
            GUI.color = Color.white;
            
            // Tooltip
            if (Settings.enableIconTooltips && target != null)
            {
                GUI.Label(rect, new GUIContent("", target.GetType().Name));
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
            
            // Left click toggle
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                e.Use();
                _dragToggleState = !isEnabled;
                _dragToggledObjects.Clear();
                
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
            var method = typeof(EditorUtility).GetMethod("DisplayObjectContextMenu", 
                BindingFlags.Static | BindingFlags.NonPublic, 
                null, 
                new[] { typeof(Rect), typeof(UnityEngine.Object[]), typeof(int) }, 
                null);
                
            if (method != null)
            {
                method.Invoke(null, new object[] { new Rect(Event.current.mousePosition, Vector2.zero), new[] { target }, 0 });
            }
        }
        
        private static float DrawLabels(GameObject go, Rect originalRect, float startX)
        {
            float currentX = startX;
            
            // Tag label (leftmost)
            if (Settings.showTagLabel && (Settings.showUntagged || !go.CompareTag("Untagged")))
            {
                Rect labelRect = new Rect(currentX, originalRect.y, Settings.tagLabelWidth, originalRect.height);
                DrawLabel(labelRect, go.tag, true);
                currentX += Settings.tagLabelWidth + 4;
            }
            
            // Layer label (after tag)
            if (Settings.showLayerLabel && (Settings.showDefaultLayer || go.layer != 0))
            {
                string layerName = LayerMask.LayerToName(go.layer);
                Rect labelRect = new Rect(currentX, originalRect.y, Settings.layerLabelWidth, originalRect.height);
                DrawLabel(labelRect, layerName, false);
                currentX += Settings.layerLabelWidth + 4;
            }
            
            return currentX;
        }
        
        private static void DrawLabel(Rect rect, string text, bool isTag)
        {
            var style = new GUIStyle(EditorStyles.label);
            style.fontSize = 9;
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            style.padding = new RectOffset(4, 4, 0, 0);
            
            // Optional: Add a subtle background
            var bgColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            EditorGUI.DrawRect(rect, bgColor);
            
            GUI.Label(rect, text, style);
        }
        
        private static bool IsToggleable(Component component)
        {
            return component is Behaviour || component is Renderer || component is Collider;
        }
    }
}