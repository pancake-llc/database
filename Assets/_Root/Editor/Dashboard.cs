using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class Dashboard : EditorWindow
{
    public static Dashboard instance;
    protected static VisualElement groupView;
    protected static VisualElement entityView;
    protected static VisualElement entity;
    protected static VisualElement inspector;
    protected static ToolbarSearchField groupSearch;
    protected static ToolbarSearchField entitySearch;
    protected static ToolbarButton groupBtnNew;
    protected static ToolbarButton groupBtnDel;
    protected static ToolbarButton groupBtnReload;
    protected static ToolbarButton entityBtnNew;
    protected static ToolbarButton entityBtnDel;
    protected static ToolbarButton entityBtnClone;

    [MenuItem("Tools/Pancake/Dashboard")]
    public static void Open()
    {
        if (instance != null)
        {
            FocusWindowIfItsOpen(typeof(Dashboard));
            return;
        }

        instance = GetWindow<Dashboard>();
        instance.titleContent = new GUIContent("Dashboard");
        instance.minSize = new Vector2(800, 200);
        instance.Show();
        instance.FullRebuild();
    }

    private void FullRebuild()
    {
        if (instance == null) return;

        instance.LoadUxml();
        instance.Rebuild(true);
    }

    private void Rebuild(bool fullRebuild = false) { }

    private void LoadUxml()
    {
        instance.rootVisualElement.Clear();
        var visualTree = Resources.Load<VisualTreeAsset>("dashboard");
        visualTree.CloneTree(instance.rootVisualElement);

        groupView = instance.rootVisualElement.Q<VisualElement>("group_view");
        entityView = instance.rootVisualElement.Q<VisualElement>("entity_view");
        entity = instance.rootVisualElement.Q<VisualElement>("entity");
        inspector = instance.rootVisualElement.Q<VisualElement>("inspector");
        groupSearch = instance.rootVisualElement.Q<ToolbarSearchField>("group_search");
        entitySearch = instance.rootVisualElement.Q<ToolbarSearchField>("entity_search");
        groupBtnNew = instance.rootVisualElement.Q<ToolbarButton>("group_btn_new");
        groupBtnDel = instance.rootVisualElement.Q<ToolbarButton>("group_btn_del");
        groupBtnReload = instance.rootVisualElement.Q<ToolbarButton>("group_btn_reload");
        entityBtnNew = instance.rootVisualElement.Q<ToolbarButton>("entity_btn_new");
        entityBtnDel = instance.rootVisualElement.Q<ToolbarButton>("entity_btn_del");
        entityBtnClone = instance.rootVisualElement.Q<ToolbarButton>("entity_btn_clone");

        groupBtnNew.clicked += OnGroupBtnNewClicked;
        groupBtnDel.clicked += OnGroupBtnDelClicked;
        groupBtnReload.clicked += OnGroupBtnReloadClicked;
        entityBtnNew.clicked += OnEntityBtnNewClicked;
        entityBtnDel.clicked += OnEnityBtnDelClicked;
        entityBtnClone.clicked += OnEntityBtnCloneClicked;
    }

    private void OnEntityBtnCloneClicked() { }

    private void OnEnityBtnDelClicked() { }

    private void OnEntityBtnNewClicked() { }

    private void OnGroupBtnReloadClicked() { }

    private void OnGroupBtnDelClicked() { }

    private void OnGroupBtnNewClicked()
    {
        
    }
}