using System;
using UnityEditor;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public static class R
    {
        private static GUIStyle splitterPaneLeft;
        private static GUIStyle splitterPaneRight;

        public static GUIStyle Get(ref GUIStyle style, Func<GUIStyle> getter)
        {
            if (style == null) getter();
            return style;
        }

        public static GUIStyle SplitterPanelLeft
        {
            get
            {
                return Get(ref splitterPaneLeft,
                    () => new GUIStyle { padding = new RectOffset(4, 4, 4, 4), margin = new RectOffset(4, 0, 0, 0), border = new RectOffset(4, 4, 4, 4), normal = null });
            }
        }

        public static GUIStyle SplitterPanelRight
        {
            get
            {
                return Get(ref splitterPaneRight,
                    () => new GUIStyle { padding = new RectOffset(4, 4, 4, 4), margin = new RectOffset(0, 4, 0, 0), border = new RectOffset(4, 4, 4, 4), normal = null });
            }
        }
    }
}