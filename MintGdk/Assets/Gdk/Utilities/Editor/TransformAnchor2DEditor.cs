using UnityEngine;
using UnityEditor;
using Mint.Gdk.Utilities.Runtime;

namespace Mint.Gdk.Utilities.Editor
{
    [CustomEditor(typeof(TransformAnchor2D))]
    public class TransformAnchor2DEditor : UnityEditor.Editor
    {
        private const float PreviewWidth = 250f;
        private const float PreviewHeight = 250f;
        private bool _isDragging = false;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            TransformAnchor2D transformAnchor = (TransformAnchor2D)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transform Anchor Settings", EditorStyles.boldLabel);
            DrawDefaultInspector();
            EditorGUI.BeginChangeCheck();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (Application.isPlaying)
                {
                    transformAnchor.UpdatePosition();
                }
            }

            EditorGUILayout.Space();

            // Display anchor info
            Vector2 anchor = transformAnchor.Anchor;
            EditorGUILayout.HelpBox(
                $"Viewport Position: ({anchor.x:F2}, {anchor.y:F2})\n" +
                $"0,0 = Bottom-Left | 1,1 = Top-Right | 0.5,0.5 = Center",
                MessageType.Info);

            EditorGUILayout.Space();

            // Draw visual preview in Inspector
            DrawInspectorPreview(anchor, transformAnchor);

            EditorGUILayout.Space(20);

            // Update button
            if (GUILayout.Button("Update Position", GUILayout.Height(30)))
            {
                transformAnchor.UpdatePosition();
            }

