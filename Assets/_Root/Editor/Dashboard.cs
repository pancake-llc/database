using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Pancake.Database
{
    public class Dashboard : EditorWindow
    {
        private const string UXML_ASSET_NAME = "dashboard";

        public static Entity CurrentSelectedEntity
        {
            get
            {
                if (instance._currentSelectedEntity != null)
                {
                    return instance._currentSelectedEntity;
                }

                string currentGuid = EditorSettings.Get(EditorSettings.ESettingKey.CurrentEntityGuid);
                string currentPath = AssetDatabase.GUIDToAssetPath(currentGuid);
                Entity asset = AssetDatabase.LoadAssetAtPath<Entity>(currentPath);
                instance._currentSelectedEntity = asset;
                return instance._currentSelectedEntity;
            }
            set
            {
                string currentPath = AssetDatabase.GetAssetPath(value);
                GUID currentGuid = AssetDatabase.GUIDFromAssetPath(currentPath);
                EditorSettings.Set(EditorSettings.ESettingKey.CurrentEntityGuid, currentGuid.ToString());
                instance._currentSelectedEntity = value;
            }
        }

        private Entity _currentSelectedEntity;

        public static IGroup CurrentSelectedGroup
        {
            get
            {
                if (instance._currentGroupSelected != null)
                {
                    return instance._currentGroupSelected;
                }

                string currentName = EditorSettings.Get(EditorSettings.ESettingKey.CurrentGroupName);
                GroupFoldableButton button = groupColumn.Q<GroupFoldableButton>(currentName);
                if (button == null)
                {
                    // Debug.Log($"Failed to find a group button '{currentName}'.");
                    return null;
                }

                IGroup asset = button.Group;
                if (asset == null)
                {
                    Debug.Log($"Failed to find group asset '{currentName}'.");
                }

                instance._currentGroupSelected = asset;
                return instance._currentGroupSelected;
            }
            set
            {
                string title = value == null ? "NULL GROUP" : value.Title;
                EditorSettings.Set(EditorSettings.ESettingKey.CurrentGroupName, title);
                instance._currentGroupSelected = value;
            }
        }

        private IGroup _currentGroupSelected;

        // toolbar
        protected static Historizer historizer;
        public static ToolbarSearchField groupSearch;
        public static ToolbarSearchField entitySearch;
        public static bool SearchTypeIsDirty => groupSearch != null && groupSearch.value != groupSearchCache;
        public static bool SearchAssetIsDirty => entitySearch != null && entitySearch.value != enitySearchCache;
        private static string enitySearchCache;
        private static string groupSearchCache;

        // columns
        public static GroupColumn groupColumn;
        public static EntityColumn enityColumn;
        public static InspectorColumn inspectorColumn;

        // action callbacks ////////////////

        // major changers
        public static Action onCurrentAssetChanged;
        public static Action onCurrentGroupChanged;

        // searches
        public static Action onSearchAssets;
        public static Action onSearchGroups;

        // assets
        public static Action onDeleteAssetStart;
        public static Action onDeleteAssetComplete;
        public static Action onCreateNewAssetStart;
        public static Action onCreateNewAssetComplete;
        public static Action onCloneAssetStart;
        public static Action onCloneAssetComplete;

        // groups
        public static Action onCreateGroupStart;
        public static Action onCreateGroupComplete;

        // wrappers for views
        protected static VisualElement groupView;
        protected static VisualElement entity;
        protected static VisualElement entityView;
        protected static VisualElement inspectorView;

        private static ToolbarButton entityBtnNew;
        private static ToolbarButton entityBtnDel;
        private static ToolbarButton entityBtnClone;
        private static ToolbarButton entityBtnRemoveFromGroup;
        private static ToolbarButton groupBtnNew;
        private static ToolbarButton groupBtnDel;

        public static Dashboard instance;

        private static readonly StyleColor ButtonInactive = new StyleColor(Color.gray);
        private static readonly StyleColor ButtonActive = new StyleColor(Color.white);

        [MenuItem("Tools/Pancake/DB/Dashboard %#d", priority = 0)]
        public static void Open()
        {
            //Debug.Log("Open()");
            if (instance != null)
            {
                FocusWindowIfItsOpen(typeof(Dashboard));
                return;
            }

            instance = GetWindow<Dashboard>();
            instance.titleContent.text = "Data Dashboard";
            instance.minSize = new Vector2(850, 200);
            instance.Show();
            instance.RebuildFull();
        }

        public void OnEnable() { instance = this; }

        public void Update()
        {
            if (SearchAssetIsDirty)
            {
                enitySearchCache = entitySearch.value;
                EditorSettings.Set(EditorSettings.ESettingKey.SearchEntities, enitySearchCache);
                onSearchAssets?.Invoke();
            }

            if (SearchTypeIsDirty)
            {
                groupSearchCache = groupSearch.value;
                EditorSettings.Set(EditorSettings.ESettingKey.SearchGroups, groupSearchCache);
                onSearchGroups?.Invoke();
            }
        }

        [InitializeOnLoadMethod]
        private static void OnRecompile() { }

        private void LoadUxmlTemplate()
        {
            instance.rootVisualElement.Clear();

            // load uxml and elements
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>(UXML_ASSET_NAME);
            visualTree.CloneTree(rootVisualElement);

            // find important parts and reference them
            groupView = rootVisualElement.Q<VisualElement>("group_view");
            entityView = rootVisualElement.Q<VisualElement>("entity_view");
            entity = rootVisualElement.Q<VisualElement>("entity");
            inspectorView = rootVisualElement.Q<VisualElement>("inspector_view");
            groupSearch = rootVisualElement.Q<ToolbarSearchField>("group_search");
            entitySearch = rootVisualElement.Q<ToolbarSearchField>("entity_search");

            historizer = new Historizer();
            rootVisualElement.Q<VisualElement>("statusbar").Add(historizer);

            // init group column buttons
            groupBtnNew = rootVisualElement.Q<ToolbarButton>("group_btn_new");
            groupBtnDel = rootVisualElement.Q<ToolbarButton>("group_btn_del");
            rootVisualElement.Q<ToolbarButton>("group_btn_reload").clicked += CallbackButtonRefresh;

            groupBtnNew.clicked += CreateBtnNewDataGroupBtnCallback;
            groupBtnDel.clicked += DeleteSelectedDataGroupBtn;

            // init Asset Column Buttons
            entityBtnNew = entity.Q<ToolbarButton>("entity_btn_new");
            entityBtnDel = entity.Q<ToolbarButton>("entity_btn_del");
            entityBtnClone = entity.Q<ToolbarButton>("entity_btn_clone");
            entityBtnRemoveFromGroup = entity.Q<ToolbarButton>("entity_btn_remove_from_group");

            entityBtnNew.clicked += CreateNewAssetCallback;
            entityBtnDel.clicked += DeleteSelectedAsset;
            entityBtnClone.clicked += CloneSelectedAsset;
            entityBtnRemoveFromGroup.clicked += RemoveAssetFromGroup;


            groupView.Add(groupColumn);
            entityView.Add(enityColumn);
            inspectorView.Add(inspectorColumn);

            // init split pane draggers
            // BUG - basically we have to do this because there is no proper/defined initialization for the drag anchor position.
            var mainSplit = rootVisualElement.Q<SplitView>("main_view");
            var columnSplit = rootVisualElement.Q<SplitView>("content_view");
            mainSplit.fixedPaneInitialDimension = 549;
            columnSplit.fixedPaneInitialDimension = 250;
        }

        public void RebuildFull()
        {
            //Debug.Log($"Start RebuildFull() - Dashboard is {(Instance == null ? "null" : "valid")}");
            if (instance == null) return;

            instance.LoadUxmlTemplate();
            Rebuild(true);
        }

        public void Rebuild(bool fullRebuild = false)
        {
            //Debug.Log($"... Rebuild()");
            // search data
            groupSearch.SetValueWithoutNotify(EditorSettings.Get(EditorSettings.ESettingKey.SearchGroups));
            entitySearch.SetValueWithoutNotify(EditorSettings.Get(EditorSettings.ESettingKey.SearchEntities));
            groupSearchCache = groupSearch.value;
            enitySearchCache = entitySearch.value;

            // rebuild
            RebuildGroupColumn(fullRebuild);
            RebuildAssetColumn(fullRebuild);
            RebuildInspectorColumn(fullRebuild);
            SetCurrentGroup(CurrentSelectedGroup);

            onCreateNewAssetComplete += OnCreatedNewAsset;
        }

        private void RebuildGroupColumn(bool fullRebuild = false)
        {
            if (fullRebuild || groupColumn == null)
            {
                groupView.Clear();
                groupColumn = new FilterColumn();
                groupView.Add(groupColumn);
            }

            groupColumn.Rebuild();
        }

        private void RebuildAssetColumn(bool fullRebuild = false)
        {
            if (fullRebuild || enityColumn == null)
            {
                entityView.Clear();
                enityColumn = new EntityColumn();
                entityView.Add(enityColumn);
            }

            enityColumn.Rebuild();
        }

        private void RebuildInspectorColumn(bool fullRebuild = false)
        {
            if (fullRebuild || inspectorColumn == null)
            {
                inspectorColumn?.RemoveFromHierarchy();
                inspectorColumn = new InspectorColumn();
                inspectorView.Add(inspectorColumn);
            }

            inspectorColumn.Rebuild();
        }

        public static void SetCurrentGroup(IGroup group, bool isCustom = false)
        {
            if (group == null) return;

            entityBtnRemoveFromGroup.SetEnabled(isCustom);
            entityBtnRemoveFromGroup.style.unityBackgroundImageTintColor = isCustom ? ButtonActive : ButtonInactive;

            CurrentSelectedGroup = group;
            groupColumn.SelectButtonByTitle(group.Title);
            entitySearch.value = string.Empty;
            onCurrentGroupChanged?.Invoke();
        }

        public static void SetCurrentInspectorAsset(Entity asset)
        {
            CurrentSelectedEntity = asset;
            onCurrentAssetChanged?.Invoke();
        }

        public static void InspectAssetRemote(Object asset, Type t)
        {
            if (asset == null && t == null) return;
            if (t == null) return;

            if (instance == null) Open();
            instance.Focus();
            entitySearch.SetValueWithoutNotify(string.Empty);

            VisualElement button = groupView.Q<VisualElement>(t.Name);
            IGroupButton buttonInterface = (IGroupButton) button;
            if (buttonInterface != null)
            {
                buttonInterface.SetAsCurrent();
                groupColumn.ScrollTo(button);
            }

            if (asset != null) enityColumn.Pick((Entity) asset);
        }

        /// <summary>
        /// The Dashboard button calls this to create a new asset in the current group.
        /// </summary>
        private static void CreateNewAssetCallback()
        {
            if (CurrentSelectedGroup.Type.IsAbstract)
            {
                bool confirm = UnityEditor.EditorUtility.DisplayDialog("Group Error",
                    "Selected Class is abstract! We can't create a new asset in abstract class groups. Choose a valid class and create a new Data Asset, then you can store it in a Custom Group.",
                    "Ok");
                if (confirm) return;
            }

            CreateNewAsset();
        }

        /// <summary>
        /// Create a new asset with the current group Type.
        /// </summary>
        /// <returns></returns>
        public static Entity CreateNewAsset()
        {
            onCreateNewAssetStart?.Invoke();

            Entity newAsset = enityColumn.NewAsset(CurrentSelectedGroup.Type);

            onCreateNewAssetComplete?.Invoke();
            return newAsset;
        }

        /// <summary>
        /// Create a new asset with a specific Type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Entity CreateNewAsset(Type t)
        {
            Debug.Log($"Create new asset with specific Type: {t.Name}");
            onCreateNewAssetStart?.Invoke();
            Entity newAsset = enityColumn.NewAsset(t);
            onCreateNewAssetComplete?.Invoke();
            return newAsset;
        }

        public static void CloneSelectedAsset()
        {
            onCloneAssetStart?.Invoke();
            enityColumn.CloneSelection();
            onCloneAssetComplete?.Invoke();
        }

        public static void DeleteSelectedAsset()
        {
            onDeleteAssetStart?.Invoke();
            enityColumn.DeleteSelection();
            onDeleteAssetComplete?.Invoke();
        }

        private static void OnCreatedNewAsset() { Builder.Reload(); }

        public static void RemoveAssetFromGroup()
        {
            CurrentSelectedGroup.Remove(CurrentSelectedEntity.ID);
            enityColumn.Rebuild();
        }

        public static void CreateBtnNewDataGroupBtnCallback() { CreateNewDataGroup(); }

        public static void DeleteSelectedDataGroupBtn()
        {
            if (CurrentSelectedGroup == null) return;
            CustomGroup customGroup = (CustomGroup) CurrentSelectedGroup;
            if (customGroup == null) return;

            bool confirm = UnityEditor.EditorUtility.DisplayDialog("Delete Custom Group",
                $"Are you sure you want to permanently delete '{CurrentSelectedGroup.Title}'?",
                "Delete",
                "Abort");
            if (!confirm) return;

            InspectAssetRemote(null, typeof(object));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(customGroup));
            CurrentSelectedGroup = null;
            instance.Rebuild();
        }

        public static CustomGroup CreateNewDataGroup()
        {
            onCreateGroupStart?.Invoke();
            CustomGroup result = (CustomGroup) enityColumn.NewAsset(typeof(CustomGroup));
            groupColumn.Rebuild();
            InspectAssetRemote(result, typeof(CustomGroup));
            onCreateGroupComplete?.Invoke();
            return null;
        }

        public void CallbackButtonRefresh() { RebuildFull(); }
    }
}