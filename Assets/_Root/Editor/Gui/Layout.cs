using System.Collections.Generic;

namespace Snorlax.Database.Editor
{
    public abstract class Layout : View
    {
        protected readonly List<View> childs = new List<View>();

        public override int ChildCount => childs.Count;

        public Layout Add(View view)
        {
            childs.Add(view);
            return this;
        }

        public Layout Add(View view, int index)
        {
            childs.Insert(index, view);
            return this;
        }

        public void Clear() { childs.Clear(); }

        public View this[int i] => childs[i];
    }
}