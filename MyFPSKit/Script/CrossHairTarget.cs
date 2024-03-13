using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHairTarget : MonoBehaviour
{
    private Camera _mainCamera;
    private Ray ray;
    private RaycastHit hitInfo;
    void Start()
    {
        _mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        ray.origin = _mainCamera.transform.position;
        ray.direction = _mainCamera.transform.forward;
        if (Physics.Raycast(ray, out hitInfo))
        {
            transform.position = hitInfo.point;
        }
        else
        {
            transform.position = ray.origin + ray.direction * 1000.0f;
        }
        
    }
}
