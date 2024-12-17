using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace SD.ECSBT.BehaviourTree.Data
{
    [Serializable]
    public class BTVar
    {
        public string id;
        public string typeReference;
        public string value;
        
        public Type Type => Type.GetType(typeReference);

        public static IEnumerable<Type> SupportedTypes()
        {
            yield return typeof(bool);
            yield return typeof(int);
            yield return typeof(float);
            yield return typeof(string);
            yield return typeof(Vector2);
            yield return typeof(Vector3);
            yield return typeof(Quaternion);
            yield return typeof(Entity);
            yield return typeof(AbortType);
            yield return typeof(NotifyType);
            yield return typeof(BlackboardConditionType);
        }
    }
}