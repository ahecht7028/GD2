using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableRenderer : MonoBehaviour
{
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
