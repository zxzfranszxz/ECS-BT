namespace Game.ECS.AI.BehaviourTree.Instance
{
    public enum ActiveNodeState : byte
    {
        None = 0,
        Running = 1,
        Success = 2,
        Failure = 3
    }
}