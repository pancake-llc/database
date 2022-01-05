using System;
using LiteDB;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snorlax.Database
{
    public class Demo : MonoBehaviour
    {
        private void Start()
        {
            using var db = DatabaseBridge.Open("ItemData");
            var col = db.GetCollection("skills");


            for (int i = 0; i < 10; i++)
            {
                var sword = new BsonDocument
                {
                    ["_id"] = i + 1,
                    ["name"] = "Lightning Fall",
                    ["damage"] = Random.Range(40, 60),
                    ["mana"] = Random.Range(10, 30),
                    ["level"] = Random.Range(0, 5),
                    ["cooldown"] = Random.Range(10, 15)
                };

                var result = col.FindOne(Query.EQ("_id", i + 1));
                if (result == null) col.Insert(sword);
            }


            var col2 = db.GetCollection("items");


            for (int i = 0; i < 10; i++)
            {
                var sword = new BsonDocument { ["_id"] = i + 1, ["name"] = "Sword", ["damage"] = Random.Range(5, 30), ["level"] = Random.Range(0, 5) };

                var result = col2.FindOne(Query.EQ("_id", i + 1));
                if (result == null) col2.Insert(sword);
            }
        }
    }
}