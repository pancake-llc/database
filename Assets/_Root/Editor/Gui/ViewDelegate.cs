namespace Snorlax.Database.Editor
{
    public class ViewDelegate : View
    {
        public View @delegate;
        public override void GUI() { @delegate?.GUI(); }

        public override void Dispose()
        {
            base.Dispose();
            @delegate?.Dispose();
        }
    }
}