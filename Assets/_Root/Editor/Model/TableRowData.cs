using System;
using System.Collections.Generic;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    [Serializable]
    public class TableRowData : ScriptableObject
    {
        public Dictionary<string, string> dictData = new Dictionary<string, string>();
    }
}