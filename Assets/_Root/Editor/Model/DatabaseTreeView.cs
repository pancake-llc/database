using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Pancake.DatabaseEditor
{
    public class DatabaseTreeView : TreeView
    {
        public List<TreeViewItem> Items { get; set; } = new List<TreeViewItem>();

        public DatabaseTreeView(TreeViewState state)
            : base(state)
        {
            showAlternatingRowBackgrounds = false;
            showBorder = false;
            cellMargin = 6;
            multiColumnHeader.sortingChanged += OnSortingChanged;
            multiColumnHeader.ResizeToFit();
            Reload();
        }

        private void OnSortingChanged(MultiColumnHeader multicolumnheader)
        {
            Sort(GetRows());
            Repaint();
        }

        public DatabaseTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader)
            : base(state, multiColumnHeader)
        {
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            SetupParentsAndChildrenFromDepths(root, Items);

            return root;
        }

        private void Sort(IList<TreeViewItem> rows)
        {
            // if (multiColumnHeader.sortedColumnIndex == -1)
            //     return;
            //
            // if (rows.Count == 0)
            //     return;
            //
            // int sortedColumn = multiColumnHeader.sortedColumnIndex;
            // var childrens = rootItem.children.Cast<DatabaseViewerItem>();
            //
            // var comparer = new Comparer(CultureInfo.CurrentCulture);
            // var ordered = multiColumnHeader.IsSortedAscending(sortedColumn) ? childrens.OrderBy(k => k.properties[sortedColumn], comparer) : childrens.OrderByDescending(k => k.properties[sortedColumn], comparer);
            //
            // rows.Clear();
            // foreach (var v in ordered)
            //     rows.Add(v);
        }
    }
}