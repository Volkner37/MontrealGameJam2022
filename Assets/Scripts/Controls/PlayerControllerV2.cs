using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerV2 : MonoBehaviour
{
    #region Options

    [Header("Global")]

    [Header("Jump")] 
    [SerializeField] private bool allowJumping = true;
    [SerializeField] private float jumpForce;

    [Space] 
    [Header("Walking")] 
    [SerializeField] private float forwardSpeed = 10;
    [SerializeField] private float backwardSpeed = 10;
    [SerializeField] private float sideSpeed = 10;
    [SerializeField] private float airControlRatio = 0.3f;

    [Space] [Header("Gun Settings")] 
    [Header("Shared")] 
    [SerializeField] private Transform defaultLookPosition;
    [SerializeField] private GameObject gunModel;
    [SerializeField] private Transform gunPosition;
    [SerializeField] private float maxRange;
    [SerializeField] private Transform gunTipTransform;
    [SerializeField] private float gunReplacePositionSpeed = 0.5f;
    [Header("Attraction")]
    [SerializeField] private float attractionForceMagnet;
    [SerializeField] private AnimationCurve staticAttractionAcceleration;
    [Header("Repulsion")]
    [SerializeField] private float repulsionForceMagnet;
    [SerializeField] private AnimationCurve staticRepulsionAcceleration;
    
    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugTextOutput;

    #endregion
    
    #region Velocities
    private Vector3 _magneticVelocity = Vector3.zero;
    private Vector3 _playerVelocity = Vector3.zero;
    private Vector3 _jumpVelocity = Vector3.zero;
    #endregion
    
    #region Inputs
    private float _verticalAxis;
    private float _horizontalAxis;
    private bool _isTryingToAttract;
    private bool _isTryingToRepel;
    #endregion
    
    #region Gun
    private bool _repelLocked = false; 
    private bool _attractLocked = false;
    private bool IsAttracting => _isTryingToAttract && IsLookingAtMagneticObject;
    private bool IsRepelling => _isTryingToRepel && IsLookingAtMagneticObject;
    private bool IsUsingGun => (IsRepelling || IsAttracting);
    #endregion
    
    #region States
    private bool _isGrounded;
    private bool _needJumping;
    private bool _isJumping;
    private bool _isSticked;
    private bool _isOnPlatform;
    #endregion

    #region Target
    private bool IsLookingAtMagneticObject => currentTarget != null;
    private float _currentTargetDistance;
    public Vector3 _currentTargetPosition = Vector3.zero;
    public Magnetic currentTarget = null;
    #endregion
    
    #region Others
    private Camera _camera;
    private Rigidbody _rigidbody;
    private GameObject _currentParent;
    #endregion

    #region Effects
    private VisualEffect _gunVfx;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        _rigidbody = GetComponent<Rigidbody>();
        _gunVfx = GetComponentInChildren<VisualEffect>();
        _gunVfx.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInputs();
        CheckForMagneticObject();
        AnimateGun();
    }

    private void AnimateGun()
    {
        if (!_isSticked)
        {
            gunModel.transform.localPosition = Vector3.Slerp(gunModel.transform.localPosition, Vector3.zero, gunReplacePositionSpeed * Time.deltaTime);
            gunModel.transform.localRotation = Quaternion.Slerp(gunModel.transform.localRotation,Quaternion.identity, gunReplacePositionSpeed * Time.deltaTime);

            //TODO : fix the gun rotation 
            // gunModel.transform.localRotation = Quaternion.Slerp(gunModel.transform.localRotation,
            //     Quaternion.LookRotation(_currentTargetPosition - transform.position),
            //     gunReplacePositionSpeed * Time.deltaTime);
        }
    }

    private void CheckForMagneticObject()
    {
        //Without this, the player could block the raycast.
        int layerMask = ~LayerMask.GetMask("Player");
        RaycastHit hit;
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, maxRange, layerMask))
        {
            if (hit.transform.TryGetComponent(out currentTarget))
            {
                _currentTargetPosition = hit.point;
                _currentTargetDistance = hit.distance;
                return;
            }
            
            currentTarget = null;
            _currentTargetPosition = Vector3.zero;
            _currentTargetDistance = 0;
        }
        
        currentTarget = null;
        _currentTargetDistance = 0;
        _currentTargetPosition = Vector3.zero;    
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
            _isSticked = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            _isTryingToRepel = false;
        }
        if (allowJumping && !_needJumping && _isGrounded)
        {
            _needJumping = Input.GetKeyDown(KeyCode.Space);
        }
    }

    private void FixedUpdate()
    {
        UpdateGrounded();
        UpdateInputDirection();
        UpdateStickStatus();
        SetGunLock();
        
        //Check for jump
        if (_needJumping)
        {
            _needJumping = false;
            _rigidbody.AddForce(transform.up * jumpForce);
        }

        if (IsUsingGun)
        {
            UpdateMagnetGunEffect();
            if(!_isSticked)
                PlayVFX();
        }
        else
        {
            _magneticVelocity = Vector3.zero;
            StopVFX();
        }

        Vector3 velocity = _playerVelocity + _jumpVelocity;

        if (_rigidbody.velocity.magnitude >= forwardSpeed || IsUsingGun)
        {
            _rigidbody.AddForce(_playerVelocity + _jumpVelocity, ForceMode.Acceleration);
            velocity.x = _rigidbody.velocity.x;
            velocity.z = _rigidbody.velocity.z;
        }
        
        //Force gravity
        _rigidbody.useGravity = !IsUsingGun;
        velocity.y = _rigidbody.velocity.y;

        _rigidbody.velocity = velocity;
        
        ShowDebug();
    }
    
    private void UpdateInputDirection()
    {
        //Accelerations
        if (_verticalAxis >= 0)
            _verticalAxis *= forwardSpeed;
        else if (_verticalAxis <= 0)
            _verticalAxis *= backwardSpeed;
    
        _horizontalAxis *= sideSpeed;

        if (!_isGrounded)
        {
            _verticalAxis *= airControlRatio;
            _horizontalAxis *= airControlRatio;
        }

        float maxDiagonalSpeed = Mathf.Max(forwardSpeed, backwardSpeed, sideSpeed);

        //This prevents to move faster in diagonal

        Vector3 xCameraAxes = new Vector3(_camera.transform.right.x, 0, _camera.transform.right.z).normalized;
        Vector3 ZCameraAxes = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        
        Vector3 result = Vector3.ClampMagnitude(xCameraAxes * _horizontalAxis + ZCameraAxes * _verticalAxis, maxDiagonalSpeed);

        _playerVelocity.x = result.x;
        _playerVelocity.y = 0;
        _playerVelocity.z = result.z;
    }

    private void UpdateGrounded()
    {
        int layerMask = ~LayerMask.GetMask("Player");
        _isGrounded = Physics.Raycast(transform.position, new Vector3(0, -1, 0), out _, 1.1f, layerMask);
    }

    private void UpdateStickStatus()
    {
        _rigidbody.constraints = _isSticked ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void SetGunLock()
    {
        if (_isSticked)
        {
            gunModel.transform.SetParent(transform);
        }
        else
        {
            gunModel.transform.SetParent(gunPosition);
            SetParent(null);
        }
    }

    private void ShowDebug()
    {
        Debug.DrawRay(_camera.transform.position, _camera.transform.forward * maxRange, Color.red);

        if (debugTextOutput != null)
        {
            debugTextOutput.text = $"IsGrounded ={_isGrounded}\n" +
                                   $"IsJumping ={_needJumping}\n" +
                                   "\n" +
                                   $"IsUsingGun ={IsUsingGun}\n" +
                                   $"IsAttracting = {IsAttracting}\n" +
                                   $"IsTryingToAttract = {_isTryingToAttract}\n" +
                                   //$"AttractLock{ _attractLocked}\n" +
                                   $"IsRepelling = {IsRepelling}\n" +
                                   $"IsTryingToRepel = {_isTryingToRepel}\n" +
                                   $"IsLookingAtMagneticObject = {IsLookingAtMagneticObject}\n" +
                                   //$"RepelLock{ _repelLocked}\n" +
                                   "\n"+
                                   //$"IsLookingAtObject = {_isLookingAtMagneticObject}\n+" +
                                   "\n"+
                                   //$"Current Speed = {GetCurrentSpeed()}\n\n"+
                                   $"Current Target Position = {_currentTargetPosition}\n"+
                                   $"Magnetic Velocity = {_magneticVelocity}\n"+
                                   $"CurrentJumpVelocity = {_jumpVelocity}\n"+
                                   $"Current Walking = {_playerVelocity}\n"+
                                   $"Total Velocity = {_playerVelocity + _jumpVelocity + _magneticVelocity}=={_rigidbody.velocity}\n";
        }
    }
    
    void UpdateMagnetGunEffect()
    {
        Vector3 beforeVelocity = _rigidbody.velocity;
        
        if (IsLookingAtMagneticObject && ((_isTryingToAttract && !_attractLocked) || (_isTryingToRepel && !_repelLocked)))
        {
            if (!currentTarget.IsStatic) return;
            
            if (IsRepelling)
            {
                float force = staticRepulsionAcceleration.Evaluate(_currentTargetDistance / maxRange);
                _magneticVelocity = -_camera.transform.forward * (force * repulsionForceMagnet);
            }
            else if (IsAttracting)
            {
                float force = staticAttractionAcceleration.Evaluate(_currentTargetDistance / maxRange);
                _magneticVelocity = _camera.transform.forward * (force * attractionForceMagnet);
            }
            
            _rigidbody.AddForce(_magneticVelocity);
        }
    }
    
    private void PlayVFX()
    {
        _gunVfx.SetVector3("origin", gunTipTransform.transform.position);
        _gunVfx.SetVector3("target", _currentTargetPosition);

        _gunVfx.Play();
    }

    private void StopVFX()
    {
        _gunVfx.Stop();
    }

    private void SetParent(GameObject otherGameObject)
    {
        if (otherGameObject != null)
        {
            _currentParent = otherGameObject;
            transform.SetParent(_currentParent.transform);
            _isOnPlatform = true;
            Debug.Log("attaching");
        }
        else
        {
            if (_currentParent != null)
            {
                _currentParent.transform.DetachChildren();
                _currentParent = null;
                _isOnPlatform = false;
                Debug.Log("detaching");
            }
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.GetComponent<Magnetic>() == currentTarget)
            _isSticked = IsAttracting;

        bool result = other.gameObject?.transform?.parent?.TryGetComponent<MovingPlatform>(out _) ?? false;
        if (result == false)
            result = other.gameObject.TryGetComponent<MovingPlatform>(out _);

        if (result)
        {
            SetParent(other.gameObject);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (_isOnPlatform && other.gameObject == _currentParent && other.gameObject.TryGetComponent<MovingPlatform>(out _))
        {
            SetParent(null);
        }
    }
}
