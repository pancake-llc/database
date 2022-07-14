using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Database
{
    [Serializable]
    public class DatabaseCustomGroup : DataEntity, IDataGroup
    {
        [SerializeField] private List<DataEntity> datas = new List<DataEntity>();
        [SerializeField, HideInInspector] private string typeName = typeof(DataEntity).AssemblyQualifiedName;

        public Type Type { get => Type.GetType(typeName); set => typeName = value.AssemblyQualifiedName; }
        public List<DataEntity> Content { get => datas; set => datas = value; }

        protected override void Reset()
        {
            base.Reset();
            Title = "New Database Custom Group";
            Description = "Used to store a list of custom Data Entity types for easy reference.";
        }

        public void Add(DataEntity entity)
        {
            if (Content.Contains(entity)) return;
            Content.Add(entity);
            EditorHandleDirty();
        }

        public void Remove(int key)
        {
            for (int i = 0; i < Content.Count; i++)
            {
                if (Content[i].ID == key)
                {
                    Content.RemoveAt(i);
                }
            }

            EditorHandleDirty();
        }

        public void CleanUp() { Content.RemoveAll(x => x == null); }

        protected virtual void EditorHandleDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}