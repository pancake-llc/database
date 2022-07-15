using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Database
{
    [Serializable]
    public class DatabaseStaticGroup : IDataGroup
    {
        [SerializeField] private List<DataEntity> datas = new List<DataEntity>();
        [SerializeField, HideInInspector] private string typeName = typeof(DataEntity).AssemblyQualifiedName;

        public string Title { get => Type.Name; set => Debug.Log($"Tried to set a static group Title to {value}. But, we can't set the Title on static groups."); }
        public Type Type { get => Type.GetType(typeName); set => typeName = value.AssemblyQualifiedName; }
        public List<DataEntity> Content { get => datas; set => datas = value; }

        public void Add(DataEntity entity) { Content.Add(entity); }

        public void Remove(string key)
        {
            for (int i = 0; i < Content.Count; i++)
            {
                if (Content[i].ID == key) Content.RemoveAt(i);
            }
        }

        public void CleanUp() { Content.RemoveAll(_ => _ == null); }

        public DatabaseStaticGroup(Type type) { Type = type; }
    }
}