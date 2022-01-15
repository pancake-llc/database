using System;
using Snorlax.Editor;
using UnityEditor;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public class SplitLayout<TLeft, TRight> : View where TLeft : View where TRight : View
    {
        private const float MIN_WIDTH = 80;
        private const float SPLIT_WIDTH = 5;

        public TLeft leftView;
        public TRight rightView;
        public EditorWindow window;
        public Func<float> heightGetter;

        private GUIStyle _leftViewStyle;
        private GUIStyle _rightViewStyle;
        private readonly AreaResizer _resizer;
        private float _leftWidth = 300;
        private float _startWidth;


        public SplitLayout(TLeft leftView, TRight rightView)
        {
            this.leftView = leftView;
            this.rightView = rightView;
            _resizer = new AreaResizer();
        }

        public virtual float GetLeftWidth() { return _leftWidth; }

        public virtual void SetLeftWidth(float width) { _leftWidth = width; }

        public override void GUI()
        {
            float height = heightGetter?.Invoke() ?? (window ? window : DatabaseEditorStatic.Show()).position.height - GUILayoutUtility.GetLastRect().yMax - 40;
            UtilEditor.Horizontal(() =>
            {
                
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            leftView?.Dispose();
            rightView?.Dispose();
        }
    }
}