using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    private enum EnemyType { Scout, Car }
    private EnemyType enemyType;

    private enum SpawnType { Random, Base, Wave }

    public static EnemySpawner enemySpawnerInstance;

    [SerializeField]
    GameObject spawnSphere, terrainObject;

    [SerializeField]
    Transform[] mapCorners;

    private MeshCollider terrainCollider;
    private Transform enemyPool, buildingParent, enemyBaseParent;
    private GameObject spawnedUnit, waveUnit, coreBuilding, enemyHQ, leadUnit;
    private List<GameObject> buildingList;

    private bool sphereSet, sphereResized, spawningUnits, spawningWave, timerStarted;
    private float sphereScale, spawnRadius, unitSpawnTimer, waveSpawnTimer;
    private int unitCount, unitMax, waveGroupNum, waveGroupCount, waveGroupMax, waveSize;


    void Awake()
    {
        enemySpawnerInstance = this;
    }


    void Start()
    {
        enemyPool = GameObject.Find("EnemyPool").transform;
        buildingParent = GameObject.Find("PlayerBuildingsParent").transform;
        enemyBaseParent = GameObject.Find("EnemyBaseParent").transform;

        buildingList = new List<GameObject>();
        sphereSet = false;
        sphereResized = false;
        spawningUnits = false;
        spawningWave = false;
        timerStarted = false;
        unitCount = 0;
        unitMax = 4;
        waveGroupNum = 0;
        waveGroupMax = 6;
        waveSize = 3;

        Invoke("SpawnBase", 2f);
        Invoke("UpdateSphere", 2f);
    }

    /// <summary>
    /// First spawns the enemy base building in one of the four corners of the map
    /// </summary>
    private void SpawnBase()
    {
        bool baseSpawned = false;
        RaycastHit hit;

        do
        {
            for (int i = 0; i < mapCorners.Length; i++)
            {
                Vector3 baseSpawn = Random.insideUnitSphere * 50 + mapCorners[i].position;

                Ray raycast = new Ray(new Vector3(baseSpawn.x, 180.0f, baseSpawn.z), Vector3.down);

                if (Physics.Raycast(raycast, out hit, Mathf.Infinity, 1 << 8))
                {
                    if (hit.point.y == 18.49f)
                    {
                        baseSpawn.y = hit.point.y + 2f;

                        enemyHQ = Instantiate(Resources.Load("EnemyBuildings/EnemyBase_HQ"), baseSpawn,
                            Quaternion.Euler(0, Random.Range(0, 360), 0), enemyBaseParent) as GameObject;
                        baseSpawned = true;
                        break;
                    }
                }
            }

        }
        while (!baseSpawned);
    }

    /// <summary>
    /// Called whenever a new building is placed by the player or one is destroyed
    /// Used to resize the spawn perimeter for enemy units
    /// </summary>
    private void UpdateSphere()
    {
        if (!sphereSet)
        {
            coreBuilding = buildingParent.GetChild(0).gameObject;

            spawnSphere.transform.position = coreBuilding.transform.position;
            sphereScale = 150f;
            spawnSphere.transform.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
            buildingList.Add(coreBuilding);
            sphereSet = true;
            sphereResized = true;
        }
        else StartCoroutine(ResizeSphere());
    }

    /// <summary>
    /// Resizes sphere by increasing its radius until it overlaps all currently active base buildings
    /// </summary>
    /// <returns></returns>
    private IEnumerator ResizeSphere()
    {
        sphereResized = false;
        spawnSphere.transform.position = coreBuilding.transform.position;
        sphereScale = 150f;
        spawnSphere.transform.localScale = new Vector3(sphereScale, sphereScale, sphereScale);

        do
        {
            Collider[] hitColliders = Physics.OverlapSphere(spawnSphere.transform.position, sphereScale * 0.5f, 1 << 9);

            if (hitColliders.Length == buildingList.Count)
            {
                sphereResized = true;
                sphereScale += 150;
            }
            else
            {
                sphereScale += 5f;
            }

            spawnSphere.transform.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
            yield return new WaitForFixedUpdate();
        }
        while (!sphereResized);

        if (!timerStarted)
        {
            StartCoroutine(UnitSpawnTime());
            StartCoroutine(WaveSpawnTime());
            timerStarted = true;
        }
    }

    /// <summary>
    /// Spawns wandering units at a random location within the spawnSphere radius
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnUnits()
    {
        Vector3 mapPoint;

        spawningUnits = true;

        do
        {
            mapPoint = FindPointOnMap(SpawnType.Random);

            foreach (Transform unit in enemyPool)
            {
                if (!unit.gameObject.activeSelf)
                {
                    spawnedUnit = unit.gameObject;
                    spawnedUnit.SetActive(true);
                    spawnedUnit.transform.position = mapPoint;
                    Enemy enemyScript = spawnedUnit.GetComponent<Enemy>();
                    enemyScript.SpawnUnit(null);
                    break;
                }
            }

            unitCount++;

            yield return new WaitForSeconds(0.5f);

        }
        while (unitCount < unitMax);

        spawningUnits = false;
    }

    /// <summary>
    /// Spawns a group of units around the enemy base building object
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnWave()
    {
        Vector3 mapPoint;
        Enemy enemyScript;
        waveGroupNum = 0;
        spawningWave = true;

        do
        {
            bool leadSpawned = false;
            waveGroupCount = 0;

            for (int i = 0; i < waveGroupMax; i++)
            {
                if (!leadSpawned)
                {
                    mapPoint = FindPointOnMap(SpawnType.Base);

                    foreach (Transform unit in enemyPool)
                    {
                        if (!unit.gameObject.activeSelf)
                        {
                            waveUnit = unit.gameObject;
                            waveUnit.SetActive(true);
                            waveUnit.transform.position = mapPoint;
                            leadUnit = waveUnit;
                            break;
                        }
                    }
                }
                else
                {
                    mapPoint = FindPointOnMap(SpawnType.Base);

                    foreach (Transform unit in enemyPool)
                    {
                        if (!unit.gameObject.activeSelf)
                        {
                            waveUnit = unit.gameObject;
                            waveUnit.SetActive(true);
                            waveUnit.transform.position = mapPoint;
                            break;
                        }
                    }
                }

                enemyScript = waveUnit.GetComponent<Enemy>();
                enemyScript.SpawnUnit(coreBuilding);

                yield return new WaitForSeconds(0.2f);
            }

            waveGroupNum++;
            yield return new WaitForSeconds(30f);

        }
        while (waveGroupNum < waveSize);

        spawningWave = false;
    }

    /// <summary>
    /// Used to find the Vector3 spawn locations for the different unit spawnings
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private Vector3 FindPointOnMap(SpawnType type)
    {
        RaycastHit rayHit;
        NavMeshHit navHit;
        Vector3 spawnPoint = new Vector3(300, 20, 300), rayPoint, randomPoint;
        bool validPoint, hitTerrain;

        spawningUnits = true;

        for (int i = 0; i < 30; i++)
        {
            hitTerrain = false;

            switch (type)
            {
                case SpawnType.Random:
                    randomPoint = spawnSphere.transform.position + Random.insideUnitSphere.normalized * sphereScale * 0.5f;
                    break;
                case SpawnType.Base:
                    randomPoint = enemyHQ.transform.position + Random.insideUnitSphere * 70f;
                    break;
                case SpawnType.Wave:
                    randomPoint = leadUnit.transform.position + Random.insideUnitSphere * 5f;
                    break;
                default:
                    randomPoint = spawnSphere.transform.position + Random.insideUnitSphere.normalized * sphereScale * 0.5f;
                    break;
            }

            rayPoint = new Vector3(randomPoint.x, 100f, randomPoint.z);

            if (Physics.Raycast(rayPoint, transform.up * -200f, out rayHit, 200f, -1, QueryTriggerInteraction.Collide))
            {
                if (rayHit.transform.tag == "Terrain")
                {
                    hitTerrain = true;
                }
            }

            if (hitTerrain)
            {
                if (NavMesh.SamplePosition(randomPoint, out navHit, 200f, NavMesh.AllAreas))
                {
                    spawnPoint = navHit.position;
                    validPoint = true;

                    break;
                }
                else validPoint = false;
            }
        }

        return spawnPoint;
    }

    /// <summary>
    /// Time delay between wandering units being spawned
    /// </summary>
    /// <returns></returns>
    private IEnumerator UnitSpawnTime()
    {
        do
        {
            yield return new WaitForSeconds(20f);

            if (unitCount < unitMax & !spawningUnits)
            {
                StartCoroutine(SpawnUnits());
            }
        }
        while (true);
    }

    /// <summary>
    /// Time delay between waves of units spawning
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaveSpawnTime()
    {
        do
        {
            yield return new WaitForSeconds(300f);

            if (!spawningWave)
            {
                StartCoroutine(SpawnWave());
            }
        }
        while (true);
    }


    public void AddToBuildingList(GameObject building)
    {
        buildingList.Add(building);
        UpdateSphere();
    }


    public void RemoveFromBuildingList(GameObject building)
    {
        buildingList.Remove(building);
        UpdateSphere();
    }


    public void DespawnUnit(GameObject unit)
    {
        unit.transform.position = enemyPool.position;
        unit.gameObject.SetActive(false);
        unitCount--;
    }
}
