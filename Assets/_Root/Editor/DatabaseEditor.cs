using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Snorlax.Common;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public class DatabaseEditor : EditorWindow
    {
        private LiteDatabase _db; // connect once database at a time
        private ConnectionString _connectionString; // connect string for open db
        private bool _enableButtonHeader = true;
        private bool _isConnected;
        private TaskData _task;

        [SerializeField] private TreeViewState treeViewState;
        private DatabaseCollectionTreeView _treeView;

        [SerializeField] private TreeViewState filterTreeViewState;
        private DatabaseTreeView _databaseTreeView;
        private readonly List<string> _headerData = new List<string>();
        private MultiColumnHeaderState _multiColumnHeaderState;
        private MultiColumnHeader _multiColumnHeader;
        private MultiColumnHeaderState.Column[] _columns;
        private Vector2 _tableViewScrollPosition;
        private float _multiColumnHeaderWidth;
        private GUIStyle _groupStyle;
        private List<TableRowData> _rowDatas = new List<TableRowData>();

        private Rect _positionEntryPoint;
        private const float REMOVE_BTN_WIDTH = 30f;
        private const float DRAG_EDIT_WIDTH = 5f;

        private float SingleRowHeight => EditorGUIUtility.singleLineHeight;

        public void Initialize()
        {
            _task = new TaskData();
            treeViewState ??= new TreeViewState();
            filterTreeViewState ??= new TreeViewState();
            SceneView.duringSceneGui += DuringSceneGUI;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private void DuringSceneGUI(SceneView sceneView) { }

        private void PlayModeStateChanged(PlayModeStateChange stateChange) { }

        private void OnGUI()
        {
            _task ??= new TaskData();
            _treeView ??= new DatabaseCollectionTreeView(treeViewState) {onSelected = OnSetCurrentTableSelected};
            _groupStyle ??= new GUIStyle(GUI.skin.box);

            #region header

            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            if (!_enableButtonHeader) GUI.enabled = false;

            if (_isConnected)
            {
                if (GUILayout.Button("Disconnect", EditorStyles.miniButtonLeft, GUILayout.Width(85)))
                {
                    Disconnect();
                }
            }
            else
            {
                if (GUILayout.Button("Connect", EditorStyles.miniButtonLeft, GUILayout.Width(85)))
                {
                    OnButtonClickConnect();
                }
            }


            if (GUILayout.Button("Refresh", EditorStyles.miniButtonMid, GUILayout.Width(85)))
            {
            }

            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

            #endregion

            #region body

            #region treeview

            var treeRect = EditorGUILayout.BeginVertical(_groupStyle, GUILayout.Width(150), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
            EditorGUILayout.LabelField("Tree View",
                new GUIStyle(EditorStyles.label) {alignment = TextAnchor.UpperCenter, fontSize = 13, richText = true},
                GUILayout.Height(16),
                GUILayout.Width(150));
            // todo show db tree
            _treeView?.OnGUI(GUILayoutUtility.GetRect(0, float.MaxValue, 0, float.MaxValue));
            EditorGUILayout.EndVertical();

            #endregion

            #region content

            var windowRect = GUILayoutUtility.GetLastRect();
            if (_task != null && !string.IsNullOrEmpty(_task.NameTableSelected))
            {
                // Here we are basically assigning the size of window to our newly positioned `windowRect`.
                windowRect.width = position.width;
                windowRect.height = position.height;

                // After compilation and some other events data of the window is lost if it's not saved in some kind of container. Usually those containers are ScriptableObject(s).
                if (_multiColumnHeader == null) InitializeHeader();

                //! Here we just basically move around group. It's not really padding, we are just setting position and reducing size.
                var groupRectPaddingInWindow = new Vector2(treeRect.width + 5, 0);
                var groupRect = new Rect(windowRect);

                groupRect.x += groupRectPaddingInWindow.x;
                groupRect.width -= groupRectPaddingInWindow.x + 10;

                groupRect.y += groupRectPaddingInWindow.y;
                groupRect.height -= groupRectPaddingInWindow.y + 32;

                GUI.BeginGroup(groupRect);
                {
                    groupRect.x -= groupRectPaddingInWindow.x;
                    groupRect.y -= groupRectPaddingInWindow.y;

                    var positionalRectAreaOfScrollView = new Rect(groupRect);
                    positionalRectAreaOfScrollView.x -= 5;
                    positionalRectAreaOfScrollView.y -= 32;

                    var positionalRectAreaOfScrollView2 = new Rect(groupRect);

                    // Create a `viewRect` since it should be separate from `rect` to avoid circular dependency.
                    var viewRect = new Rect(groupRect)
                    {
                        width = _multiColumnHeaderState.widthOfAllVisibleColumns, // Scroll max on X is basically a sum of width of columns.
                        //xMax = _columns.Sum((column) => column.width),
                        //? Do not remove this hegiht. It's compensating for the size of bottom scroll slider when it appears, that is why the right side scroll slider appears.
                        //height = groupRect.height - columnHeight, // Remove `columnHeight` - basically size of header.
                    };

                    groupRect.width += groupRectPaddingInWindow.x * 2;
                    groupRect.height += groupRectPaddingInWindow.y * 2;

                    _tableViewScrollPosition = GUI.BeginScrollView(positionalRectAreaOfScrollView,
                        _tableViewScrollPosition,
                        viewRect,
                        false,
                        false);
                    {
                        // Scroll View Scope.

                        //? After debugging for a few hours - this is the only hack I have found to actually work to aleviate that scaling bug.
                        _multiColumnHeaderWidth = Mathf.Max(positionalRectAreaOfScrollView2.width + _tableViewScrollPosition.x, _multiColumnHeaderWidth);

                        // This is a rect for our multi column table.
                        var columnRectPrototype = new Rect(positionalRectAreaOfScrollView2)
                        {
                            width = _multiColumnHeaderWidth, height = SingleRowHeight // This is basically a height of each column including header.
                        };

                        // Draw header for columns here.
                        _multiColumnHeader.OnGUI(columnRectPrototype, 0.0f);

                        DrawTableContent(columnRectPrototype);
                    }
                    GUI.EndScrollView(true);
                }
                GUI.EndGroup();
            }

            #endregion

            #endregion
        }

        #region func

        private void OnButtonClickConnect()
        {
            if (_db == null) ConnectionDatabaseEditorStatic.Show(_connectionString ?? new ConnectionString(), Connect);
        }

        private async void Connect(ConnectionString connectString)
        {
            _enableButtonHeader = false;
            try
            {
                _db = await AsyncConnect(connectString);
                int userVersion = _db.UserVersion;
            }
            catch (Exception e)
            {
                _db?.Dispose();
                _db = null;

                EditorUtility.DisplayDialog("Connecting Error", e.Message, "Ok");
                return;
            }
            finally
            {
                _enableButtonHeader = true;
            }

            _enableButtonHeader = true;
            _connectionString = connectString;
            _isConnected = true;
            LoadTreeView();
        }

        private void Disconnect()
        {
            _isConnected = false;

            try
            {
                _db?.Dispose();
                _db = null;
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", e.Message, "Ok");
            }

            _treeView.Items.Clear();
            _treeView.Reload();
        }

        private async Task<LiteDatabase> AsyncConnect(ConnectionString connectionString) { return await Task.Run(() => new LiteDatabase(connectionString)); }

        private void PopulateRecentList()
        {
            var recents = SettingManager.Settings.RecentConnectionStrings;
        }

        private void LoadTreeView()
        {
            _treeView.Items.Clear();
            _treeView.Items.Add(new TreeViewItem {id = 1, depth = 0, displayName = Path.GetFileName(_connectionString.Filename)});

            string[] collections = _db.GetCollectionNames().OrderBy(_ => _).ToArray();

            for (var i = 0; i < collections.Length; i++)
            {
                if (!_treeView.Items.Exists(_ => _.id == i + 2)) _treeView.Items.Add(new TreeViewItem {id = i + 2, depth = 1, displayName = collections[i]});
            }

            _treeView.Reload();
            _treeView.ExpandAll();
        }

        private void Execute(TaskData task)
        {
            task.Parameters = new BsonDocument();
            string sql = string.Format(TaskData.SQL_QUERY, task.NameTableSelected);
            if (_db != null)
            {
                using (var reader = _db.Execute(sql, task.Parameters))
                {
                    task.ReadResult(reader);

                    LoadResult(task);
                }
            }
        }

        private void LoadResult(TaskData task)
        {
            if (task?.Result != null)
            {
                _rowDatas.Clear();
                foreach (var value in task.Result)
                {
                    var doc = value.IsDocument ? value.AsDocument : new BsonDocument {["[value]"] = value};
                    if (doc.Keys.Count == 0) doc["[root]"] = "{}";

                    foreach (string key in doc.Keys)
                    {
                        if (!_headerData.Contains(key))
                        {
                            _headerData.Add(key);
                        }
                    }

                    var record = CreateInstance<TableRowData>();
                    InitializeRecordData(doc, ref record);
                    _rowDatas.Add(record);
                }

                InitializeHeader();
            }
        }

        private void OnSetCurrentTableSelected(string name)
        {
            _task.NameTableSelected = name;
            _columns = null;
            _multiColumnHeader = null;
            _multiColumnHeaderState = null;
            _headerData.Clear();
            Execute(_task);
        }

        private void InitializeHeader()
        {
            _multiColumnHeaderWidth = position.width;
            _columns = new MultiColumnHeaderState.Column[_headerData.Count];
            for (var i = 0; i < _headerData.Count; i++)
            {
                var type = TryConvertDataToType(_rowDatas[0].dictData[_headerData[i]], out _);
                _columns[i] = new MultiColumnHeaderState.Column
                {
                    allowToggleVisibility = false,
                    headerContent = new GUIContent(_headerData[i]),
                    minWidth = GetMinHeaderWidth(type),
                    maxWidth = GetMaxHeaderWidth(type),
                    autoResize = true,
                    canSort = false
                };

                _columns[i].width = _columns[i].minWidth;
            }

            _multiColumnHeaderState = new MultiColumnHeaderState(_columns);
            _multiColumnHeader = new MultiColumnHeader(_multiColumnHeaderState);
            _multiColumnHeader.visibleColumnsChanged += header => header.ResizeToFit();
            _multiColumnHeader.ResizeToFit();

            _databaseTreeView = new DatabaseTreeView(filterTreeViewState, _multiColumnHeader);
        }

        private void InitializeRecordData(BsonDocument document, ref TableRowData reader)
        {
            reader.dictData.Clear();
            var filters = document.RawValue;
            foreach (var value in filters)
            {
                reader.dictData.Add(value.Key, value.Value.ToString().Replace("\\", "").Replace("\"", ""));
            }
        }

        private float GetMinHeaderWidth(Type type)
        {
            float newSize = 100;
            if (type == typeof(bool))
            {
                newSize = 60;
            }
            else if (type == typeof(Vector2) || type == typeof(Vector2Int))
            {
                newSize = 125;
            }
            else if (type == typeof(Vector3) || type == typeof(Vector3Int))
            {
                newSize = 160;
            }
            else if (type == typeof(Vector4))
            {
                newSize = 200;
            }

            // todo
            return newSize;
        }

        private float GetMaxHeaderWidth(Type type)
        {
            float newSize = 250;
            // todo
            return newSize;
        }

        private bool CanSort(SerializedPropertyType type)
        {
            switch (type)
            {
                case SerializedPropertyType.AnimationCurve:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.BoundsInt:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.Vector4:
                    return false;
                default:
                    break;
            }

            return true;
        }

        private void DrawTableContent(Rect columnRectPrototype)
        {
            float heightJump = SingleRowHeight;
            for (int i = 0; i < _rowDatas.Count; i++)
            {
                var serializedObject = new SerializedObject(_rowDatas[i]);

                var calculatedRowHeight = 0f;
                var rowRect = new Rect(columnRectPrototype);
                rowRect.y += heightJump * (i + 1) * 1.3f;

                for (int j = 0; j < _columns.Length; j++)
                {
                    int visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(j);
                    var columnRect = _multiColumnHeader.GetColumnRect(visibleColumnIndex);
                    columnRect.y = rowRect.y + heightJump;

                    var nameFieldGUIStyle = new GUIStyle(GUI.skin.label) {padding = new RectOffset(10, 10, 2, 2)};

                    var pos = _multiColumnHeader.GetCellRect(visibleColumnIndex, columnRect);
                    pos.position -= new Vector2(-5, 0);
                    pos.width -= 5;
                    var type = TryConvertDataToType(_rowDatas[i].dictData[_headerData[j]], out object result);
                    if (type == typeof(bool))
                    {
                        int indexSelection = _rowDatas[i].dictData[_headerData[j]].ToLower().Equals("true") ? 0 : 1;
                        indexSelection = EditorGUI.Popup(pos, "", indexSelection, new[] {"True", "False"});
                        _rowDatas[i].dictData[_headerData[j]] = indexSelection == 0 ? "True" : "False";
                    }
                    else if (type == typeof(int))
                    {
                        _rowDatas[i].dictData[_headerData[j]] = EditorGUI.IntField(pos,
                                GUIContent.none,
                                int.Parse(_rowDatas[i].dictData[_headerData[j]]),
                                EditorStyles.textField)
                            .ToString();
                    }
                    else if (type == typeof(long))
                    {
                        _rowDatas[i].dictData[_headerData[j]] = EditorGUI.LongField(pos,
                                GUIContent.none,
                                long.Parse(_rowDatas[i].dictData[_headerData[j]]),
                                EditorStyles.textField)
                            .ToString();
                    }
                    else if (type == typeof(float))
                    {
                        _rowDatas[i].dictData[_headerData[j]] = EditorGUI.FloatField(pos,
                                GUIContent.none,
                                float.Parse(_rowDatas[i].dictData[_headerData[j]]),
                                EditorStyles.textField)
                            .ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(double))
                    {
                        _rowDatas[i].dictData[_headerData[j]] = EditorGUI.DoubleField(pos,
                                GUIContent.none,
                                double.Parse(_rowDatas[i].dictData[_headerData[j]]),
                                EditorStyles.textField)
                            .ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(DateTime))
                    {
                        _rowDatas[i].dictData[_headerData[j]] = EditorGUI.TextField(pos, GUIContent.none, _rowDatas[i].dictData[_headerData[j]], EditorStyles.textField);
                    }
                    else if (type == typeof(decimal))
                    {
                        _rowDatas[i].dictData[_headerData[j]] = EditorGUI.TextField(pos, GUIContent.none, _rowDatas[i].dictData[_headerData[j]], EditorStyles.textField);
                    }
                    else if (type == typeof(GuidConverter))
                    {
                        _rowDatas[i].dictData[_headerData[j]] = EditorGUI.TextField(pos, GUIContent.none, _rowDatas[i].dictData[_headerData[j]], EditorStyles.textField);
                    }
                    else if (type == typeof(ObjectId))
                    {
                        _rowDatas[i].dictData[_headerData[j]] = EditorGUI.TextField(pos, GUIContent.none, _rowDatas[i].dictData[_headerData[j]], EditorStyles.textField);
                    }
                    else if (type == typeof(Color))
                    {
                        _rowDatas[i].dictData[_headerData[j]].TryParseHtmlString(out var colorResult);
                        _rowDatas[i].dictData[_headerData[j]] = EditorGUI.ColorField(pos,
                                GUIContent.none,
                                colorResult,
                                true,
                                false,
                                false)
                            .ToHtmlStringRGBA();
                    }
                    else if (type == typeof(Vector2))
                    {
                        var vector2 = Vector2Converter.ValueOf(_rowDatas[i].dictData[_headerData[j]], StringConverter.Default.Culture);
                        vector2 = EditorGUI.Vector2Field(pos, GUIContent.none, vector2);
                    }
                    else if (type == typeof(Vector2Int))
                    {
                        var vector2Int = Vector2IntConverter.ValueOf(_rowDatas[i].dictData[_headerData[j]], StringConverter.Default.Culture);
                        vector2Int = EditorGUI.Vector2IntField(pos, GUIContent.none, vector2Int);
                    }
                    else if (type == typeof(Vector3))
                    {
                        var vector3 = Vector3Converter.ValueOf(_rowDatas[i].dictData[_headerData[j]], StringConverter.Default.Culture);
                        vector3 = EditorGUI.Vector3Field(pos, GUIContent.none, vector3);
                    }
                    else if (type == typeof(Vector3Int))
                    {
                        var vector3Int = Vector3IntConverter.ValueOf(_rowDatas[i].dictData[_headerData[j]], StringConverter.Default.Culture);
                        vector3Int = EditorGUI.Vector3IntField(pos, GUIContent.none, vector3Int);
                    }
                    else if (type == typeof(Vector4))
                    {
                        var vector4 = Vector3Converter.ValueOf(_rowDatas[i].dictData[_headerData[j]], StringConverter.Default.Culture);
                        vector4 = EditorGUI.Vector4Field(pos, GUIContent.none, vector4);
                    }
                    else if (type == typeof(string))
                    {
                        _rowDatas[i].dictData[_headerData[j]] = EditorGUI.TextField(pos, GUIContent.none, _rowDatas[i].dictData[_headerData[j]], EditorStyles.textField);
                    }
                }
            }
        }

        private static Type TryConvertDataToType(string data, out object result, CultureInfo cultureInfo = null)
        {
            StringConverter.Default.Culture = cultureInfo ?? CultureInfo.InvariantCulture;
            return StringConverter.Default.TryConvert(data, out result, out var type) ? type : null;
        }

        #endregion


        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            _db?.Dispose();
            _db = null;
        }
    }

    public static class DatabaseEditorStatic
    {
        [MenuItem("Tools/Snorlax/Show Database &_d")]
        public static DatabaseEditor Show()
        {
            var window = EditorWindow.GetWindow<DatabaseEditor>("Database", true);
            window.maxSize = new Vector2(1200, 800);
            window.minSize = new Vector2(600, 400);
            if (window != null)
            {
                window.Initialize();
                window.Show(true);
            }

            return window;
        }
    }
}