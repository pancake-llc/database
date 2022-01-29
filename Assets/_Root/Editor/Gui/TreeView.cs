using System;
using System.Collections.Generic;
using Snorlax.Editor;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public abstract class TreeView<T> : View where T : IName
    {
        public readonly List<TreeNode<T>> roots = new List<TreeNode<T>>();
        internal int currentRow;
        private Texture2D _linkTexture;
        private T _selected;
        private readonly string _name;
        public Action onAdded;
        public Action<T> onSelected;
        public Action<T> onDeleted;
        public Action customHeader;

        public T Selected
        {
            get => _selected;
            set
            {
                bool changed = !Equals(_selected, value);
                _selected = value;
                if (changed && onSelected != null)
                {
                    onSelected.Invoke(_selected);
                    GUIUtility.ExitGUI();
                }
            }
        }

        public Texture2D LinkTexture
        {
            get
            {
                if (_linkTexture != null) return _linkTexture;

                _linkTexture = R.Texture1X1(new Color(.77f, .77f, .77f, 1));
                return _linkTexture;
            }
        }

        protected TreeView(string name) { _name = name; }

        public override void GUI()
        {
            if (customHeader != null)
            {
                customHeader();
            }
            else
            {
                UtilEditor.Horizontal(() =>
                {
                    if (onAdded != null && GUILayout.Button("", R.ButtonAdd, R.OptionsMinRect))
                    {
                        onAdded();
                        GUIUtility.keyboardControl = 0; // refresh
                        GUIUtility.ExitGUI();
                    }

                    GUILayout.Label(_name, R.CellHeader, GUILayout.Height(R.MIN_ROW_HEIGHT));

                    if (onDeleted != null && GUILayout.Button("", R.ButtonDelete, R.OptionsMinRect))
                    {
                        DBEditorUtil.ExitIf(_selected == null, "Please, select a row to remove");
                        onDeleted(_selected);
                    }
                });
            }
        }

        public void OnGUI(T model)
        {
            bool isCurrent = Equals(Selected, model);
            var guiStyle = isCurrent ? R.CellSelected : R.Cell;

            ItemUI(model, guiStyle, isCurrent);
            var @event = Event.current;
            if (@event.type == EventType.MouseUp && GUILayoutUtility.GetLastRect().Contains(@event.mousePosition))
            {
                Selected = model;
            }
        }

        private void ShowData()
        {
            currentRow = 0;
            foreach (var treeNode in roots)
            {
                treeNode.OnGUI();
            }
        }

        protected virtual void ItemUI(T model, GUIStyle style, bool isCurrent) { GUILayout.Label(model.Name, style); }
    }
}