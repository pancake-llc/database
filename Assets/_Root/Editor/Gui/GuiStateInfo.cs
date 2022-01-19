using System;
using Snorlax.Common;

namespace Snorlax.Database.Editor
{
    [Serializable]
    public class GuiStateInfo
    {
        public UId TakeId(string id) { return new UId(id); }
    }

    public interface IStateable
    {
        void To(GuiStateInfo state);
        void From(GuiStateInfo state);
    }
}