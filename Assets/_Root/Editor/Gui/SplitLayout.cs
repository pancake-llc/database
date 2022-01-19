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
                _leftViewStyle = new GUIStyle(R.SplitterPanelLeft) { fixedWidth = GetLeftWidth() };
                if (leftView != null)
                {
                    UtilEditor.Vertical(_leftViewStyle, () => { leftView.GUI(); }, GUILayout.MinHeight(height));
                }

                _resizer.Process(4,
                    height,
                    _ => _startWidth = GetLeftWidth(),
                    detal =>
                    {
                        float currentWidth = _startWidth + detal;
                        if (currentWidth < MIN_WIDTH) return;
                        SetLeftWidth(currentWidth);
                        _leftViewStyle.fixedWidth = GetLeftWidth();
                        GUIUtility.ExitGUI();
                    });

                if (rightView != null)
                {
                    UtilEditor.Vertical(R.SplitterPanelRight,
                        () =>
                        {
                            rightView.GUI();
                            GUILayout.MinHeight(height);
                        });
                }
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