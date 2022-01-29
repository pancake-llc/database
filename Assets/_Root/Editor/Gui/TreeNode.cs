using System;
using System.Collections.Generic;
using System.Linq;
using Snorlax.Common;
using Snorlax.Editor;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public class TreeNode<T> where T : IName
    {
        private const int MAX_DEPTH = 1024;

        private readonly TreeView<T> _tree;
        private TreeNode<T> _parent;
        private List<TreeNode<T>> _childrens;
        private readonly T _model;

        public bool Collapsed { get; set; }
        public T Model => _model;

        public TreeNode<T> Parent
        {
            get => _parent;
            set
            {
                if (_parent == value) return;

                if (_parent != null && _parent._childrens != null) _parent._childrens.Remove(this);
                _parent = value;
                if (_parent == null) return;

                if (_tree.roots.Contains(this)) _tree.roots.Remove(this); // remove from old parent

                //recursion check
                var currentParent = _parent;
                var nestingLevel = 0;
                while (currentParent != null)
                {
                    if (currentParent == this) throw new Exception("Recursion check fail!");
                    if (nestingLevel > MAX_DEPTH) throw new Exception("Recursion check fail! Unacceptable Nesting Level " + nestingLevel);

                    currentParent = currentParent._parent;
                    nestingLevel++;
                }

                // add to parent
                _parent._childrens ??= new List<TreeNode<T>>();
                if (!_parent._childrens.Contains(this)) _parent._childrens.Add(this);
            }
        }

        public int Level
        {
            get
            {
                if (Parent == null) return 0;

                var parentLevel = Parent.Level;
                if (parentLevel > MAX_DEPTH) throw new Exception("Recursion check fail! Unacceptable Nesting Level " + parentLevel);

                return parentLevel + 1;
            }
        }

        public TreeNode(TreeView<T> tree, T model)
        {
            if (tree == null) throw new Exception("tree can not be null!");

            _tree = tree;
            _model = model;
        }

        public void ExpandCollapsed(bool collapsed, bool rescursive)
        {
            Collapsed = collapsed;
            if (rescursive) IterateChildren(child => child.ExpandCollapsed(collapsed, true));
        }

        private void IterateChildren(Action<TreeNode<T>> action)
        {
            if (_childrens.IsNullOrEmpty()) return;

            foreach (var child in _childrens) action(child);
        }

        public TreeNode<T> Find(Predicate<T> func) { return func(Model) ? this : _childrens?.Select(_ => _.Find(func)).FirstOrDefault(result => result != null); }

        public Vector2 OnGUI()
        {
            _tree.currentRow++;
            var result = Vector2.zero;
            var position = Vector2.zero;
            var nodeTexture = new Texture2D(0, 0);

            var level = Level;
            UtilEditor.Horizontal(() =>
            {
                var textureRectWidth = 16 * (level + 1);
                //reserve space for texture
                var nodeTextureRect = GUILayoutUtility.GetRect(textureRectWidth, 0, new GUIStyle {fixedWidth = textureRectWidth}, GUILayout.Width(textureRectWidth));
                _tree.OnGUI(_model);

                var availableHeight = GUILayoutUtility.GetLastRect().height;
                position = result = new Vector2(nodeTextureRect.xMax, nodeTextureRect.y + availableHeight / 2f);
                var rect = new Rect(nodeTextureRect.xMax - nodeTexture.width, nodeTextureRect.y + (availableHeight - nodeTexture.height) / 2f, nodeTexture.width, nodeTexture.height);
                if (HasChildren)
                {
                    if (GUI.Button(rect, nodeTexture, GUIStyle.none)) Collapsed = !Collapsed;
                }
                else
                {
                    if (level == 0) GUI.DrawTexture(rect, nodeTexture);
                }
            });

            if (HasChildren) result.x -= nodeTexture.width;
            if (!HasChildren || Collapsed) return result;

            var texture = _tree.LinkTexture;
            float linkStartX = position.x;
            float linkStartY = position.y;

            var connectorPosition = Vector2.zero;
            UtilEditor.Vertical(() =>
            {
                foreach (var children in _childrens)
                {
                    connectorPosition = children.OnGUI();
                    if (Event.current.type == EventType.Repaint)
                    {
                        var p = new Rect(linkStartX, linkStartY, connectorPosition.x - linkStartX, 1);
                        GUI.DrawTexture(p, texture, ScaleMode.StretchToFill);
                    }
                }
            });

            if (Event.current.type == EventType.Repaint)
            {
                var p = new Rect(linkStartX, linkStartY, 1, connectorPosition.y - linkStartY);
                GUI.DrawTexture(p, texture, ScaleMode.StretchToFill);
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return EqualityComparer<T>.Default.Equals(_model, ((TreeNode<T>) obj)._model);
        }

        public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(_model);

        private bool HasChildren => _childrens.IsNullOrEmpty();
    }
}