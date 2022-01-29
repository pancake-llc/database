using System;
using Snorlax.Common;

namespace Snorlax.Database.Editor
{
    public abstract class DatabaseObject : BaseObject, IName
    {
        private string _name;

        public DatabaseObject(UId identifier)
            : base(identifier)
        {
        }

        public virtual string Name { get => _name; }

        private void SetName(string name)
        {
            string error = ValidateName(name);
            if (error != null) throw new Exception($"Error in name {name} : {error}");

            _name = name;
        }

        public static string ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Name can not be empty";
            // other validate in here
            return null;
        }
    }
}