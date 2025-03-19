using System;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.Nodes.Attributes
{
    public class NodeHandlerAttribute : Attribute
    {
        public readonly ulong StableTypeHash;
        
        public NodeHandlerAttribute(Type handlerType)
        {
            StableTypeHash = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(handlerType)).StableTypeHash;
        }
    }
    
    public class NodeReturnHandlerAttribute : Attribute
    {
        public readonly ulong StableTypeHash;
        
        public NodeReturnHandlerAttribute(Type handlerType)
        {
            StableTypeHash = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(handlerType)).StableTypeHash;
        }
    }
}