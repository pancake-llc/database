using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace Pancake.DatabaseEditor
{
    public static class SettingManager
    {
        private static Settings settings;

        public static Settings Settings { get { return settings ??= new Settings(); } }

        public static void AddToRecentList(ConnectionString connectionString)
        {
            // check duplication
            var connection = Settings.RecentConnectionStrings.FirstOrDefault(cs => cs.Filename == connectionString.Filename);
            if (connection != null)
            {
                // remove the old item
                Settings.RecentConnectionStrings.Remove(connection);
            }

            if (Settings.RecentConnectionStrings.Count + 1 > Settings.MaxRecentListItems)
            {
                // remove last item in the list
                Settings.RecentConnectionStrings.RemoveAt(Settings.RecentConnectionStrings.Count - 1);
            }

            // add new to the top
            Settings.RecentConnectionStrings = new List<ConnectionString>(Settings.RecentConnectionStrings.Prepend(connectionString));
        }
    }
}