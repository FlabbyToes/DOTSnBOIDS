using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BoidAuthoring : MonoBehaviour
{
    public float attractRadius;
    public float avoidRadius;
    public float movementSpeed;

    private class Baker : Baker<BoidAuthoring>
    {
        public override void Bake(BoidAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Boid {
                movementSpeed = authoring.movementSpeed,
                attractRadius = authoring.attractRadius,
                avoidRadius = authoring.avoidRadius
            });
        }
    }
}

public struct Boid : IComponentData
{
    public float attractRadius;
    public float avoidRadius;
    public float movementSpeed;
}
