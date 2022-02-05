using System;
using System.Collections.Generic;
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


            for (int i = 0; i < 20; i++)
            {
                var sword = new BsonDocument
                {
                    ["_id"] = i + 1,
                    ["name"] = "Lightning Fall",
                    ["damage"] = Random.Range(40, 60),
                    ["mana"] = Random.Range(10, 30),
                    ["level"] = Random.Range(0, 5),
                    ["cooldown"] = Random.Range(10, 15),
                    ["position"] = new BsonArray(1, 2, 3),
                    ["price"] = 30000000000,
                    ["dic"] = BsonMapper.Global.ToDocument(new Dictionary<string, int> {{"melle", 30}, {"range", 10}}),
                    ["date_create"] = DateTime.UtcNow.Date,
                    ["date_purchase"] = new BsonArray(DateTime.UtcNow, new DateTime(1996, 4, 25, 7, 30, 0))
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
            
            var result22 = col.FindOne(Query.EQ("_id", 1));
            Debug.Log(result22["date_purchase"]);
            // var d = result22["date_create"].ToString();
            // if (d.Contains("\"$date\":"))
            // {
            //     string realValue = d.Replace("{\"$date\":\"", "");
            //     realValue = realValue.Remove(realValue.Length - 2, 2);
            //     Debug.Log(realValue);
            // }
        }
    }
}