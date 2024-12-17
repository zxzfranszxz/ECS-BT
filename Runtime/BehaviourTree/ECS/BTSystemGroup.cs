using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class BTSystemGroup : ComponentSystemGroup
    {
    }
    
    [UpdateInGroup(typeof(BTSystemGroup))]
    public partial class BTLogicSystemGroup : ComponentSystemGroup
    {
    }
    
    [UpdateInGroup(typeof(BTSystemGroup))]
    [UpdateBefore(typeof(BTLogicSystemGroup))]
    public partial class BTActionsSystemGroup : ComponentSystemGroup
    {
    }
    
    [UpdateInGroup(typeof(BTSystemGroup))]
    [UpdateBefore(typeof(BTLogicSystemGroup))]
    public partial class BTServiceSystemGroup : ComponentSystemGroup
    {
    }
    
}