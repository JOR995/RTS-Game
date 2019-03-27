using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController cameraControllerInstance;

    [SerializeField]
    GameObject terrainObject;

    [SerializeField]
    int mapWidth, mapHeight;

    [SerializeField]
    Camera mainCam, FoWCam, wireframeCam, projectilesCam, UICam;

    [SerializeField]
    Transform rotatePoint;

    private RenderTexture renderTexture;
    private Vector3 clampedPos;

    private float cameraPanSpeed, cameraZoomSpeed;
    private float forwardMovement, sideMovement, zoomMovement, rotateMovement, minPosX, maxPosX, minPosV, maxPosV;
    private bool holdingBuilding;


    void Awake()
    {
        cameraControllerInstance = this;
    }


    void Start()
    {
        //Camera movement speed variable, used as a multiplier to the distance the camera is moved every frame
        cameraPanSpeed = 150.0f;

        //Camera zoom speed variable, used as a mutliplier to the distance the camera is moved when the user scrolls the mouse wheel
        cameraZoomSpeed = 1500f;

        //Tells whether the player is currently holding a building
        holdingBuilding = false;

        //Works out the maximum and minimum values for x and z using the position of the terrain object, the map width and height and the scale of the terrain object 
        minPosX = (terrainObject.transform.position.x - mapWidth / 2) * terrainObject.transform.localScale.x;
        maxPosX = (terrainObject.transform.position.x + mapWidth / 2) * terrainObject.transform.localScale.x;
        minPosV = (terrainObject.transform.position.z - mapHeight / 2) * terrainObject.transform.localScale.z - 50f;
        maxPosV = (terrainObject.transform.position.z + mapHeight / 2) * terrainObject.transform.localScale.z;

        renderTexture = new RenderTexture(mapWidth * 5, mapHeight * 5, 16, RenderTextureFormat.ARGB32);
        //mainCam.targetTexture = renderTexture;
    }


    void LateUpdate()
    {
        //Detects for key presses corresponding to the vertical and horizontal input axis
        //Once detected the axis value (1 or -1) is multiplied by the camera movement speed and by deltaTime
        forwardMovement = Input.GetAxis("Vertical") * cameraPanSpeed * Time.deltaTime;
        sideMovement = Input.GetAxis("Horizontal") * cameraPanSpeed * Time.deltaTime;

        if (!holdingBuilding) zoomMovement = Input.GetAxis("Mouse ScrollWheel") * -cameraZoomSpeed * Time.deltaTime;

        //rotateMovement = Input.GetAxis("Rotate") * cameraPanSpeed * Time.deltaTime;
        //transform.Rotate(rotatePoint.position,rotateMovement);

        //Then translates the position of the camera in world space by applying the values worked out in the previous lines
        transform.Translate(sideMovement, zoomMovement, forwardMovement, Space.World);

        //Assigns temporary Vector3 to equal current position of camera
        clampedPos = transform.position;

        //Then clamps the x and z positions of that vector3 using the minimum and maximum values for x and z
        clampedPos.x = Mathf.Clamp(transform.position.x, minPosX, maxPosX);
        clampedPos.z = Mathf.Clamp(transform.position.z, minPosV, maxPosV);
        clampedPos.y = Mathf.Clamp(transform.position.y, 40f, 125f);
        //Finally makes the position of the camera equal to the clamped position - this keeps the camera within the world space above the terrain object
        transform.position = clampedPos;
    }


    public bool HoldingBuilding
    {
        set
        {
            holdingBuilding = value;
        }
    }


    public void PlayerBaseSpawned(Transform playerBase)
    {
        transform.position = new Vector3(playerBase.position.x, 100, playerBase.position.z - 100f);
    }
}
