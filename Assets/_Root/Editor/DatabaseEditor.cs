using System;
using System.Collections;
using System.Threading.Tasks;
using LiteDB;
using Snorlax.Editor;
using UnityEditor;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public class DatabaseEditor : EditorWindow
    {
        private LiteDatabase _db; // connect once database at a time
        private ConnectionString _connectionString; // connect string for open db
        private string _status;
        private bool _enableButtonHeader = true;
        private bool _isConnected = false;

        public void Initialize()
        {
            SceneView.duringSceneGui += DuringSceneGUI;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private void DuringSceneGUI(SceneView sceneView) { }

        private void PlayModeStateChanged(PlayModeStateChange stateChange) { }

        private void OnGUI()
        {
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

            #region hierarchy

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(200), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
            EditorGUILayout.LabelField("Hierarchy",
                new GUIStyle(EditorStyles.label) {alignment = TextAnchor.UpperCenter, fontSize = 13, richText = true, contentOffset = new Vector2(0, -2)},
                GUILayout.Height(16));
            // todo show db tree

            EditorGUILayout.EndVertical();

            #endregion

            #region content

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Content",
                new GUIStyle(EditorStyles.label) {alignment = TextAnchor.UpperCenter, fontSize = 13, richText = true, contentOffset = new Vector2(0, -2)},
                GUILayout.Height(16));
            // todo display editor db

            EditorGUILayout.EndVertical();

            #endregion

            EditorGUILayout.EndHorizontal();
            GUILayout.Label(_status, new GUIStyle(EditorStyles.label) {alignment = TextAnchor.MiddleLeft, fontSize = 12, richText = true}, GUILayout.Height(12));
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
        }

        private async Task<LiteDatabase> AsyncConnect(ConnectionString connectionString) { return await Task.Run(() => new LiteDatabase(connectionString)); }

        private void PopulateRecentList()
        {
            var recents = SettingManager.Settings.RecentConnectionStrings;
        }

        #endregion


        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
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