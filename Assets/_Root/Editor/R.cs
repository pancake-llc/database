using System;
using UnityEditor;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public static class R
    {
        public const int MIN_ROW_HEIGHT = 22;
        public const int TABLE_HEADER_HEIGHT = 18;
        public static readonly GUILayoutOption[] OptionsMinRect = GetOptions(MIN_ROW_HEIGHT, MIN_ROW_HEIGHT);

        private static GUIStyle splitterPaneLeft;
        private static GUIStyle splitterPaneRight;
        private static GUIStyle buttonAdd;
        private static GUIStyle buttonDelete;
        private static GUIStyle cellHeader;
        private static GUIStyle cell;
        private static GUIStyle cellSelected;


        public static GUILayoutOption[] GetOptions(float width, float height) { return new[] {GUILayout.Width(width), GUILayout.Height(height)}; }

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
                    () => new GUIStyle {padding = new RectOffset(4, 4, 4, 4), margin = new RectOffset(4, 0, 0, 0), border = new RectOffset(4, 4, 4, 4), normal = null});
            }
        }

        public static GUIStyle SplitterPanelRight
        {
            get
            {
                return Get(ref splitterPaneRight,
                    () => new GUIStyle {padding = new RectOffset(4, 4, 4, 4), margin = new RectOffset(0, 4, 0, 0), border = new RectOffset(4, 4, 4, 4), normal = null});
            }
        }

        public static GUIStyle CellHeader
        {
            get
            {
                return Get(ref cellHeader,
                    () => new GUIStyle
                    {
                        fontSize = 12,
                        padding = new RectOffset(4, 4, 4, 4),
                        border = new RectOffset(8, 8, 8, 8),
                        alignment = TextAnchor.MiddleCenter,
                        normal = {textColor = new Color(.77f, .77f, .77f, 1), background = Texture1X1(new Color(0.33f, 0.33f, 0.33f))}
                    });
            }
        }

        public static GUIStyle ButtonAdd
        {
            get
            {
                return Get(ref buttonAdd,
                    () => new GUIStyle
                    {
                        border = new RectOffset(4, 4, 4, 4),
                        fixedWidth = MIN_ROW_HEIGHT,
                        fixedHeight = MIN_ROW_HEIGHT,
                        normal = {background = null},
                        active = {background = null}
                    });
            }
        }

        public static GUIStyle ButtonDelete
        {
            get { return Get(ref buttonDelete, () => new GUIStyle {border = new RectOffset(2, 2, 16, 4), normal = {background = null}, active = {background = null}}); }
        }

        public static GUIStyle Cell
        {
            get
            {
                return Get(ref cell,
                    () => new GUIStyle
                    {
                        padding = new RectOffset(4, 4, 4, 4),
                        border = new RectOffset(8, 8, 8, 8),
                        clipping = TextClipping.Clip,
                        richText = false,
                        normal = {background = null, textColor = new Color(.77f, .77f, .77f, 1)},
                        active = {background = null, textColor = new Color(.77f, .77f, .77f, 1)},
                        focused = {background = null, textColor = new Color(.77f, .77f, .77f, 1)}
                    });
            }
        }

        public static GUIStyle CellSelected
        {
            get
            {
                return Get(ref cellSelected,
                    () => new GUIStyle
                    {
                        padding = new RectOffset(4, 4, 4, 4),
                        border = new RectOffset(8, 8, 8, 8),
                        clipping = TextClipping.Clip,
                        richText = false,
                        normal = {textColor = new Color(.77f, .77f, .77f, 1), background = Texture1X1(new Color(0.31f, 0.42f, 0.2f))},
                        active = {background = Texture1X1(new Color(0.31f, 0.42f, 0.2f))}
                    });
            }
        }

        public static Texture2D Texture1X1(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply(false);
            return texture;
        }
        
        public static bool IsCollectionForDB(this string value) => value[0].Equals('[') && value[value.Length - 1].Equals(']');
    }
}