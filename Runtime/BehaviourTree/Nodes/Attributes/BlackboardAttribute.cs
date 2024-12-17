using System;

namespace SD.ECSBT.BehaviourTree.Nodes.Attributes
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public class BlackboardAttribute : Attribute
    {
        public Type Type { get; }
        public string Name { get; }

        public BlackboardAttribute(string name, Type type)
        {
            Type = type;
            Name = name;
        }
    }
}