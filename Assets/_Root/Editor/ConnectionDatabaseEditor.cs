using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LiteDB;
using Snorlax.Editor;
using UnityEditor;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public class ConnectionDatabaseEditor : EditorWindow
    {
        private const long MB = 1024 * 1024;

        //private
        private ConnectionString _connectionString;
        private ConnectionType _connectionType;
        private string _fileName;
        private bool _isReadOnly;
        private string _password;
        private int _initilizeSize;
        private int _cacheSelectedMode;
        private List<string> _sort;
        private List<string> _culture;
        private string _cacheSort = "";
        private string _cacheCulture = "";

        public void Initialize()
        {
            _sort = new List<string> {""};
            _sort.AddRange(Enum.GetNames(typeof(CompareOptions)));

            _culture = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Select(x => x.LCID)
                .Distinct()
                .Where(x => x != 4096)
                .Select(x => CultureInfo.GetCultureInfo(x).Name)
                .ToList();
        }

        private void Connect()
        {
            _connectionString = new ConnectionString
            {
                Connection = _connectionType,
                Filename = _fileName,
                ReadOnly = _isReadOnly,
                Upgrade = false,
                Password = _password,
                InitialSize = _initilizeSize * MB,
                Collation = new Collation($"{_cacheCulture}/{_cacheSort}")
            };

            SettingManager.Settings.LastConnectionStrings = _connectionString;
            SettingManager.AddToRecentList(_connectionString);
        }

        private void OnGUI()
        {
            UtilEditor.MiniBoxedSection("Connection Mode",
                () =>
                {
                    EditorGUILayout.BeginHorizontal();
                    string[] options = {"  Direct           ", "  Shared"};
                    _cacheSelectedMode = GUILayout.SelectionGrid(_cacheSelectedMode, options, 2, EditorStyles.radioButton);
                    _connectionType = (ConnectionType) _cacheSelectedMode;

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);
                });

            EditorGUILayout.Space(16);

            UtilEditor.MiniBoxedSection("Filename",
                () =>
                {
                    EditorGUILayout.BeginHorizontal();
                    _fileName = GUILayout.TextField(_fileName);
                    ConnectionDatabaseEditorStatic.PickFilePath(ref _fileName, style: EditorStyles.colorField);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);
                });

            EditorGUILayout.Space(16);

            UtilEditor.MiniBoxedSection("Parameters",
                () =>
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Password:", EditorStyles.label, GUILayout.Width(120));
                    _password = EditorGUILayout.PasswordField(_password);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Initial Size (mb):", EditorStyles.label, GUILayout.Width(120));
                    _initilizeSize = EditorGUILayout.IntField(_initilizeSize, EditorStyles.numberField, GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Collation:", EditorStyles.label, GUILayout.Width(120));

                    void CultureMenuFunc(object culture) { _cacheCulture = (string) culture; }

                    void SortMenuFunc(object sort) { _cacheSort = (string) sort; }

                    var cultureMenu = new GenericMenu();
                    foreach (string culture in _culture)
                    {
                        cultureMenu.AddItem(new GUIContent(culture), culture.Equals(_cacheCulture), CultureMenuFunc, culture);
                    }

                    var sortMenu = new GenericMenu();
                    foreach (string sort in _sort)
                    {
                        sortMenu.AddItem(new GUIContent(sort), sort.Equals(_cacheSort), SortMenuFunc, sort);
                    }

                    if (EditorGUILayout.DropdownButton(new GUIContent(_cacheCulture), FocusType.Keyboard, EditorStyles.popup, GUILayout.Width(100)))
                    {
                        cultureMenu.ShowAsContext();
                    }

                    GUILayout.Label("    /", EditorStyles.label, GUILayout.Width(30));

                    if (EditorGUILayout.DropdownButton(new GUIContent(_cacheSort), FocusType.Keyboard, EditorStyles.popup, GUILayout.Width(100)))
                    {
                        sortMenu.ShowAsContext();
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);

                    _isReadOnly = EditorGUILayout.Toggle("Read Only:", _isReadOnly);
                    EditorGUILayout.Space(4);
                });

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Connect", EditorStyles.miniButton, GUILayout.Width(85), GUILayout.MinHeight(85)))
            {
                Connect();
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.EndHorizontal();
        }
    }

    public static class ConnectionDatabaseEditorStatic
    {
        public static void Show()
        {
            var window = EditorWindow.GetWindow<ConnectionDatabaseEditor>("Connection Manager", true);
            window.minSize = new Vector2(550, 350);
            if (window != null)
            {
                window.Initialize();
                window.Show(true);
            }
        }

        /// <summary>
        /// Show panel to pickup file
        /// </summary>
        /// <param name="pathResult"></param>
        /// <param name="keySave"></param>
        /// <param name="style"></param>
        public static void PickFilePath(ref string pathResult, string keySave = "", GUIStyle style = null)
        {
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button(new GUIContent("", "Select File"), style, GUILayout.Width(18), GUILayout.Height(18)))
            {
                var path = EditorUtility.OpenFilePanel("Select file", pathResult, ".db");
                if (!string.IsNullOrEmpty(path))
                {
                    pathResult = path;
                    if (!string.IsNullOrEmpty(keySave)) EditorPrefs.SetString(keySave, pathResult);
                }

                GUI.FocusControl(null);
            }

            GUI.backgroundColor = Color.white;
        }
    }
}