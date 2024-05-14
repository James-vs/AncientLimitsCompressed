using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public enum Grid
    {
        TWOSTORY,
        TWOSTORYL,
        ONESTORY,
        ONESTORYL,
        ONESTORYCANOPY,
        ROCK,
        EMPTY
    }

    [Header("Grid Size & Position")]
    public int maxWidth;
    public int mapLength;
    public int widthOffset;
    public int lengthOffset;
    public bool canReset;

    [Header("GameObjects")]
    public GameObject[] buildings;
    public GameObject rock;
    private Grid[,] grid;
    protected GameObject parentObject;
    //protected List<WalkerObject> Walkers;
    [Header("Information Variables (DO NOT MANUALLY ALTER)")]
    [SerializeField] protected int buildingCount = 0;
    //[SerializeField] protected int spawnCount = 0;
    //[SerializeField] protected int brickValue;
    //[SerializeField] protected bool passManInCenter = false;
    [Header("Item Spawn Variables")]
    public int maxWalkers = 10;
    public float fillPercentage = 0.3f;
    //public float passManInCenterChance = 0.4f;
    //public float firewallSpawnChance = 0.2f;
    public float waitTime = 0.05f;
    [Range(1, 100)]
    public float smoothness;

}
