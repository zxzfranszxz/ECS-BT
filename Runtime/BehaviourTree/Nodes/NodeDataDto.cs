using System;
using System.Collections.Generic;
using SD.ECSBT.BehaviourTree.Data;
using SD.ECSBT.BehaviourTree.Nodes.Attributes;
using UnityEngine;

namespace SD.ECSBT.BehaviourTree.Nodes
{
    [Serializable]
    public class NodeDataDto
    {
        public string name;
        public string description;
        [NodeTypeProperty]
        public string typeReference;
        public string guid;
        public string parentGuid;

        public List<string> childrenGuids = new();
        public List<BTVar> vars = new();
        public List<BlackboardVarLink> blackboardVars = new();

        // editor
        public Vector2 position;

        public Type Type => Type.GetType(typeReference);

        public Guid Guid => Guid.Parse(guid);

        public Guid ParentGuid => Guid.TryParse(parentGuid, out var pGuid) ? pGuid : Guid.Empty;
    }
}