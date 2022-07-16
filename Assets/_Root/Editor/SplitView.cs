using UnityEngine.UIElements;

namespace Pancake.Editor
{
    public class SplitView : TwoPaneSplitView
        {
            public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }
        }
}