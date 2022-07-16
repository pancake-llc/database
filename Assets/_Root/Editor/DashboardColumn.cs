using UnityEngine.UIElements;

namespace Pancake.Editor
{
    public abstract class DashboardColumn : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<VisualElement, UxmlTraits> { }
        public abstract void Rebuild();
    }
}