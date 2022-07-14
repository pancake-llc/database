using System;
using UnityEngine;

namespace Pancake.Database
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.GenericParameter | AttributeTargets.Property)]
    public class AssetDropdownAttribute : PropertyAttribute
    {
        public Type Type { get; private set; }
        public AssetDropdownAttribute(Type type) { Type = type; }
    }
}