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

        public void Initialize()
        {
            SceneView.duringSceneGui += DuringSceneGUI;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private void DuringSceneGUI(SceneView sceneView) { }

        private void PlayModeStateChanged(PlayModeStateChange stateChange) { }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Connect", EditorStyles.miniButtonLeft, GUILayout.Width(85)))
            {
            }

            if (GUILayout.Button("Refresh", EditorStyles.miniButtonMid, GUILayout.Width(85)))
            {
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(false), GUILayout.Width(200));

            // todo show db tree

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));

            // todo display editor db

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        #region func

        private void OnButtonClickConnect()
        {
            if (_db == null)
            {
                var connectDatabaseWindow = ConnectionDatabaseEditorStatic.Show(_connectionString ?? new ConnectionString(), Connect);
                
            }
        }

        private void Connect(string connectString) { }

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