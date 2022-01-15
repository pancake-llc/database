using System;

namespace Snorlax.Database.Editor
{
    public abstract class View : IDisposable
    {
        public abstract void GUI();

        public virtual void Reset() { }

        public virtual int ChildCount => 0;

        public virtual void Dispose() { }
    }

    public class DefaultView : View
    {
        private readonly Action _action;

        public DefaultView(Action action) { _action = action; }

        public override void GUI() { _action.Invoke(); }
    }
}