using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BoidSpawn : MonoBehaviour
{
    public bool spawn;
    public int spawnCount;
    public bool clear;
    private void Awake()
    {
        spawn = false;
        spawnCount = 1;
        clear = false;
    }
    public void Spawn(int n)
    {
        spawn = true;
        spawnCount = n;
    }
    public void Clear()
    {
        clear = true;
    }
}