            // Quick anchor presets
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Top-Left")) SetAnchor(new Vector2(0, 1));
            if (GUILayout.Button("Top-Center")) SetAnchor(new Vector2(0.5f, 1));
            if (GUILayout.Button("Top-Right")) SetAnchor(new Vector2(1, 1));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Middle-Left")) SetAnchor(new Vector2(0, 0.5f));
            if (GUILayout.Button("Center")) SetAnchor(new Vector2(0.5f, 0.5f));
            if (GUILayout.Button("Middle-Right")) SetAnchor(new Vector2(1, 0.5f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Bottom-Left")) SetAnchor(new Vector2(0, 0));
            if (GUILayout.Button("Bottom-Center")) SetAnchor(new Vector2(0.5f, 0));
            if (GUILayout.Button("Bottom-Right")) SetAnchor(new Vector2(1, 0));
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInspectorPreview(Vector2 anchor, TransformAnchor2D transformAnchor)
        {
            EditorGUILayout.LabelField("Visual Preview", EditorStyles.boldLabel);

            // Get rect for preview with calculated dimensions
            Rect previewRect = GUILayoutUtility.GetRect(PreviewWidth, PreviewHeight);
            previewRect.width = PreviewWidth;
            previewRect.height = PreviewHeight;

            // Draw background
            EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.2f, 1f));

            // Draw border
            Handles.BeginGUI();
            Handles.color = new Color(0, 1, 0, 0.5f);
            Handles.DrawLine(new Vector3(previewRect.x, previewRect.y), new Vector3(previewRect.xMax, previewRect.y));
            Handles.DrawLine(new Vector3(previewRect.xMax, previewRect.y), new Vector3(previewRect.xMax, previewRect.yMax));
            Handles.DrawLine(new Vector3(previewRect.xMax, previewRect.yMax), new Vector3(previewRect.x, previewRect.yMax));
            Handles.DrawLine(new Vector3(previewRect.x, previewRect.yMax), new Vector3(previewRect.x, previewRect.y));

            // Draw grid lines
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            for (float i = 0.25f; i <= 0.75f; i += 0.25f)
            {
                // Vertical lines
                float x = previewRect.x + previewRect.width * i;
                Handles.DrawLine(new Vector3(x, previewRect.y), new Vector3(x, previewRect.yMax));

                // Horizontal lines
                float y = previewRect.y + previewRect.height * i;
                Handles.DrawLine(new Vector3(previewRect.x, y), new Vector3(previewRect.xMax, y));
            }

            // Draw corner labels
            GUI.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.normal.textColor = new Color(0, 1, 0, 0.8f);

            GUI.Label(new Rect(previewRect.x + 2, previewRect.yMax - 15, 50, 15), "(0,0)", labelStyle);
            GUI.Label(new Rect(previewRect.xMax - 35, previewRect.yMax - 15, 50, 15), "(1,0)", labelStyle);
            GUI.Label(new Rect(previewRect.x + 2, previewRect.y + 2, 50, 15), "(0,1)", labelStyle);
            GUI.Label(new Rect(previewRect.xMax - 35, previewRect.y + 2, 50, 15), "(1,1)", labelStyle);

            // Calculate anchor position in preview
            // Note: Y is flipped in GUI coordinates (0 is top, 1 is bottom)
            float anchorX = previewRect.x + previewRect.width * anchor.x;
            float anchorY = previewRect.y + previewRect.height * (1 - anchor.y); // Flip Y for GUI

            // Draw crosshair at anchor position
            Handles.color = Color.yellow;
            float crossSize = 8f;
            Handles.DrawLine(
                new Vector3(anchorX - crossSize, anchorY),
                new Vector3(anchorX + crossSize, anchorY));
            Handles.DrawLine(
                new Vector3(anchorX, anchorY - crossSize),
                new Vector3(anchorX, anchorY + crossSize));

            // Draw anchor point circle
            Handles.color = new Color(1f, 1f, 0f, 0.8f);
            Handles.DrawSolidDisc(new Vector3(anchorX, anchorY), Vector3.forward, 5f);

            Handles.color = Color.yellow;
            Handles.DrawWireDisc(new Vector3(anchorX, anchorY), Vector3.forward, 5f);

            // Draw anchor label
            GUIStyle anchorLabelStyle = new GUIStyle(EditorStyles.whiteBoldLabel);
            anchorLabelStyle.fontSize = 10;
            anchorLabelStyle.normal.textColor = Color.yellow;
            string anchorLabel = $"({anchor.x:F2}, {anchor.y:F2})";
            Vector2 labelSize = anchorLabelStyle.CalcSize(new GUIContent(anchorLabel));

            Rect labelRect = new Rect(anchorX - labelSize.x / 2, anchorY + 10, labelSize.x + 8, labelSize.y + 4);
            EditorGUI.DrawRect(labelRect, new Color(0, 0, 0, 0.7f));
            GUI.Label(new Rect(anchorX - labelSize.x / 2 + 4, anchorY + 10, labelSize.x, labelSize.y), anchorLabel, anchorLabelStyle);

            Handles.EndGUI();

            // Handle mouse input for dragging
            Event e = Event.current;
            if (e.type == EventType.MouseDown && previewRect.Contains(e.mousePosition))
            {
                _isDragging = true;
                e.Use();
            }

            if (_isDragging && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown))
            {
                Vector2 localPos = e.mousePosition - new Vector2(previewRect.x, previewRect.y);
                Vector2 normalizedPos = new Vector2(
                    Mathf.Clamp01(localPos.x / previewRect.width),
                    Mathf.Clamp01(1 - (localPos.y / previewRect.height)) // Flip Y back to viewport space
                );

                transformAnchor.SetAnchor(normalizedPos);
                serializedObject.ApplyModifiedProperties();

                if (Application.isPlaying)
                {
                    transformAnchor.UpdatePosition();
                }

                Repaint();
                e.Use();
            }

            if (e.type == EventType.MouseUp)
            {
                _isDragging = false;
            }

            // Add instruction text
            GUI.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            EditorGUILayout.LabelField("Click or drag in the preview to set anchor position", EditorStyles.centeredGreyMiniLabel);
            GUI.color = Color.white;
        }

        private void SetAnchor(Vector2 anchor)
        {
            TransformAnchor2D transformAnchor = (TransformAnchor2D)target;
            transformAnchor.SetAnchor(anchor);
            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
            {
                transformAnchor.UpdatePosition();
            }
        }
    }
}