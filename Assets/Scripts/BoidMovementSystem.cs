using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public partial struct BoidMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Boid>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        BoidMovementJob boidMovementJob = new BoidMovementJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        };
        boidMovementJob.Schedule();

    }
    [BurstCompile]
    public partial struct BoidMovementJob : IJobEntity
    {
        public float deltaTime;
        public void Execute(ref LocalTransform localTransform, in Boid boid)
        {
            localTransform.Position += localTransform.Forward()* boid.movementSpeed * deltaTime;
        }
    }
}
