using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Database
{
    public static class DB
    {
        private static Database database;

        public static Database Data
        {
            get
            {
                if (database == null) database = (Database) Resources.Load(Database.GLOBAL_DATABASE_NAME);
                return database;
            }
        }

        /// <summary>
        /// Directly query the database for a specific key. This is the most efficient way to access data.
        /// </summary>
        /// <param name="key">The item ID</param>
        /// <returns>A reference to the <see cref="DataEntity"/>.</returns>
        public static DataEntity Query(string key) { return Data.Query(key); }

        /// <summary>
        /// Slow way to get every item of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of Items you want.</typeparam>
        /// <returns>All of the items that are of the given type.</returns>
        public static List<T> Query<T>() where T : DataEntity { return Data.Query<T>(); }
    }
}