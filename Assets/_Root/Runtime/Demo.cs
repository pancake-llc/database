using System;
using LiteDB;
using UnityEngine;

namespace Snorlax.Database
{
    public class Demo : MonoBehaviour
    {
        private void Start()
        {
            using var db = DatabaseBridge.Open("item");
            var col = db.GetCollection("items");

            var sword = new BsonDocument();
            sword["id"] = 1;
            sword["name"] = "Wooden Sword";
            sword["damage"] = "4";
            sword["level"] = 0;

            var result = col.FindOne(Query.EQ("id", 1));
            if (result == null)
            {
                col.Insert(sword);
            }
        }
    }
}