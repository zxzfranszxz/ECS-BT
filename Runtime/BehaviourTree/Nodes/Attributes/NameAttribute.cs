using System;

namespace SD.ECSBT.BehaviourTree.Nodes.Attributes
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface)]
    public class NameAttribute : Attribute
    {
        public string Name { get; }
        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}