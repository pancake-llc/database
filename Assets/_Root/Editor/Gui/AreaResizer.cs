using System;
using UnityEditor;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    /// <summary>
    /// resize area
    /// </summary>
    public class AreaResizer
    {
        private readonly Vector2 _size = new Vector2(4, 4);
        private float _startPosition = -1;
        private readonly bool _isSkipUi;
        private readonly bool _isVertical;

        public AreaResizer(bool isSkipUi = false, bool isVertical = false)
        {
            _isSkipUi = isSkipUi;
            _isVertical = isVertical;
        }

        public bool Active => _startPosition >= 0;

        public void Process(int width, float height, Action<float> onResizeStart, Action<float> onDrag)
        {
            Process(GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(width), GUILayout.Height(height)), onResizeStart, onDrag);
        }

        public void Process(Rect rect, Action<float> onResizeStart, Action<float> onDrag)
        {
            OnGui(rect);

            Input(rect, onResizeStart, onDrag);
        }

        public void OnGui(Rect rect)
        {
            if (_isSkipUi) GUI.DrawTextureWithTexCoords(rect, null, new Rect(0, 0, rect.width / _size.x, rect.height / _size.y));
        }

        public void Input(Rect rect, Action<float> onResizeStart, Action<float> onDrag)
        {
            var position = Event.current.mousePosition;

            if (_startPosition > 0 && Event.current.type == EventType.MouseUp)
            {
                _startPosition = -1;
                return;
            }

            EditorGUIUtility.AddCursorRect(rect, _isVertical ? MouseCursor.ResizeVertical : MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown)
            {
                if (rect.Contains(position))
                {
                    _startPosition = _isVertical ? position.y : position.x;
                    onResizeStart?.Invoke(_startPosition);
                    return;
                }
            }

            if (_startPosition > 0 && Event.current.type == EventType.MouseDrag)
            {
                if (_isVertical)
                {
                    onDrag?.Invoke(position.y - _startPosition);
                }
                else
                {
                    onDrag?.Invoke(position.x - _startPosition);
                }
            }
        }
    }
}