using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Snorlax.Database.Editor
{
    public class DbCollectionTreeView : TreeView
    {
        public List<TreeViewItem> Items { get; set; } = new List<TreeViewItem>();

        public DbCollectionTreeView(TreeViewState state)
            : base(state)
        {
            showBorder = false;
            Reload();
        }

        public DbCollectionTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader)
            : base(state, multiColumnHeader)
        {
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            SetupParentsAndChildrenFromDepths(root, Items);

            return root;
        }
    }
}