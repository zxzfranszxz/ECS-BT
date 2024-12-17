using System.Linq;
using SD.ECSBT.BehaviourTree.ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace SD.ECSBT.BehaviourTree.ECS.Authoring
{
    public class BehaviourTreeAuthoring : MonoBehaviour
    {
#if UNITY_EDITOR
        private class BehaviourTreeAuthoringBaker : Baker<BehaviourTreeAuthoring>
        {
            public override void Bake(BehaviourTreeAuthoring authoring)
            {
                // Find all asset GUIDs in the "Assets" folder
                var guids = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(BehaviourTreeSO));

                var btList = guids.Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                    .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<BehaviourTreeSO>).Where(asset => asset)
                    .ToList();

                var entity = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<BTDataSOElement>(entity);
                
                foreach (var treeDataSO in btList)
                {
                    buffer.Add(new BTDataSOElement
                    {
                        BTData = treeDataSO,
                    });
                }
            }
        }
#endif
    }
}