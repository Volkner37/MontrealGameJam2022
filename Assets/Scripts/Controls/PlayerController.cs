using System;
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
    [SerializeField] private float maxSpeed = 1;
    [SerializeField] private float forceJump;

    [Space]
    [Header("Gun settings")] 
    [SerializeField] private float maxRange;
    [SerializeField] private float forceMagnet;
    [SerializeField] private float forceMagnetObject;
    [SerializeField] private AnimationCurve staticAcceleration;

    [Space] [Header("Debug")] 
    [SerializeField] private bool enableDebugRay = false;
    [SerializeField] private bool enableDebugGun = false;
    [SerializeField] private bool enableSticky = false;
    
    private Camera _camera;
    private Rigidbody _rigidbody;
    private PhysicMaterial _physicsMaterial;
    private Vector3 _inputDirection;
    private bool _isGrounded = false;

    #region Attract/Retract
    private bool _isTryingToRepel = false;
    private bool _isTryingToAttract = false;
    private bool _repelLocked = false; 
    private bool _attractLocked = false;
    private bool IsAttracting => _isTryingToAttract && !_attractLocked;
    private bool IsRepelling => _isTryingToRepel && !_repelLocked;
    private bool IsUsingGun => IsRepelling || IsAttracting;
    public Vector3 TargetPosition { get; private set; }

    #endregion
    
    #region WallStick
    private bool _isSticked = false;
    #endregion

    #region Inputs

    private float _verticalAxis;
    private float _horizontalAxis;
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
        _verticalAxis = Input.GetAxis("Vertical");
        _horizontalAxis = Input.GetAxis("Horizontal");
        _isTryingToAttract = Input.GetMouseButton(0);
        _isTryingToRepel = Input.GetMouseButton(1);
    
        if (Input.GetMouseButtonUp(0))
        {
            _repelLocked = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            _attractLocked = false;
        }
    
        if(!_isJumping)
            _isJumping = Input.GetKeyDown(KeyCode.Space);
    
        if (Input.GetMouseButtonUp(0))
        {
            _repelLocked = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            _attractLocked = false;
        }
    }

    void FixedUpdate()
    {
        UpdateGrounded();

        UpdateStickStatus();
        
        if(!IsUsingGun)
        {
            TargetPosition = Vector3.zero;
            _isSticked = false;
            
            _physicsMaterial.dynamicFriction = 2;
        
            UpdateInputDirection();
            CheckJumpInput();
        
            _rigidbody.AddForce(_inputDirection * Time.deltaTime, ForceMode.Impulse);
        }
        else
        {
            _physicsMaterial.dynamicFriction = 0;
            _repelLocked = IsAttracting;
            _attractLocked = IsRepelling;
            UpdateMagnetGunEffect();
        }
    
        _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxSpeed);
    
        ShowDebug();
    }

    private void UpdateGrounded()   
    {
        RaycastHit hitGround;
        int layerMask = ~LayerMask.GetMask("Player");
        _isGrounded = Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hitGround, 1.5f, layerMask);
    }

    private void UpdateStickStatus()
    {
        if (_isSticked)
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        else
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void UpdateInputDirection()
    {
        //Accelerations
        _verticalAxis = Input.GetAxis("Vertical");
        if (_verticalAxis >= 0)
            _verticalAxis *= forwardAcceleration;
        else if (_verticalAxis <= 0)
            _verticalAxis *= backwardAcceleration;
    
        _horizontalAxis *= sideAcceleration;

        //For diagonal speeds
        float maxDiagonalSpeed = Mathf.Max(forwardAcceleration, backwardAcceleration, sideAcceleration);
    
        if (!_isGrounded)
        {
            _verticalAxis *= airControlRatio;
            _horizontalAxis *= airControlRatio;
            maxDiagonalSpeed *= airControlRatio;
        }
    
        _inputDirection = Vector3.ClampMagnitude((new Vector3(_camera.transform.right.x,0, _camera.transform.right.z)) * _horizontalAxis + (new Vector3(_camera.transform.forward.x,0, _camera.transform.forward.z)) * _verticalAxis, maxDiagonalSpeed);
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

        if (enableDebugRay)
        {
            var position = transform.position;
            Debug.DrawRay(position,  _inputDirection * 2.0f, Color.green);
            Debug.DrawRay(position,  _rigidbody.velocity * 2.0f, Color.blue);
            Debug.DrawRay(_camera.transform.position, _camera.transform.forward * 3f, Color.yellow);
        }
        #endregion
    
        #region Gun

        if (enableDebugGun)
        {
            if(IsAttracting)
                Debug.Log("Attracting");
            if(IsRepelling)
                Debug.Log("Repelling");
        }
        #endregion
        
        #region Stick
        if(enableSticky)
        {
            if(_isSticked)
                Debug.Log("IsSticked");
            if(!_isSticked)
                Debug.Log("IsFree");
        }
        #endregion

    }

    void UpdateMagnetGunEffect()
    {
        //Without this, the player could block the raycast.
        int layerMask = ~LayerMask.GetMask("Player");

        RaycastHit hit;

        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, maxRange,
            layerMask))
        {
            Magnetic target;
            if (hit.transform.TryGetComponent(out target))
            {
                if (target is {IsStatic: true})
                {
                    float acceleration = staticAcceleration.Evaluate((hit.distance) / maxRange);
                    _rigidbody.AddForce(
                        (IsAttracting ? _camera.transform.forward : -_camera.transform.forward) *
                        (acceleration * forceMagnet * Time.deltaTime), ForceMode.Force);
                }
            }
            else
            {
                _repelLocked = false;
                _attractLocked = false;
            }
        }
    
        TargetPosition = hit.point;
    }

    private void OnCollisionStay(Collision other)
    {
        if(other.gameObject.GetComponent<Magnetic>())
            _isSticked = IsAttracting;
    }
}
