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
    private Vector3 FullVelocity =>  _currentJumpVelocity + _currentGravity + _walkingVelocity;
    public Vector3 TargetPosition { get; private set; }
    private Vector3 _currentGravity = new Vector3();
    private Vector3 _currentJumpVelocity;
    private Vector3 _walkingVelocity;
    #endregion
    
    #region WallStick
    private bool _isSticked = false;
    #endregion

    #region Inputs
    private float _verticalAxis;
    private float _horizontalAxis;
    private bool _isJumping;
    private bool _canJump;
    
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

    #region Update
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
                _currentJumpVelocity = forceJump * Vector3.up;
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

    #endregion
    
    void FixedUpdate()
    {
        UpdateGrounded();
        UpdateStickStatus();
        
        //Update the velocity
        if(allowJumping)
            UpdateJumpVelocity();
        
        if(!IsUsingGun)
        {
            CalculateGravity();

            //Resetting some values
            TargetPosition = Vector3.zero;
            _isSticked = false;
            
            StopVFX();
            CalculateVelocityDecay();
            UpdateInputDirection();
        }
        else
        {
            _currentJumpVelocity = Vector3.zero;
            _repelLocked = IsAttracting;
            _attractLocked = IsRepelling;
            UpdateMagnetGunEffect();
        }
    
        //Limits the max speed of the overall velocity
        _rigidbody.velocity = Vector3.ClampMagnitude(FullVelocity, maxHorizontalSpeed);
        
        ShowDebug();
        
        _lastPosition = transform.position;
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
            _currentGravity = Vector3.zero;
        }
        else
            _magnetVelocity -= _magnetVelocity.normalized * (_magnetVelocityDecay * Time.deltaTime);
        
        //If we enter in a wall.
        if (GetCurrentSpeed() == 0)
        {
            _magnetVelocity = Vector3.zero;
        }

    }

    private float GetCurrentSpeed()
    {
        return (transform.position - _lastPosition).magnitude / Time.deltaTime;
         
    }

    
    
    private void CalculateGravity()
    {
        if (_isGrounded || _isSticked)
            _currentGravity = Vector3.zero;
        else
        {
            if (_currentGravity.y > -1 * maxVerticalSpeed)
                _currentGravity += Vector3.down * (gravity * Time.deltaTime);
            else
                _currentGravity = Vector3.down * (maxVerticalSpeed);    
        }
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

        _walkingVelocity.x = result.x;
        _walkingVelocity.y = 0;
        _walkingVelocity.z = result.z;

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
            _currentJumpVelocity = Vector3.up * forceJump;
            _isJumping = false;
        }
        
        //We decay the jump velocity with gravity
        _currentJumpVelocity += Vector3.down * (gravity * Time.deltaTime);
        if (_currentJumpVelocity.y < 0)
            _currentJumpVelocity = Vector3.zero;
    }

    private void ShowDebug()
    {
        #region DebugRays

        if (enableDebugRay)
        {
            var position = transform.position;
            Debug.DrawRay(position,  _walkingVelocity * 2.0f, Color.green);
            Debug.DrawRay(position,  _currentGravity * 2.0f, Color.magenta);
            Debug.DrawRay(position,  _currentJumpVelocity * 2.0f, Color.yellow);
            Debug.DrawRay(position, _magnetVelocity * 2.0f, Color.red);
            Debug.DrawRay(position, FullVelocity * 2.0f, Color.black);
            Debug.DrawRay(position,  _rigidbody.velocity * 2.0f, Color.blue);
            Debug.DrawRay(_camera.transform.position, _camera.transform.forward * maxRange, Color.white);
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
                                   $"Current Speed = {GetCurrentSpeed()}\n\n"+
                                   $"Magnetic Velocity = {_magnetVelocity}\n"+
                                   $"CurrentJumpVelocity = {_currentJumpVelocity}\n"+
                                   $"Current Gravity = {_currentGravity}\n"+
                                   $"Current Walking = {_walkingVelocity}\n"+
                                   $"Total Velocity = {FullVelocity}(Applied : {_rigidbody.velocity})\n";
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
                _magnetVelocity = -_camera.transform.forward * (acceleration * repulsionForceMagnet * Time.deltaTime);
                //_rigidbody.AddForce(-_camera.transform.forward * (acceleration * repulsionForceMagnet * Time.deltaTime), ForceMode.Force);     
            }
            else if (IsAttracting)
            {
                float acceleration = staticAttractionAcceleration.Evaluate(currentTargetDistance / maxRange);
                _magnetVelocity = _camera.transform.forward * (acceleration * repulsionForceMagnet * Time.deltaTime);
                //_rigidbody.AddForce(_camera.transform.forward * (acceleration * attractionForceMagnet * Time.deltaTime), ForceMode.Force);  
            }

            _magnetVelocity += Vector3.down * (gravity * Time.deltaTime);

            PlayVFX();
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if(other.gameObject.GetComponent<Magnetic>() == currentTarget)
            _isSticked = IsAttracting;
    }
}
