using System;
using System.Collections.Generic;
using System.Threading;
using LiteDB;

namespace Pancake.DatabaseEditor
{
    public class TaskData
    {
        private const int RESULT_LIMIT = 1000;

        public string NameTableSelected { get; set; } = "";
        public Tuple<int, int> Position { get; set; }

        public const string SQL_QUERY = "SELECT $ FROM {0}";
        public string Collection { get; set; } = "";
        public List<BsonValue> Result { get; set; }
        public BsonDocument Parameters { get; set; } = new BsonDocument();

        public bool LimitExceeded { get; set; }
        public bool IsParametersLoaded = false;

        public void ReadResult(IBsonDataReader reader)
        {
            Result = new List<BsonValue>();
            LimitExceeded = false;
            Collection = reader.Collection;

            var index = 0;

            while (reader.Read())
            {
                if (index++ >= RESULT_LIMIT)
                {
                    LimitExceeded = true;
                    break;
                }

                Result.Add(reader.Current);
            }
        }
    }
}