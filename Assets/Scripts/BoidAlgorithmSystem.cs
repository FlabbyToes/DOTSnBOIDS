using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct BoidAlgorithmSystem : ISystem
{
    private EntityQuery entityQuery;
    private NativeList<float3> points;
    private BoidController controller;
    private float castRadius;
    private bool once;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Boid>();
        state.RequireForUpdate<BoidController>();
        entityQuery = state.GetEntityQuery(typeof(LocalTransform),typeof(Boid));
        once = true;

        //dont plan on adding any boids during runtime

        
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        controller = SystemAPI.GetSingleton<BoidController>();
        //unecessary but whatever
        if (once)
        {
            castRadius = controller.obstacleAvoidRadius;
            points = new NativeList<float3>(controller.numRaycastPoints + 1, Allocator.Persistent);
            points.Add(Vector3.forward * castRadius);
            for (int i = 0; i < controller.numRaycastPoints; ++i)
            {
                float ind = i + .5f;
                float phi = Mathf.Acos(1 - 2 * ind / controller.numRaycastPoints);
                float theta = Mathf.PI * (1 + Mathf.Pow(5, 0.5f)) * ind;
                points.Add(castRadius * new Vector3(Mathf.Cos(theta) * Mathf.Sin(phi), Mathf.Sin(theta) * Mathf.Sin(phi), Mathf.Cos(phi)));
            }
            once = false;
        }




        NativeArray<LocalTransform> entityQueryResults = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        BoidAlgorithmJob boidAlgorithmJob = new BoidAlgorithmJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            birdArray = entityQueryResults,
            rayPoints = points,
            physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>()
        };
        boidAlgorithmJob.ScheduleParallel();
        entityQueryResults.Dispose(state.Dependency);
    }
    [BurstCompile]
    public partial struct BoidAlgorithmJob : IJobEntity
    {
        [ReadOnly]
        public NativeArray<LocalTransform> birdArray;
        public float deltaTime;
        [ReadOnly]
        public NativeList<float3> rayPoints;
        [ReadOnly]
        public PhysicsWorldSingleton physicsWorld;

        public void Execute(ref LocalTransform transform, in Boid boid)
        {
            int attractCount = 0;
            int avoidCount = 0;
            float3 avgHeading = float3.zero;
            float3 avgPos = float3.zero;
            float3 avgClose = float3.zero;

            for (int i = 0; i < birdArray.Length; ++i)
            {
                if (math.length(transform.Position - birdArray[i].Position) <=boid.attractRadius)
                {
                    if (math.length(transform.Position - birdArray[i].Position) <= boid.avoidRadius)
                    {
                        avgClose += birdArray[i].Position;
                        ++avoidCount;
                    }
                    avgPos += birdArray[i].Position;
                    avgHeading += birdArray[i].Forward();
                    ++attractCount;
                    
                }

            }
            if (attractCount > 1)
            {
                avgHeading /= attractCount;
                avgPos /= attractCount;
                transform.Rotation = Quaternion.Lerp(transform.Rotation, Quaternion.LookRotation(avgHeading), 0.01f);
                transform.Rotation = Quaternion.Lerp(transform.Rotation, Quaternion.LookRotation(avgPos - transform.Position), 0.01f);
            }
            if (avoidCount > 1)
            {
                avgClose /= avoidCount;
                transform.Rotation = Quaternion.Lerp(transform.Rotation, Quaternion.LookRotation(-(avgClose - transform.Position)), 0.03f);
            }
            //obstacle avoid
            int ind = 0;
            RaycastInput ray = new RaycastInput
            {
                Start = transform.Position,
                End = transform.Position + math.mul(transform.Rotation,rayPoints[ind]),
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u<<3,
                    GroupIndex = 0
                }
            };
            int max = rayPoints.Length;
            while (physicsWorld.CastRay(ray)&&ind < (max*.85)-1)//85% vision
            {
                ++ind;
                ray = new RaycastInput
                {
                    Start = transform.Position,
                    End = transform.Position + math.mul(transform.Rotation,rayPoints[ind]),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << 3,
                        GroupIndex = 0
                    }
                };
            }
            transform.Rotation = Quaternion.LookRotation(math.mul(transform.Rotation, rayPoints[ind]));
            

        }
    }
}
