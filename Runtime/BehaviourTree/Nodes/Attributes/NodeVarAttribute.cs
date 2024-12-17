using System;

namespace SD.ECSBT.BehaviourTree.Nodes.Attributes
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public class NodeVarAttribute : Attribute
    {
        public string Id { get; }
        public Type Type { get; }

        public NodeVarAttribute(string id, Type type)
        {
            Id = id;
            Type = type;
        }
    }
}