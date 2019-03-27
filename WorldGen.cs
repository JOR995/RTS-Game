using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorldGen : MonoBehaviour
{
    [SerializeField]
    GameObject terrainObject;

    [SerializeField]
    Transform treeParent, rockParent, metalParent, crystalParent, playerBuildingsParent;

    [SerializeField]
    int mapWidth, mapHeight, octaves, seed;

    [SerializeField]
    float noiseScale, persistance, meshHeightMultiplier;

    [SerializeField]
    AnimationCurve meshHeightCurve;

    [SerializeField]
    Vector2 offset;

    public TerrainType[] regions;

    private NavMeshSurface navMeshSurface;


    private void Start()
    {
        mapWidth = 150;
        mapHeight = 150;
        octaves = Random.Range(1, 51);
        seed = Random.Range(0, 2001);
        noiseScale = 35;
        persistance = Random.Range(-4f, 4f);
        meshHeightMultiplier = 20f;
        offset = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));

        //Debug.Log("Generating terrain with following parameters: seed = " + seed + ", persistance = " + persistance
        //    + ", octaves = " + octaves + ", offset = " + offset);

        GenerateMap();
    }

    /// <summary>
    /// Handles assignment of map colours and height modifications using the passed array of generated perlin values
    /// </summary>
    public void GenerateMap()
    {
        //2D array of float values generated within the NoiseGen script and returned here
        float[,] noiseGrid = NoiseGen.GenerateNoise(mapWidth, mapHeight, noiseScale, octaves, persistance, seed, offset);
        Color[] colourMap = new Color[mapWidth * mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseGrid[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();

        mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseGrid, meshHeightMultiplier, meshHeightCurve), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        terrainObject.AddComponent<MeshCollider>();

        //Calls for the terrain to be populated with environmental objects
        MapObjectGeneration.GenerateObjects(terrainObject, treeParent, rockParent, metalParent, crystalParent, playerBuildingsParent, mapWidth, mapWidth);

        navMeshSurface = terrainObject.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();

        NavMeshHit navHit;
        Vector3 engineerSpawn;
        bool engineerSpawned = false;
        GameObject spawnedUnit;


        do
        {
            //Selects a random vector3 position within a sphere centering around the transform position of the base core
            engineerSpawn = Random.insideUnitSphere * 50 + GameObject.Find("BaseCore(Clone)").transform.position;

            //Creates a new ray object using this random position and sets it to be above the terrain
            Ray raycast = new Ray(new Vector3(engineerSpawn.x, 180.0f, engineerSpawn.z), Vector3.down);

            //Casts another collider raycast using the ray object
            if (NavMesh.SamplePosition(engineerSpawn, out navHit, 200f, NavMesh.AllAreas))
            {
                engineerSpawn = navHit.position;

                //Then spawns the engineer unit
                spawnedUnit = Instantiate(Resources.Load("PlayerUnits/Engineer"), engineerSpawn, Quaternion.identity, GameObject.Find("UnitParent").transform) as GameObject;
                spawnedUnit.GetComponent<Unit>().SpawnUnit();
                engineerSpawned = true;
            }
        }
        while (!engineerSpawned);

    }


    void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (octaves < 0) octaves = 0;
    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public float roundedHeight;
        public Color colour;
    }
}
