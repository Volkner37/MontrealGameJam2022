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
    [SerializeField] private float airControlRatio = 0.3f;

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
    [SerializeField] private bool debugMode = true;

    
    private Camera _camera;
    private Rigidbody _rigidbody;
    private PhysicMaterial _physicsMaterial;
    private Vector3 _inputDirection;
    private bool _isGrounded = false;

    #region GunLogics  
    private bool _IsTryingToRepel = false;
    private bool _IsTryingToAttract = false;
    private bool _RepelLocked = false; 
    private bool _AttractLocked = false;
    private bool _IsAttracting => _IsTryingToAttract && !_AttractLocked;
    private bool _IsRepeling => _IsTryingToRepel && !_RepelLocked;
    private bool _IsUsingGun => _IsRepeling || _IsAttracting;
    #endregion
    
    #region Inputs

    private float _VecticalAxis;
    private float _HorizontalAxis;
    private bool _isJumping;


    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        _rigidbody = GetComponent<Rigidbody>();
        _physicsMaterial = GetComponent<CapsuleCollider>().material;
    }

    void Update()
    {
        UpdateInputs();
    }

    private void UpdateInputs()
    {
        _VecticalAxis = Input.GetAxis("Vertical");
        _HorizontalAxis = Input.GetAxis("Horizontal");
        _IsTryingToAttract = Input.GetMouseButton(0);
        _IsTryingToRepel = Input.GetMouseButton(1);
        
        if(!_isJumping)
            _isJumping = Input.GetKeyDown(KeyCode.Space);
    }
    
    void FixedUpdate()
    {
        UpdateGrounded();

        if(!_IsUsingGun)
        {
            _physicsMaterial.dynamicFriction = 2;
            
            UpdateInputDirection();
            CheckJumpInput();
            
            _rigidbody.AddForce(_inputDirection * Time.deltaTime, ForceMode.Impulse);
        }
        else
        {
            _physicsMaterial.dynamicFriction = 0;
            _RepelLocked = _IsAttracting;
            _AttractLocked = _IsRepeling;
            UpdateMagnetGunEffect();
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
        
        if(debugMode)
            ShowDebug();
    }

    private void UpdateGrounded()   
    {
        RaycastHit hitGround;
        int layerMask = ~LayerMask.GetMask("Player");
        _isGrounded = Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hitGround, 1.5f, layerMask);
    }

    private void UpdateInputDirection()
    {
        //Accelerations
        _VecticalAxis = Input.GetAxis("Vertical");
        if (_VecticalAxis >= 0)
            _VecticalAxis *= forwardAcceleration;
        else if (_VecticalAxis <= 0)
            _VecticalAxis *= backwardAcceleration;
        
        _HorizontalAxis *= sideAcceleration;

        //For diagonal speeds
        float maxDiagonalSpeed = Mathf.Max(forwardAcceleration, backwardAcceleration, sideAcceleration);
        
        if (!_isGrounded)
        {
            _VecticalAxis *= airControlRatio;
            _HorizontalAxis *= airControlRatio;
            maxDiagonalSpeed *= airControlRatio;
        }
        
        _inputDirection = Vector3.ClampMagnitude((new Vector3(_camera.transform.right.x,0, _camera.transform.right.z)) * _HorizontalAxis + (new Vector3(_camera.transform.forward.x,0, _camera.transform.forward.z)) * _VecticalAxis, maxDiagonalSpeed);
    }

    private void CheckJumpInput()
    {
        if (_isJumping && _isGrounded)
        {
            _rigidbody.AddForce(Vector3.up * (forceJump * Time.deltaTime), ForceMode.Impulse);
            _isJumping = false;
        }
    }

    

    private void ShowDebug()
    {
        #region DebugRays

        var position = transform.position;
        Debug.DrawRay(position,  _inputDirection * 2.0f, Color.green);
        Debug.DrawRay(position,  _rigidbody.velocity * 2.0f, Color.blue);
        Debug.DrawRay(_camera.transform.position, _camera.transform.forward * 3f, Color.yellow);
        #endregion
        
        #region Gun
        if(_IsAttracting)
            Debug.Log("Attracting");
        if(_IsRepeling)
            Debug.Log("Repelling");
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

            Magnetic magneticComponent = objectHit.GetComponent<Magnetic>();
            if(magneticComponent is {IsStatic: true})
            {
                float acceleration = staticAcceleration.Evaluate((hit.distance)/maxRange);
                _rigidbody.AddForce((_IsAttracting? _camera.transform.forward : -_camera.transform.forward) * (acceleration * forceMagnet * Time.deltaTime) , ForceMode.Force);
            }
        }
        else
        {
            _RepelLocked = false;
            _AttractLocked = false;
        }

    }
}
