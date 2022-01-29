using Snorlax.Common;

namespace Snorlax.Database.Editor
{
    public abstract class BaseObject : IIdentifier
    {
        private UId _identifier;

        public UId Id => _identifier;

        protected BaseObject(UId identifier) { _identifier = identifier; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Id == ((BaseObject) obj).Id;
        }

        public override int GetHashCode() { return Id.GetHashCode(); }

        public override string ToString() { return " Id:" + Id; }
    }
}