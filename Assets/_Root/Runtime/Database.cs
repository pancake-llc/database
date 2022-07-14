using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Database
{
    public class Database : ScriptableObject, ISerializationCallbackReceiver
    {
        public const string GLOBAL_DATABASE_NAME = "GlobalDatabase";
        public Dictionary<int, DataEntity> data = new Dictionary<int, DataEntity>();

        [SerializeField] private List<int> keys = new List<int>();
        [SerializeField] private List<DataEntity> values = new List<DataEntity>();
        [SerializeField] private List<DatabaseStaticGroup> staticGroups = new List<DatabaseStaticGroup>();

        private string _uniqueId;
        
        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { }

        #endregion
    }
}