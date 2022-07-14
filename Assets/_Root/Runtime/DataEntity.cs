using System;
using UnityEngine;

namespace Pancake.Database
{
    public abstract class DataEntity : ScriptableObject
    {
        [SerializeField] private int id;
        [SerializeField] private string title;
        [SerializeField, TextArea] private string description;

        public string Title { get => title; set => title = value; }
        public string Description { get => description; set => description = value; }
        public int ID { get => id; set => id = value; }

        public Sprite Icon => GetIconInternal();

        protected virtual Sprite GetIconInternal()
        {
            return null;
        }

        protected virtual void Reset()
        {
            Title = $"UNASSIGNED_{Ulid.NewUlid()}";
            Description = "";
        }
    }
}