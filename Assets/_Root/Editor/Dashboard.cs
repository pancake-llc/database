using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class Dashboard : EditorWindow
{
    [MenuItem("Window/UI Toolkit/Dashboard")]
    public static void ShowExample()
    {
        Dashboard wnd = GetWindow<Dashboard>();
        wnd.titleContent = new GUIContent("Dashboard");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Root/Editor/Resources/dashboard.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);
    }
}