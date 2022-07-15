using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.Database
{
    public class Database : ScriptableObject, ISerializationCallbackReceiver
    {
        public const string GLOBAL_DATABASE_NAME = "GlobalDatabase";
        public Dictionary<string, DataEntity> data = new Dictionary<string, DataEntity>();

        [SerializeField] private List<string> keys = new List<string>();
        [SerializeField] private List<DataEntity> values = new List<DataEntity>();
        [SerializeField] private List<DatabaseStaticGroup> staticGroups = new List<DatabaseStaticGroup>();
        
        public List<DatabaseStaticGroup> StaticGroups => staticGroups;

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var entity in data)
            {
                keys.Add(entity.Key);
                values.Add(entity.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            data = new Dictionary<string, DataEntity>();
            for (int i = 0; i < keys.Count; i++)
            {
                data.Add(keys[i], values[i]);
            }
        }

        #endregion

        public DataEntity Query(string id) { return data.ContainsKey(id) ? data[id] : null; }

        public List<T> Query<T>() { return values.OfType<T>().ToList().Map(_ => (T) Convert.ChangeType(_, typeof(T))); }

        public virtual void Add(DataEntity entity, bool forceId = true)
        {
            string id = forceId ? Ulid.NewUlid().ToString() : entity.ID;
            if (data.ContainsKey(id)) return;

            entity.ID = id;
            data.Add(id, entity);
        }

        public virtual void Remove(DataEntity entity)
        {
            if (data.ContainsKey(entity.ID)) data.Remove(entity.ID);
        }

        public virtual void Remove(string id)
        {
            if (data.ContainsKey(id)) data.Remove(id);
        }

        public virtual DatabaseStaticGroup GetStaticGroup<T>() where T : DataEntity { return StaticGroups.Single(_ => _.Type == typeof(T)); }

        public virtual DatabaseStaticGroup GetStaticGroup(Type type) { return StaticGroups.Single(_ => _.Type == type); }

        public virtual void SetStaticGroup(DatabaseStaticGroup group)
        {
            foreach (var staticGroup in StaticGroups.Filter(_=>_.Type == group.Type))
            {
                StaticGroups.Remove(group);
            }
            staticGroups.Add(group);
        }
        
        public virtual void Release()
        {
            data.Clear();
            keys.Clear();
            values.Clear();
        }

        public virtual void ReleaseStaticGroup()
        {
            StaticGroups.Clear();
        }

        public virtual int Count => data.Count;
        
    }
}