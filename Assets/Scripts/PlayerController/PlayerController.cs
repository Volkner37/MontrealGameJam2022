using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float forwardAcceleration = 1;
    [SerializeField] private float backwardAcceleration = 1;
    [SerializeField] private float sideAcceleration = 1;
    [SerializeField] private float forwardDeceleration = 1;
    [SerializeField] private float sideDeceleration = 1;
    [SerializeField] public float gravity = 9.8f;
    [SerializeField] private float maxSpeed = 1;


    [SerializeField] private float forceJump;

    [SerializeField] private float maxDistance;
    [SerializeField] private float force_magnet;

    [SerializeField] private float force_magnet_object;

    [SerializeField] private AnimationCurve staticAcceleration;

    private Boolean leftLock = false;
    private Boolean rightLock = false;


    private Camera _camera;
    private Rigidbody _rigidbody;
    private Vector3 _inputDirection;
    private Vector3 _dampeningDirection;
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //grounded
        Boolean grounded = false;
        RaycastHit ground;
        int layerMask = ~LayerMask.GetMask("Player");
        if (Physics.Raycast(_rigidbody.transform.position, new Vector3(0,-1,0), out ground, 1.2f, layerMask)) 
        {
            grounded = true;
        }

        //Accelerations
        float vertical = Input.GetAxis("Vertical");
        if (vertical >= 0)
            vertical *= forwardAcceleration;
        else if (vertical <= 0)
            vertical *= backwardAcceleration;

        float horizontal = Input.GetAxis("Horizontal");
        horizontal *= sideAcceleration;

        //For diagonal speeds
        float maxDiagonalSpeed = Mathf.Max(forwardAcceleration, backwardAcceleration, sideAcceleration);
        _inputDirection = Vector3.ClampMagnitude((new Vector3(_camera.transform.right.x,0,_camera.transform.right.z)) * horizontal + (new Vector3(_camera.transform.forward.x,0,_camera.transform.forward.z)) * vertical, maxDiagonalSpeed);


        _dampeningDirection = _rigidbody.velocity * (-1 * forwardDeceleration);
                
        //Applying forces
    
        if(!rightLock && !leftLock)
            _rigidbody.AddForce((_inputDirection + ( grounded ? _dampeningDirection : Vector3.zero )) * Time.deltaTime, ForceMode.VelocityChange);
        

        
        //Magnet and stuff
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            _rigidbody.AddForce(forceJump * Vector3.up, ForceMode.Impulse);
        }

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
        }

        _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxSpeed);
        
    #region DebugRays
        Debug.DrawRay(transform.position,  _inputDirection * 2.0f, Color.green);
        Debug.DrawRay(transform.position,  _rigidbody.velocity * 2.0f, Color.blue);
        Debug.DrawRay(transform.position, _dampeningDirection, Color.red);
        Debug.DrawRay(_camera.transform.position, _camera.transform.forward * 3f, Color.yellow);
    #endregion
    }

    void pushpull(Boolean pushOrPull){
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Metal");
        //layerMask = ~layerMask;
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, maxDistance, layerMask)) // Mathf.Infinity
        { 
            GameObject objectHit = hit.transform.gameObject;

            if(objectHit.GetComponent<PullandPush>())
            {
                PullandPush interable = objectHit.GetComponent<PullandPush>();
                Boolean isStatic = interable.getIsStatic();
                if(isStatic)
                {
                    float acceleration = staticAcceleration.Evaluate((hit.distance)/30);
                    _rigidbody.AddForce(acceleration * force_magnet * (pushOrPull? _camera.transform.forward : -_camera.transform.forward) , ForceMode.Impulse);
                }
                else{

                }
            }
        }
        else{
            if(pushOrPull)
                rightLock = false;
            else
                leftLock = false;
        }

    }
}
