using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BoidControllerAuthoring : MonoBehaviour
{
    public int numSpawn;
    public int obstacleAvoidRadius;
    public int numRaycastPoints;
    public GameObject boidPrefab;
    private class Baker : Baker<BoidControllerAuthoring>
    {
        public override void Bake(BoidControllerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new BoidController
            {
                numSpawn = authoring.numSpawn,
                obstacleAvoidRadius = authoring.obstacleAvoidRadius,
                numRaycastPoints = authoring.numRaycastPoints,
                boidPrefab = GetEntity(authoring.boidPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct BoidController : IComponentData
{
    public int numSpawn;
    public int obstacleAvoidRadius;
    public int numRaycastPoints;
    public Entity boidPrefab;
}

