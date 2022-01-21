using System;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTester : MonoBehaviour
{
    private Rigidbody _rigidbody;

    [SerializeField] private float maxDistance;
    [SerializeField] private float force_magnet;

    [SerializeField] private float force_magnet_object;

    [SerializeField] private AnimationCurve staticAcceleration;

    private Boolean leftLock = false;
    private Boolean rightLock = false;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetMouseButton(0) && !leftLock)
        { // left click
            pushpull(true);
            rightLock = true;
        }
        if (Input.GetMouseButton(1) && !rightLock)
        { // right click
            pushpull(false);
            leftLock = true;
        }

        if (Input.GetMouseButtonUp(0))
        { // left click
            rightLock = false;
        }
        if (Input.GetMouseButtonUp(1))
        { // right click
            leftLock = false;
        }*/
        
    }

    void pushpull(Boolean pushOrPull){
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Metal");
        //layerMask = ~layerMask;
        Camera camera = GetComponentInChildren<Camera>();
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, maxDistance, layerMask)) // Mathf.Infinity
        {
            Debug.DrawRay(camera.transform.position, camera.transform.forward * hit.distance, Color.yellow);
            GameObject objectHit = hit.transform.gameObject;

            if(objectHit.GetComponent<PullandPush>())
            {
                PullandPush interable = objectHit.GetComponent<PullandPush>();
                Boolean isStatic = interable.getIsStatic();
                if(isStatic)
                {
                    float acceleration = staticAcceleration.Evaluate((hit.distance)/100);
                    //float acceleration = (100 - hit.distance) * (100 - hit.distance)/5000;
                    _rigidbody.AddForce(acceleration * force_magnet * (pushOrPull? camera.transform.forward : -camera.transform.forward) , ForceMode.Impulse);
                }
                else{

                }
            }
        }

    }
}
