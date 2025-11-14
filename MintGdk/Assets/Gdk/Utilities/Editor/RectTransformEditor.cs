using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Mint.Gdk.Utilities.Editor
{
    [CustomEditor(typeof(RectTransform), true)]
    public class RectTransformEditor : UnityEditor.Editor
    {
        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private UnityEditor.Editor _editorInstance;

        private void OnEnable()
        {
            Assembly ass = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            Type rtEditor = ass.GetType("UnityEditor.RectTransformEditor");
            _editorInstance = CreateEditor(target, rtEditor);
        }

        public override void OnInspectorGUI()
        {
            _editorInstance.OnInspectorGUI();
            var defaultColor = GUI.color;
            GUI.color = Event.current.type == EventType.Repaint ? Color.yellow : new Color(1f, 0.92f, 0.016f, 1f);
            if (GUILayout.Button("Update Anchors"))
            {
                UpdateAnchors();
            }
            GUI.color = defaultColor;
        }

        private void OnSceneGUI()
        {
            MethodInfo onSceneGUI_Method = _editorInstance.GetType().GetMethod("OnSceneGUI", BindingFlags.NonPublic | BindingFlags.Instance);
            onSceneGUI_Method.Invoke(_editorInstance, null);
        }

        void OnDisable()
        {
            DestroyImmediate(_editorInstance);
        }

        public void UpdateAnchors()
        {
            if (_rectTransform == null || _parentRectTransform == null)
            {
                _rectTransform = (RectTransform)target;
                _parentRectTransform = _rectTransform.parent.GetComponent<RectTransform>();
            }
            Undo.RecordObject(_rectTransform, "Update RectTransform Anchors");
            // Tìm kích thước của RectTransform và đối tượng cha
            Vector2 rectSize = _rectTransform.rect.size;
            Vector2 parentSize = _parentRectTransform.rect.size;
            var localPosition = _rectTransform.localPosition;
            var top = localPosition.y - (rectSize.y * _rectTransform.pivot.y) + parentSize.y * _parentRectTransform.pivot.y;
            var bottom = localPosition.y + (rectSize.y * (1 - _rectTransform.pivot.y)) + parentSize.y * _parentRectTransform.pivot.y;
            var left = localPosition.x - (rectSize.x * _rectTransform.pivot.x) + parentSize.x * _parentRectTransform.pivot.x;
            var right = localPosition.x + (rectSize.x * (1 - _rectTransform.pivot.x)) + parentSize.x * _parentRectTransform.pivot.x;

            // Tính toán tỉ lệ giữa cạnh trên/dưới và cạnh trái/phải của RectTransform so với đối tượng cha
            float anchorMinY = top / parentSize.y;
            float anchorMaxY = bottom / parentSize.y;

            float anchorMinX = left / parentSize.x;
            float anchorMaxX = right / parentSize.x;

            // Cập nhật anchorMin và anchorMax
            _rectTransform.anchorMin = new Vector2(anchorMinX, anchorMinY);
            _rectTransform.anchorMax = new Vector2(anchorMaxX, anchorMaxY);

            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            _rectTransform.localPosition = new Vector3(localPosition.x, localPosition.y, localPosition.z);

            EditorUtility.SetDirty(_rectTransform); // Mark as dirty so it saves correctly
        }
    }
}