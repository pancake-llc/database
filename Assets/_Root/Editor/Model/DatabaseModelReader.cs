using System;
using System.Collections.Generic;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    [Serializable]
    public class DatabaseModelReader : ScriptableObject
    {
        public Dictionary<string, string> dictData = new Dictionary<string, string>();
    }
}