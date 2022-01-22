using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Walking Settings")]
    [SerializeField] private float forwardAcceleration = 1;
    [SerializeField] private float backwardAcceleration = 1;
    [SerializeField] private float sideAcceleration = 1;
    [SerializeField] private float forwardDeceleration = 1;
    [SerializeField] private float sideDeceleration = 1;

    [Space]
    [Header("Global Values")] 
    [SerializeField] public float gravity = 9.8f;
    [SerializeField] private float maxSpeed = 1;
    [SerializeField] private float forceJump;

    [Space]
    [Header("Gun settings")] 
    [SerializeField] private float maxRange;
    [SerializeField] private float forceMagnet;
    [SerializeField] private float forceMagnetObject;
    [SerializeField] private AnimationCurve staticAcceleration;
    [SerializeField] private bool DebugMode = true;

    
    private Camera _camera;
    private Rigidbody _rigidbody;
    private Vector3 _inputDirection;
    private Vector3 _dampeningDirection;
    private bool _grounded = false;

    #region GunLogics  
    private bool _IsTryingToRepel = false;
    private bool _IsTryingToAttract = false;
    private bool _RepelLocked = false; 
    private bool _AttractLocked = false;
    private bool _IsAttracting => _IsTryingToAttract && !_AttractLocked;
    private bool _IsRepeling => _IsTryingToRepel && !_RepelLocked;
    private bool _IsUsingGun => _IsRepeling || _IsAttracting;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        UpdateGrounded();
        UpdateMouseInput();

        if(!_IsUsingGun)
        {
            UpdateInputDirection();
            UpdateDampening();    
            CheckJumpInput();

            _rigidbody.AddForce((_inputDirection + ( _grounded ? _dampeningDirection : Vector3.zero )) * Time.deltaTime, ForceMode.VelocityChange);
        }
        else
        {
            if (_IsAttracting) // left click 
            {        
                UpdateMagnetGunEffect();
                _RepelLocked = true;
            }
            if (_IsRepeling) // right click
            {
                UpdateMagnetGunEffect();
                _AttractLocked = true;
            }
        }

        if (Input.GetMouseButtonUp(0)) // Pulling button UP
        {
            _RepelLocked = false;
        }
        if (Input.GetMouseButtonUp(1)) // Pushing button UP
        {
            _AttractLocked = false;
        }

        _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxSpeed);
        
        if(DebugMode)
            ShowDebug();
    }

    private void UpdateGrounded()   
    {
        RaycastHit hitGround;
        int layerMask = ~LayerMask.GetMask("Player");
        _grounded = Physics.Raycast(_rigidbody.transform.position, new Vector3(0,-1,0), out hitGround, 1.2f, layerMask) ? true : false;
    }

    private void UpdateInputDirection()
    {
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
    }

    private void UpdateDampening()
    {
        _dampeningDirection = _rigidbody.velocity * (-1 * forwardDeceleration);
    }

    private void CheckJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _grounded)
        {
            _rigidbody.AddForce(Vector3.up * (forceJump * Time.deltaTime), ForceMode.Impulse);
        }
    }

    private void UpdateMouseInput()
    {
        _IsTryingToAttract = Input.GetMouseButton(0);
        _IsTryingToRepel = Input.GetMouseButton(1);
    }

    private void ShowDebug()
    {
        #region DebugRays
        Debug.DrawRay(transform.position,  _inputDirection * 2.0f, Color.green);
        Debug.DrawRay(transform.position,  _rigidbody.velocity * 2.0f, Color.blue);
        Debug.DrawRay(transform.position, _dampeningDirection, Color.red);
        Debug.DrawRay(_camera.transform.position, _camera.transform.forward * 3f, Color.yellow);
        #endregion
        
        #region Gun
        if(_IsAttracting)
            Debug.Log("Attracting");
        if(_IsRepeling)
            Debug.Log("Repeling");
        #endregion

    }

    void UpdateMagnetGunEffect()
    {
        RaycastHit hit;
        //Without this, the player could block the raycast.
        int layerMask = ~LayerMask.GetMask("Player");

        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, maxRange, layerMask))
        { 
            GameObject objectHit = hit.transform.gameObject;

            if(objectHit.GetComponent<Magnetic>())
            {
                Magnetic interable = objectHit.GetComponent<Magnetic>();
                if(interable.IsStatic)
                {
                    float acceleration = staticAcceleration.Evaluate((hit.distance)/maxRange);
                    _rigidbody.AddForce((_IsAttracting? _camera.transform.forward : -_camera.transform.forward) * (acceleration * forceMagnet * Time.deltaTime) , ForceMode.Impulse);
                }
            }
        }
        else{
                _RepelLocked = false;
                _AttractLocked = false;
        }

    }
}
