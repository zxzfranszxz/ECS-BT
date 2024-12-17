using System;
using System.Collections.Generic;
using SD.ECSBT.BehaviourTree.Data;

namespace SD.ECSBT.BehaviourTree
{
    [Serializable]
    public class BlackboardData
    {
        public List<BTVarInfo> vars = new();
    }
}