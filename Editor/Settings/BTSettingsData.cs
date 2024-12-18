using System;
using UnityEngine;

namespace Editor.SD.ECSBT.Settings
{
    [Serializable]
    public class BTSettingsData : ScriptableObject
    {
        [Header("BT Process System")]
        public string processSystemClassName;
        public string processSystemNamespace;
        public string processSystemClassPath;
        [Space]
        [Header("Nodes Handler")]
        public string nodeHandlerClassName;
        public string nodeHandlerNamespace;
        public string nodeHandlerClassPath;
    }
}