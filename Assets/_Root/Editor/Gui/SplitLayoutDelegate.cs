namespace Snorlax.Database.Editor
{
    public class SplitLayoutDelegate : SplitLayout<ViewDelegate, ViewDelegate>
    {
        public SplitLayoutDelegate()
            : base(new ViewDelegate(), new ViewDelegate())
        {
        }

        public SplitLayoutDelegate(ViewDelegate leftView, ViewDelegate rightView)
            : base(leftView, rightView)
        {
        }
    }
}