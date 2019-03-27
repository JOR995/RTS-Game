using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireframeCamera : MonoBehaviour {

    //Script attached to the camera the shows only the wireframe layer
    //Makes the camera render everything as a wireframe
    void OnPreRender()
    {
        GL.wireframe = true;
    }
    void OnPostRender()
    {
        GL.wireframe = false;
    }
}
