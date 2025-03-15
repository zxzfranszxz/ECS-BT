using System;

namespace SD.ECSBT.BehaviourTree.Data
{
    [Serializable]
    public class BlackboardVarLink
    {
        public string id;
        public string varId;

        public BlackboardVarLink Clone()
        {
            return (BlackboardVarLink)MemberwiseClone();
        }
    }
}