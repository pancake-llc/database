using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Snorlax.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Snorlax.Database.Editor
{
    public class DatabaseEditor : EditorWindow
    {
        private LiteDatabase _db; // connect once database at a time
        private ConnectionString _connectionString; // connect string for open db
        private string _status;
        private bool _enableButtonHeader = true;
        private bool _isConnected;
        private TaskData _task;

        [SerializeField] private TreeViewState treeViewState;
        private DbCollectionTreeView _treeView;

        [SerializeField] private TreeViewState _filterTreeViewState;
        private DatabaseTreeView _databaseTreeView;

        public void Initialize()
        {
            _task = new TaskData();
            treeViewState ??= new TreeViewState();
            _filterTreeViewState ??= new TreeViewState();
            SceneView.duringSceneGui += DuringSceneGUI;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private void DuringSceneGUI(SceneView sceneView) { }

        private void PlayModeStateChanged(PlayModeStateChange stateChange) { }

        private void OnGUI()
        {
            _treeView ??= new DbCollectionTreeView(treeViewState) { onSelected = OnSetCurrentTableSelected };
           // _databaseTreeView ??= new DatabaseTreeView(_filterTreeViewState);

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

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();

            #region treeview

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(150), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
            EditorGUILayout.LabelField("Tree View",
                new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperCenter, fontSize = 13, richText = true },
                GUILayout.Height(16),
                GUILayout.Width(150));
            // todo show db tree

            _treeView?.OnGUI(GUILayoutUtility.GetRect(0, 100000, 0, 100000));
            EditorGUILayout.EndVertical();

            #endregion


            #region content

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Content",
                new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperCenter, fontSize = 13, richText = true },
                GUILayout.Height(16));
            // todo display editor db

            EditorGUILayout.EndVertical();

            #endregion

            EditorGUILayout.EndHorizontal();
            GUILayout.Label(_status, new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 12, richText = true }, GUILayout.Height(12));
            EditorGUILayout.EndVertical();

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
            _status = $"Openning {connectString.Filename} ...";
            await Task.Delay(100);
            try
            {
                _db = await AsyncConnect(connectString);
                int userVersion = _db.UserVersion;
                await Task.Delay(100);
                _status = "Done";
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
            await Task.Delay(100);
            _status = "";
            LoadTreeView();
        }

        private async void Disconnect()
        {
            _status = "Closing...";
            await Task.Delay(100);
            _isConnected = false;

            try
            {
                _db?.Dispose();
                _db = null;
                _status = "Done";
                await Task.Delay(100);
                _status = "";
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
            _treeView.Items.Add(new TreeViewItem { id = 1, depth = 0, displayName = Path.GetFileName(_connectionString.Filename) });

            string[] collections = _db.GetCollectionNames().OrderBy(_ => _).ToArray();

            for (var i = 0; i < collections.Length; i++)
            {
                if (!_treeView.Items.Exists(_ => _.id == i + 2)) _treeView.Items.Add(new TreeViewItem { id = i + 2, depth = 1, displayName = collections[i] });
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
                foreach (var value in task.Result)
                {
                    var doc = value.IsDocument ? value.AsDocument : new BsonDocument { ["[value]"] = value };
                    if (doc.Keys.Count == 0) doc["[root]"] = "{}";

                    foreach (string key in doc.Keys)
                    {
                        Debug.Log(key);
                    }
                }
            }
        }

        private void LoadFilterParameter(TaskData data)
        {
            // _filterTreeView.Items.Clear();
            // _filterTreeView.Items.Add(new TreeViewItem { id = 1, depth = 0, displayName = data.NameTableSelected });
            //
            // if (data?.Result != null)
            // {
            //     foreach (var value in data.Result)
            //     {
            //         var doc = value.IsDocument ? value.AsDocument : new BsonDocument { ["[value]"] = value };
            //         if (doc.Keys.Count == 0) doc["[root]"] = "{}";
            //
            //         var filters = doc.Keys.ToArray();
            //         for (int i = 0; i < filters.Length; i++)
            //         {
            //             if (!_filterTreeView.Items.Exists(_ => _.id == i + 2)) _filterTreeView.Items.Add(new TreeViewItem { id = i + 2, depth = 1, displayName = filters[i] });
            //         }
            //         
            //         break; // run first row
            //     }
            // }
            //
            // _filterTreeView.Reload();
            // _filterTreeView.ExpandAll();
        }

        private void OnSetCurrentTableSelected(string name)
        {
            _task.NameTableSelected = name; 
            Execute(_task);
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
        private static void Show()
        {
            var window = EditorWindow.GetWindow<DatabaseEditor>("Database", true, UtilEditor.GetInspectorWindowType());
            if (window != null)
            {
                window.Initialize();
                window.Show(true);
            }
        }
    }
}