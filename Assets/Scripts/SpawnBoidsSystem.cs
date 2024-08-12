using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;

public partial class SpawnBoidsSystem : SystemBase
{
    public BoidSpawn boidSpawn;
    private EntityQuery entityQuery;
    protected override void OnCreate()
    {
        RequireForUpdate<BoidController>();
    }
    protected override void OnUpdate()
    {
        if (boidSpawn==null)
        {
            boidSpawn = GameObject.Find("Canvas").GetComponent<BoidSpawn>();
        }
        else
        {
            if (boidSpawn.spawn)
            {
                BoidController boidController = SystemAPI.GetSingleton<BoidController>();
                for (int i = 0; i < boidSpawn.spawnCount; ++i)
                {
                    Entity boid = EntityManager.Instantiate(boidController.boidPrefab);
                    EntityManager.SetComponentData(boid, new LocalTransform
                    {
                        Position = new float3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5)),
                        Rotation = UnityEngine.Random.rotation,
                        Scale = .25f
                    });
                    EntityManager.AddComponentData(boid, new URPMaterialPropertyBaseColor
                    {
                        Value = new float4(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f)) }
                    );

                }
                boidSpawn.spawn = false;
            }
            if (boidSpawn.clear)
            {
                entityQuery = GetEntityQuery(typeof(Boid));
                EntityManager.DestroyEntity(entityQuery);
                boidSpawn.clear = false;
            }
        }
       
    }
}