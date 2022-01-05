using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public class DbCollectionTreeView : TreeView
    {
        public List<TreeViewItem> Items { get; set; } = new List<TreeViewItem>();
        public int selectedId;
        public Action<string> onSelected;

        public DbCollectionTreeView(TreeViewState state)
            : base(state)
        {
            showAlternatingRowBackgrounds = false;
            showBorder = false;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            SetupParentsAndChildrenFromDepths(root, Items);

            return root;
        }

        protected override void ContextClickedItem(int id)
        {
            if (id == 1) return;

            selectedId = id;
            var e = Event.current;
            e.Use();

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Open"), false, OnOpenTable);
            menu.ShowAsContext();
        }

        private void OnOpenTable()
        {
            Debug.Log("Open Table: " + Items[selectedId - 1].displayName);
            onSelected?.Invoke(Items[selectedId - 1].displayName);
        }
    }
}