using System;
using System.Collections;
using System.Drawing;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;
using Color = UnityEngine.Color;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Space]
    [Header("Global Settings")] 
    [SerializeField] private float maxHorizontalSpeed = 10;
    [SerializeField] private float maxVerticalSpeed = 2;
    [SerializeField] private float gravity = 0.3f;
    
    [Space]
    [Header("Walking Settings")]
    [SerializeField] private float forwardAcceleration = 1;
    [SerializeField] private float backwardAcceleration = 1;
    [SerializeField] private float sideAcceleration = 1;
    [SerializeField] private float airControlRatio = 0.3f;

    [Space] 
    [Header("Jump Settings")]
    [SerializeField] private bool allowJumping = true;
    [SerializeField] private float forceJump = 200;
    [SerializeField] private float dropForce = 20;
    
    [Space]
    [Header("Gun Settings")]
    [Header("Shared")]
    [SerializeField] private float maxRange;
    [SerializeField] private float _magnetVelocityDecay;
    [SerializeField] private Transform gunTipTransform;
    [Header("Attraction")]
    [SerializeField] private float attractionForceMagnet;
    [SerializeField] private AnimationCurve staticAttractionAcceleration;
    [Header("Repulsion")]
    [SerializeField] private float repulsionForceMagnet;
    [SerializeField] private AnimationCurve staticRepulsionAcceleration;
    
    [Space] 
    [Header("Debug Options")] 
    [SerializeField] private TextMeshProUGUI debugTextOutput;
    [SerializeField] private bool enableDebugRay = false;
    [SerializeField] private bool enableDebugGun = false;
    [SerializeField] private bool enableDebugSticky = false;
    [SerializeField] private bool enableDebugJump = false;
    [SerializeField] private GameObject targetHitVisualisation = null;
    
    private Camera _camera;
    private Rigidbody _rigidbody;
    private PhysicMaterial _physicsMaterial;
    private Vector3 _velocity;
    private Vector3 _magnetVelocity;
    private bool _isGrounded = false;
    private VisualEffect _VFX;
    private Vector3 _lastPosition;

    #region Attract/Retract
    private bool _isTryingToRepel = false;
    private bool _isTryingToAttract = false;
    private bool _repelLocked = false; 
    private bool _attractLocked = false;

    private Magnetic currentTarget;
    private float currentTargetDistance;
    private bool _isLookingAtMagneticObject => currentTarget != null;
    private bool IsAttracting => _isTryingToAttract && _isLookingAtMagneticObject;
    private bool IsRepelling => _isTryingToRepel && _isLookingAtMagneticObject;
    private bool IsUsingGun => (IsRepelling || IsAttracting);
    public Vector3 TargetPosition { get; private set; }
    #endregion
    
    #region WallStick
    private bool _isSticked = false;
    #endregion

    #region Inputs
    private float _verticalAxis;
    private float _horizontalAxis;
    private bool _isJumping;
    private bool _canJump;
    private float currentJumpVelocity;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        
        _camera = GetComponentInChildren<Camera>();
        _rigidbody = GetComponent<Rigidbody>();
        _physicsMaterial = GetComponent<CapsuleCollider>().material;
        _VFX = GetComponentInChildren<VisualEffect>();
        _VFX.enabled = true;
    }

    void Update()
    {
        UpdateInputs();
        
        CheckForMagneticObject();
    }

    private void UpdateInputs()
    {
        _verticalAxis = Input.GetAxis("Vertical");
        _horizontalAxis = Input.GetAxis("Horizontal");
        _isTryingToAttract = Input.GetMouseButton(0);
        _isTryingToRepel = Input.GetMouseButton(1);
    
        if (Input.GetMouseButtonUp(0))
        {
            _isTryingToAttract = false;
            _repelLocked = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            _isTryingToRepel = false;
            _attractLocked = false;
        }
        if (allowJumping && !_isJumping && _isGrounded)
        {
            _isJumping = Input.GetKeyDown(KeyCode.Space);
            if(_isJumping)
                currentJumpVelocity = forceJump;
        }
    }

    void CheckForMagneticObject()
    {
        //Without this, the player could block the raycast.
        int layerMask = ~LayerMask.GetMask("Player");
        RaycastHit hit;
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, maxRange, layerMask))
        {
            if (hit.transform.TryGetComponent(out currentTarget))
            {
                TargetPosition = hit.point;
                currentTargetDistance = hit.distance;
                return;
            }
        }
        
        currentTarget = null;
        currentTargetDistance = 0;
        TargetPosition = Vector3.zero;
    }

    void FixedUpdate()
    {
        UpdateGrounded();
        UpdateStickStatus();
        
        //Update the velocity
        if(allowJumping)
            UpdateJumpVelocity();
        
        if(!IsUsingGun)
        {
            //Resetting some values
            //_velocity = Vector3.zero;
            TargetPosition = Vector3.zero;
            _isSticked = false;
            
            StopVFX();
            CalculateVelocityDecay();
            UpdateInputDirection();

            if(!_isJumping && !_isGrounded)
                ApplyGravity();

            if (_isGrounded)
            {
                _velocity.y = 0;
            }
            
            //We apply the calculated velocity
            _rigidbody.velocity = _velocity + _magnetVelocity + (Vector3.up * currentJumpVelocity);
        }
        else
        {
            currentJumpVelocity = 0;

            _velocity = Vector3.zero;
            _repelLocked = IsAttracting;
            _attractLocked = IsRepelling;
            UpdateMagnetGunEffect();
            _magnetVelocity = _rigidbody.velocity - _velocity - (Vector3.up * currentJumpVelocity);
        }
    
        //Limits the max speed of the overall velocity
        _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxHorizontalSpeed);
        
        ShowDebug();
    }

    private void PlayVFX()
    {
        _VFX.SetVector3("origin", gunTipTransform.transform.position);
        _VFX.SetVector3("target", TargetPosition);
        _VFX.Play();
    }

    private void StopVFX()
    {
        _VFX.Stop();
    }

    private void CalculateVelocityDecay()
    {
        //Decay overtime
        if (_magnetVelocity.magnitude - (_magnetVelocity.normalized * _magnetVelocityDecay).magnitude < 0)
        {
            _magnetVelocity = Vector3.zero;
        }
        else
            _magnetVelocity -= _magnetVelocity.normalized * (_magnetVelocityDecay * Time.deltaTime);
        
        //If we enter in a wall.
        float speed = (transform.position - _lastPosition).magnitude / Time.deltaTime;
        _lastPosition = transform.position;

        if (speed <= 1)
            _magnetVelocity = Vector3.zero;
    }


    private void ApplyGravity()
    {
        if (_velocity.y > -1 * maxVerticalSpeed)
            _velocity.y -= gravity * Time.deltaTime;
        else
            _velocity.y = maxVerticalSpeed * -1;
    }

    private void UpdateGrounded()   
    {
        RaycastHit hitGround;
        int layerMask = ~LayerMask.GetMask("Player");
        _isGrounded = Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hitGround, 1.1f, layerMask);
    }

    private void UpdateStickStatus()
    {
        //todo : use velocity instead?
        if (_isSticked)
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        else
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void UpdateInputDirection()
    {
        //Accelerations
        if (_verticalAxis >= 0)
            _verticalAxis *= forwardAcceleration;
        else if (_verticalAxis <= 0)
            _verticalAxis *= backwardAcceleration;
    
        _horizontalAxis *= sideAcceleration;

        float maxDiagonalSpeed = Mathf.Max(forwardAcceleration, backwardAcceleration, sideAcceleration);
        if (!_isGrounded || _isJumping)
        {
            _verticalAxis *= airControlRatio;
            _horizontalAxis *= airControlRatio;
            maxDiagonalSpeed *= airControlRatio;
        }
        
        //This prevents to move faster in diagonal
        Vector3 result = Vector3.ClampMagnitude((new Vector3(_camera.transform.right.x,0, _camera.transform.right.z)) * _horizontalAxis + (new Vector3(_camera.transform.forward.x,0, _camera.transform.forward.z)) * _verticalAxis, maxDiagonalSpeed);

        _velocity.x = result.x;
        _velocity.z = result.z;

        //To allow the movement to cancel some magnet velocity
        float product = Vector3.Dot(_magnetVelocity, result * -1);

        if (product <= 0)
            return;
        
        Vector3 removingForce = _magnetVelocity.normalized * (-1 * product);
        
        if (_magnetVelocity.magnitude + removingForce.magnitude < 0)
        {
            _magnetVelocity = Vector3.zero;
        }
        else
        {
            _magnetVelocity += removingForce * Time.deltaTime;
        }
    }

    private void UpdateJumpVelocity()
    {
        if (_isJumping && _isGrounded)
        {
            _velocity.y += currentJumpVelocity;
            _isJumping = false;
        }
        
        //We decay the jump velocity with gravity
        currentJumpVelocity -=  gravity * Time.deltaTime;
        if (currentJumpVelocity < 0)
            currentJumpVelocity = 0;
    }

    private void ShowDebug()
    {
        #region DebugRays

        if (enableDebugRay)
        {
            var position = transform.position;
            Debug.DrawRay(position,  _velocity * 2.0f, Color.green);
            Debug.DrawRay(position,  _rigidbody.velocity * 2.0f, Color.blue);
            Debug.DrawRay(position, _magnetVelocity * 2.0f, Color.red);
            Debug.DrawRay(_camera.transform.position, _camera.transform.forward * maxRange, Color.yellow);
        }
        #endregion
    
        #region Gun

        if (enableDebugGun)
        {
            if(IsAttracting)
                Debug.Log("Attracting");
            if(IsRepelling)
                Debug.Log("Repelling");
            if (targetHitVisualisation != null)
                targetHitVisualisation.transform.position = TargetPosition;
        }
        #endregion
        
        #region Stick
        if(enableDebugSticky)
        {
            if(_isSticked)
                Debug.Log("IsSticked");
            if(!_isSticked)
                Debug.Log("IsFree");
        }
        #endregion
        
        #region Jump
        if(enableDebugJump)
        {
            if(_isJumping)
                Debug.Log("Jumping");
            if(!_isJumping && !_isGrounded)
                Debug.Log("Falling");
            if(!_isJumping && _isGrounded)
                Debug.Log("On ground");
        }
        #endregion

        if (debugTextOutput != null)
        {
            debugTextOutput.text = $"IsGrounded ={_isGrounded}\n" +
                                   $"IsJumping ={_isJumping}\n" +
                                   "\n" +
                                   $"IsUsingGun ={IsUsingGun}\n" +
                                   $"IsAttracting = {IsAttracting}\n" +
                                   $"IsTryingToAttract = {_isTryingToAttract}\n" +
                                   $"AttractLock{ _attractLocked}\n" +
                                   $"IsRepelling = {IsRepelling}\n" +
                                   $"IsTryingToRepel = {_isTryingToRepel}\n" +
                                   $"RepelLock{ _repelLocked}\n" +
                                   "\n"+
                                   $"IsLookingAtObject = {_isLookingAtMagneticObject}\n+" +
                                   "\n"+
                                   $"Current Velocity = {_velocity}\n"+
                                   $"Magnetic Velocity = {_magnetVelocity}\n"+
                                   $"CurrentJumpVelocity = {currentJumpVelocity}\n"+
                                   $"Total Velocity = {_rigidbody.velocity}\n";
        }
    }

    void UpdateMagnetGunEffect()
    {
        if (_isLookingAtMagneticObject && ((_isTryingToAttract && !_attractLocked) || (_isTryingToRepel && !_repelLocked)))
        {
            if (!currentTarget.IsStatic) return;
            
            if (IsRepelling)
            {
                float acceleration = staticRepulsionAcceleration.Evaluate(currentTargetDistance / maxRange);
                _rigidbody.AddForce(-_camera.transform.forward * (acceleration * repulsionForceMagnet * Time.deltaTime), ForceMode.Force);     
            }
            else if (IsAttracting)
            {
                float acceleration = staticAttractionAcceleration.Evaluate(currentTargetDistance / maxRange);
                _rigidbody.AddForce(_camera.transform.forward * (acceleration * attractionForceMagnet * Time.deltaTime), ForceMode.Force);   
            }
                
            PlayVFX();
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if(other.gameObject.GetComponent<Magnetic>() == currentTarget)
            _isSticked = IsAttracting;
    }
}
