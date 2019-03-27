using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class MapObjectGeneration
{

    public static void GenerateObjects(GameObject terrainObject, Transform treeParent, Transform rockParent, Transform metalParent, Transform crystalParent, Transform playerBuildingsParent, int mapWidth, int mapHeight)
    {
        GameObject generatedObject;
        MeshCollider terrainCollider = terrainObject.GetComponent<MeshCollider>();
        RaycastHit hit;
        Vector3 playerSpawn;

        int currentXPos, currentZPos, resource1Count = 0, resource2Count = 0;
        bool playerSpawned = false;

        //Gets the starting x and z coordinates in world space by taking the position of the terrain mesh
        //Then subtracts the width and height divided by 2 and multiplies by the scale
        // This finds a corner of the terrain with the lowest x and z coordinates
        currentXPos = Mathf.RoundToInt((terrainObject.transform.position.x - mapWidth / 2) * terrainObject.transform.localScale.x);
        currentZPos = Mathf.RoundToInt((terrainObject.transform.position.z - mapHeight / 2) * terrainObject.transform.localScale.z);

        //From the starting point found earlier, loops through and increments x and z, this moves to a new set of coords where an object can spawn
        for (int y = 0; y < mapHeight * terrainObject.transform.localScale.z; y += 4)
        {
            for (int x = 0; x < mapWidth * terrainObject.transform.localScale.x; x += 4)
            {
                //Sets the current position using the x and z coords of the starting corner of the terrain and adds the ints x and y
                Vector3 currentPosition = new Vector3(currentXPos + x, 0, currentZPos + y);

                //Creates a new ray and sets its position using the vector3 from the previous line, the y coord is set to be high above the map
                Ray ray = new Ray(new Vector3(currentPosition.x, 180.0f, currentPosition.z), Vector3.down);

                //Height levels = 16, 18.49, 22.09

                //Using the created ray object, a terrain collider raycast is made, this returns a hit if it collides with the collider of the terrain object
                if (terrainCollider.Raycast(ray, out hit, Mathf.Infinity))
                {
                    //Using the y coord of where the raycast hits an object is instantiated at the hit point
                    //Object spawning is also based on a random probability so objects will not always be spawned at these positions which would overcrowd the map
                    //Objects are added to parent transforms and use random rotations on the y-axis to add some variance
                    if (hit.point.y < 11.5f && Random.Range(0, 1f) < 0.12f)
                    {
                        currentPosition.y = hit.point.y;

                        generatedObject = Object.Instantiate(Resources.Load("EnvironmentObjects/FirTree_02"), currentPosition,
                            Quaternion.Euler(0, Random.Range(0, 360f), 0), treeParent) as GameObject;
                        generatedObject.transform.localScale = new Vector3(3.5f, 5.5f, 3.5f);
                    }
                    else if (hit.point.y > 16f && hit.point.y < 18.49f && Random.Range(0, 1f) < 0.004f && resource2Count < 10)
                    {
                        currentPosition.y = hit.point.y - 3.2f;
                        generatedObject = Object.Instantiate(Resources.Load("EnvironmentObjects/resource_2"), currentPosition,
                            Quaternion.Euler(0, Random.Range(0, 360f), 0), crystalParent) as GameObject;
                        generatedObject.transform.localScale = new Vector3(6f, 6f, 6f);
                        resource2Count++;
                    }
                    else if (hit.point.y == 18.49f && Random.Range(0, 1f) < 0.002f)
                    {
                        currentPosition.y = hit.point.y;

                        generatedObject = Object.Instantiate(Resources.Load("EnvironmentObjects/rock_" + Random.Range(1, 17)), currentPosition,
                            Quaternion.Euler(0, Random.Range(0, 360f), 0), rockParent) as GameObject;
                        generatedObject.transform.localScale = new Vector3(2f, 2f, 2f);


                    }
                    else if (hit.point.y == 22.09f && Random.Range(0, 1f) < 0.008f && resource1Count < 30)
                    {
                        currentPosition.y = hit.point.y - 3f;

                        generatedObject = Object.Instantiate(Resources.Load("EnvironmentObjects/resource_1"), currentPosition,
                            Quaternion.Euler(0, Random.Range(0, 360f), 0), metalParent) as GameObject;
                        generatedObject.transform.localScale = new Vector3(6f, 6f, 6f);
                        resource1Count++;
                    }
                }


            }
        }
        //Debug.Log("Spawned " + treeParent.childCount + " trees");
        //Debug.Log("Spawned " + rockParent.childCount + " rocks");
        //Debug.Log("Spawned " + metalParent.childCount + " metal nodes");
        //Debug.Log("Spawned " + crystalParent.childCount + " crystal nodes");

        //Finally the spawn location for the player's starting base is assigned and the core building instantiated
        //This is done within a loop until a valid point is found
        do
        {
            //Selects a random vector3 position within a sphere centering around the transform position of the terrain object
            playerSpawn = Random.insideUnitSphere * 150 + terrainObject.transform.position;

            //Creates a new ray object using this random position and sets it to be above the terrain
            Ray raycast = new Ray(new Vector3(playerSpawn.x, 180.0f, playerSpawn.z), Vector3.down);

            //Casts another collider raycast using the ray object
            if (terrainCollider.Raycast(raycast, out hit, Mathf.Infinity))
            {
                //Then checks the y-coord of the hit point. If it hits one of the mid level sections that position is used and building instantiated
                //If the point doesn't hit one of these heights it iterates and generates a new new position until a valid one is found
                if (hit.point.y == 18.49f || hit.point.y == 22.09f)
                {
                    playerSpawn.y = hit.point.y + 0.8f;

                    generatedObject = Object.Instantiate(Resources.Load("PlayerBuildings/BaseCore"), playerSpawn,
                        Quaternion.Euler(0, Random.Range(0, 360), 0), playerBuildingsParent) as GameObject;
                    playerSpawned = true;
                    
                    CameraController.cameraControllerInstance.PlayerBaseSpawned(generatedObject.transform);
                }
            }
        }
        while (!playerSpawned);

        //Debug.Log("Player spawn = " + playerSpawn);
    }
}
