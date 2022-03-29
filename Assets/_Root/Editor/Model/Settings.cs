using System;
using System.Collections.Generic;
using LiteDB;

namespace Pancake.DatabaseEditor
{
    [Serializable]
    public class Settings
    {
        public ConnectionString LastConnectionStrings { get; set; }
        public List<ConnectionString> RecentConnectionStrings { get; set; }

        public int MaxRecentListItems { get; set; } = 10;
        public bool LoadLastDbOnStartup { get; set; }

        public Settings() { RecentConnectionStrings = new List<ConnectionString>(); }
    }
}