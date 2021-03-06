using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerV2 : MonoBehaviour
{
    #region Options

    [Header("Global")]
    [SerializeField] private AudioSource walkingSoundSource;
    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private AudioSource jumpSoundSource;
    [SerializeField] private AudioClip jumpingSound;
    [SerializeField] private AudioClip normalLandingSound;
    [SerializeField] private AudioClip metalLandingSound;

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
    [SerializeField] private AudioSource gunSoundSource;
    [SerializeField] private AudioSource impactSource;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private AudioClip gunSound;
    [SerializeField] private float gunVolumeFadeRatio = 1;
    [SerializeField] private float maxPitch = 2;
    [SerializeField] private float minPitch = 1;
    [SerializeField] private Transform defaultLookPosition;
    [SerializeField] private GameObject gunModel;
    [SerializeField] private Transform gunPosition;
    [SerializeField] private float maxRange;
    [SerializeField] private Transform gunTipTransform;
    [SerializeField] private float gunReplacePositionSpeed = 0.5f;
    [SerializeField] private Transform pickupPosition;
    [SerializeField] private GameObject magnet;
    [SerializeField] private VisualEffect _gunVfx;
    [Header("Attraction")]
    [SerializeField] private float attractionForceMagnet;
    [SerializeField] private AnimationCurve staticAttractionAcceleration;
    [SerializeField] private float dynamicObjectAttractionForce = 200;
    [SerializeField] private float minimumDistanceToStick = 0.5f;
    [Header("Repulsion")]
    [SerializeField] private float repulsionForceMagnet;
    [SerializeField] private AnimationCurve staticRepulsionAcceleration;
    [SerializeField] private float dynamicObjectRepulsionForce = 200;
    [Header("Pickup")] 
    [SerializeField] private float pickupDistance = 2.0f;
    [Header("Reticle")] 
    [SerializeField] private RawImage reticleImage;
    [SerializeField] private Color noTargetColor;
    [SerializeField] private Gradient colorDistanceGradient;

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
    private Vector3 initialMagnetLocalPosition;
    private Quaternion initialMagnetLocalRotation;
    private bool IsAttracting => _isTryingToAttract && IsLookingAtMagneticObject && _currentPickup == null;
    private bool IsRepelling => _isTryingToRepel && IsLookingAtMagneticObject && _currentPickup == null;
    private bool IsUsingGun => (IsRepelling || IsAttracting);
    #endregion
    
    #region States
    private bool IsGroundedOnMetal = false;
    private bool _isGrounded;
    private bool IsGrounded
    {
        get => _isGrounded;
        set
        {
            if (value && _isGrounded != true && _rigidbody.velocity.y <= -1)
            {
                jumpSoundSource.volume = Mathf.Clamp((_rigidbody.velocity.y + 1) / -10, 0, 1);
                jumpSoundSource.PlayOneShot(IsGroundedOnMetal ? metalLandingSound : normalLandingSound);
            }
            
            _isGrounded = value;
        }
    }
    
    private bool _needJumping;
    private bool _isJumping;
    private bool _isSticked;
    private bool IsSticked
    {
        get => _isSticked;
        set
        {
            if (value && _isSticked != true)
            {
                impactSource.PlayOneShot(impactSound);
                magnet.transform.parent = currentTarget.transform;
                magnet.transform.position = _currentTargetPosition;
                magnet.transform.right = _currentTargetNormal;
                magnet.transform.localScale = new Vector3(15, 15,15);
            }

            if (!value && _isSticked != false)
            {
                magnet.transform.parent = gunModel.transform.GetChild(0);
                magnet.transform.localScale = new Vector3(100, 100,100);
            }

            Debug.DrawRay(_gunVfx.transform.position, _gunVfx.transform.forward, Color.red);

            //For magnet visibility in player camera
            magnet.layer = value ? 0 : 6;

            _isSticked = value; 
        }
    }
    private bool _isOnPlatform;
    
    #endregion

    #region Target
    private bool IsLookingAtMagneticObject => currentTarget != null;
    private float _currentTargetDistance;
    public Vector3 _currentTargetPosition = Vector3.zero;
    public Vector3 _currentTargetNormal =  Vector3.zero;
    public Magnetic currentTarget = null;
    private GameObject _currentPickup = null;

    

    #endregion
    
    #region Others
    private Camera _camera;
    private Rigidbody _rigidbody;
    private GameObject _currentParent;
    #endregion

    private void Awake()
    {
        initialMagnetLocalPosition = magnet.transform.localPosition;
        initialMagnetLocalRotation = magnet.transform.localRotation;
    }

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        _rigidbody = GetComponent<Rigidbody>();
        _gunVfx.enabled = true;

        //Sounds
        walkingSoundSource.clip = walkingSound;
        walkingSoundSource.loop = true;
        jumpSoundSource.clip = jumpingSound;
        jumpSoundSource.loop = false;
        gunSoundSource.loop = true;
        gunSoundSource.clip = gunSound;
        gunSoundSource.volume = 0;
        gunSoundSource.Play();

        StartCoroutine(nameof(ControlGunVolume));
    }

    public void MuteAllSounds(bool mute)
    {
        Debug.Log("MuteAllSounds");
        
        foreach (AudioSource audioSource in GetComponents<AudioSource>())
        {
            if (mute)
                audioSource.Pause();
            else
                audioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInputs();
        CheckForMagneticObject();
        AnimateGun();
        UpdateReticle();

        if (!IsSticked)
        {
            magnet.transform.localPosition = Vector3.Slerp(magnet.transform.localPosition, initialMagnetLocalPosition, 0.2f);
            magnet.transform.localRotation = Quaternion.Slerp(magnet.transform.localRotation, initialMagnetLocalRotation, 0.2f);
        }
    }
    
    private IEnumerator ControlGunVolume()
    {
        while (true)
        {
            if ((IsRepelling || IsAttracting || _currentPickup != null) && !IsSticked)
            {
                //Set volume back to 100
                if(gunSoundSource.volume < 100)
                    gunSoundSource.volume += gunVolumeFadeRatio * Time.deltaTime;

                //Set pitch
                if (IsRepelling && gunSoundSource.pitch <= maxPitch)
                {
                    gunSoundSource.pitch += gunVolumeFadeRatio * Time.deltaTime;
                }
                if (IsAttracting && gunSoundSource.pitch >= minPitch)
                {
                    gunSoundSource.pitch -= gunVolumeFadeRatio * Time.deltaTime;
                }
            }
            else
            {
                //Set Volume back to 0
                if(gunSoundSource.volume > 0)
                    gunSoundSource.volume -= gunVolumeFadeRatio * Time.deltaTime;
                
                //Set pitch to center point
                if (gunSoundSource.pitch >=minPitch + ((maxPitch-minPitch)/2))
                {
                    gunSoundSource.pitch -= gunVolumeFadeRatio * Time.deltaTime;
                }
                if (gunSoundSource.pitch <=minPitch + ((maxPitch-minPitch)/2))
                {
                    gunSoundSource.pitch += gunVolumeFadeRatio * Time.deltaTime;
                }
            }

            if (gunSoundSource.pitch < minPitch)
                gunSoundSource.pitch = minPitch;
            if (gunSoundSource.pitch > maxPitch)
                gunSoundSource.pitch = maxPitch;
            
            yield return null;
        }
    }

    private void UpdateReticle()
    {
        if (_currentTargetDistance > 0 && _currentTargetDistance <= maxRange)
            reticleImage.material.color = colorDistanceGradient.Evaluate(_currentTargetDistance / maxRange);
        else
            reticleImage.material.color = noTargetColor;
    }

    private void UpdatePickupPosition()
    {
        if (_currentPickup == null) return;
        
        Vector3 direction = pickupPosition.position - _currentPickup.transform.position;
        _currentPickup.GetComponent<Rigidbody>().AddForce(direction * 200, ForceMode.Force);
    }

    private void AnimateGun()
    {
        if (!IsSticked)
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
                _currentTargetNormal = hit.normal;
                return;
            }
            
            currentTarget = null;
            _currentTargetPosition = Vector3.zero;
            _currentTargetDistance = 0;
            _currentTargetNormal = Vector3.zero;
        }
        
        currentTarget = null;
        _currentTargetDistance = 0;
        _currentTargetPosition = Vector3.zero;    
    }

    private void UpdateInputs()
    {
        _verticalAxis = Input.GetAxis("Vertical");
        _horizontalAxis = Input.GetAxis("Horizontal");
        _isTryingToAttract = Input.GetMouseButton(0) && !Input.GetMouseButton(1);
        _isTryingToRepel = Input.GetMouseButton(1) && !Input.GetMouseButton(0);
    
        if (Input.GetMouseButtonUp(0))
        {
            _isTryingToAttract = false;
            IsSticked = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            _isTryingToRepel = false;
        }
        if (allowJumping && !_needJumping && IsGrounded)
        {
            _needJumping = Input.GetKeyDown(KeyCode.Space);
        }

        bool pickedUpSomething = false;
        if (Input.GetKeyDown(KeyCode.E) || _isTryingToAttract)
        {
            if (_currentTargetDistance <= pickupDistance && currentTarget != null && !currentTarget.IsStatic && _currentPickup == null)
            {
                pickedUpSomething = true;
                PickUp();
            }
        }
        
        if(!pickedUpSomething && (Input.GetKeyDown(KeyCode.E) || _isTryingToRepel))
        {
            Drop();
        }
    }

    private void Drop()
    {
        if (_currentPickup == null) return;
        
        Rigidbody objRigidBody = _currentPickup.GetComponent<Rigidbody>();
        objRigidBody.useGravity = true;
        objRigidBody.drag = 0;
        _currentPickup = null;
    }

    private void PickUp()
    {
        if (_currentPickup != null) return;
        
        _currentPickup = currentTarget.gameObject;
        Rigidbody objRigidBody = _currentPickup.GetComponent<Rigidbody>();
        objRigidBody.useGravity = false;    
        objRigidBody.drag = 15;
    }

    private void FixedUpdate()
    {
        UpdateGrounded();
        UpdateInputDirection();
        UpdateStickStatus();
        UpdatePickupPosition();
        SetGunLock();

        //Check for jump
        if (_needJumping)
        {
            _needJumping = false;
            _rigidbody.AddForce(transform.up * jumpForce);
            if(jumpingSound != null)
                jumpSoundSource.PlayOneShot(jumpingSound);
        }

        if (IsUsingGun)
        {
            UpdateMagnetGunEffect();
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

        if (!walkingSoundSource.isPlaying && !IsUsingGun && _playerVelocity != Vector3.zero && IsGrounded)
        {
            walkingSoundSource.Play();
        }
        else if(walkingSoundSource.isPlaying && ((_playerVelocity == Vector3.zero && IsGrounded) || IsUsingGun))
        {
            walkingSoundSource.Pause();
        }

        ShowDebug();
    }

    private void UpdateInputDirection()
    {
        _playerVelocity = Vector3.zero;

        //Accelerations
        if (_verticalAxis >= 0)
            _verticalAxis *= forwardSpeed;
        else if (_verticalAxis <= 0)
            _verticalAxis *= backwardSpeed;
        
        _horizontalAxis *= sideSpeed;

        if (!IsGrounded)
        {
            _verticalAxis *= airControlRatio;
            _horizontalAxis *= airControlRatio;
        }

        float maxDiagonalSpeed = Mathf.Max(forwardSpeed, backwardSpeed, sideSpeed);

        //This prevents to move faster in diagonal

        Vector3 xCameraAxes = new Vector3(_camera.transform.right.x, 0, _camera.transform.right.z).normalized;
        Vector3 ZCameraAxes = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;

        //Edge case of when looking up of down
        if (ZCameraAxes == Vector3.zero)
        {
            ZCameraAxes = _camera.transform.up * -1;

            if (_camera.transform.forward.normalized == Vector3.down)
                ZCameraAxes *= -1;
        }
        
        Vector3 result = Vector3.ClampMagnitude(xCameraAxes * _horizontalAxis + ZCameraAxes * _verticalAxis, maxDiagonalSpeed);
        
        
        _playerVelocity.x = result.x;
        _playerVelocity.y = 0;
        _playerVelocity.z = result.z;
    }

    private void UpdateGrounded()
    {
        int layerMask = ~LayerMask.GetMask("Player");
        RaycastHit hit;
        bool grounded = Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, 1.3f, layerMask);
        
        //Check for moving platform
        if (grounded)
        {
            bool result = (hit.transform.parent != null && hit.transform.GetComponentInParent<MovingPlatform>() != null);
            if (result == false)
                result = hit.transform.TryGetComponent<MovingPlatform>(out _);
            
            if (!IsSticked)
            {
                ChangeParent(result ? hit.transform.gameObject : null);
            }
            
            IsGroundedOnMetal = hit.transform.TryGetComponent<Magnetic>(out _);
        }
        else
        {
            IsGroundedOnMetal = false;
        }

        IsGrounded = grounded;
    }

    private void UpdateStickStatus()
    {
        _rigidbody.constraints = IsSticked ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void SetGunLock()
    {
        gunModel.transform.SetParent(IsSticked ? transform : gunPosition);
    }

    private void ShowDebug()
    {
        Debug.DrawRay(_camera.transform.position, _camera.transform.forward * maxRange, Color.red);

        if (debugTextOutput != null)
        {
            debugTextOutput.text = $"IsGrounded ={IsGrounded}\n" +
                                   $"IsGroundedOnMetal ={IsGroundedOnMetal}\n" +
                                   $"IsJumping ={_needJumping}\n" +
                                   "\n" +
                                   $"GunVolume = {gunSoundSource.volume}\n" +
                                   $"GunPitch = {gunSoundSource.pitch}/{minPitch + ((maxPitch-minPitch)/2)}\n" +
                                   "\n" +
                                   $"IsUsingGun ={IsUsingGun}\n" +
                                   $"IsAttracting = {IsAttracting}\n" +
                                   $"IsTryingToAttract = {_isTryingToAttract}\n" +
                                   //$"AttractLock{ _attractLocked}\n" +
                                   $"IsRepelling = {IsRepelling}\n" +
                                   $"IsTryingToRepel = {_isTryingToRepel}\n" +
                                   $"IsLookingAtMagneticObject = {IsLookingAtMagneticObject}\n\n" +
                                   //$"RepelLock{ _repelLocked}\n" +
                                   $"CurrentTarget = {currentTarget?.name ?? "None"}\n" +
                                   $"Target Distance = {_currentTargetDistance}\n" +
                                   $"Pickup = {_currentPickup?.name ?? "None"}\n" +
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
            if (currentTarget.IsStatic)
            {
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
            else
            {
                
                Vector3 direction = (transform.position - currentTarget.transform.position).normalized;
                float force;
                
                if (IsRepelling)
                {
                    force = staticRepulsionAcceleration.Evaluate(_currentTargetDistance / maxRange) * dynamicObjectRepulsionForce;
                    direction *= -1;
                }
                else //Attracting
                {
                    force = staticAttractionAcceleration.Evaluate(_currentTargetDistance / maxRange) * dynamicObjectAttractionForce;
                }
                
                currentTarget.GetComponent<Rigidbody>().AddForce(direction * force);
            }
        }
    }
    
    private void PlayVFX()
    {
        _gunVfx.SetVector3("Position", gunTipTransform.position);
        _gunVfx.SetFloat("MaxDistance", _currentTargetDistance);
        _gunVfx.SetBool("ColorBool", _isTryingToAttract);
        _gunVfx.Play();
    }

    private void StopVFX()
    {
        _gunVfx.Stop();
    }

    private void ChangeParent(GameObject otherGameObject)
    {
        if (otherGameObject != null)
        {
            _currentParent = otherGameObject;
            transform.SetParent(_currentParent.transform);
            _isOnPlatform = true;
        }
        else
        {
            if (_currentParent != null)
            {
                _currentParent.transform.DetachChildren();
                _currentParent = null;
                _isOnPlatform = false;
            }
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        //Check for stick or pickup
        if (other.gameObject.TryGetComponent(out Magnetic magneticResult))
        {
            if (magneticResult == currentTarget)
            {
                if (currentTarget.IsStatic)
                {
                    if(_currentTargetDistance <= minimumDistanceToStick)
                        IsSticked = IsAttracting;

                    if (IsSticked)
                    {
                        if (other.gameObject != _currentParent)
                        {
                            ChangeParent(other.gameObject);
                        }
                    }
                }
                else
                {
                    if (_isTryingToAttract)
                    {
                        PickUp();
                        _rigidbody.velocity = Vector3.zero;
                    }
                }
            }
        }
        
        
        //Check for moving platform
        bool result = other.gameObject?.transform?.parent?.TryGetComponent<MovingPlatform>(out _) ?? false;
        if (result == false)
            result = other.gameObject.TryGetComponent<MovingPlatform>(out _);

        if (result)
        {
            ChangeParent(other.gameObject);
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if(other.gameObject.GetComponent<Magnetic>() == currentTarget && _currentTargetDistance <= minimumDistanceToStick)
            IsSticked = IsAttracting;
    }

    private void OnCollisionExit(Collision other)
    {
        bool result = other.gameObject?.transform?.parent?.TryGetComponent<MovingPlatform>(out _) ?? false;
        if (result == false)
            result = other.gameObject.TryGetComponent<MovingPlatform>(out _);
        
        if (_isOnPlatform && other.gameObject == _currentParent && result)
        {
            ChangeParent(null);
        }
    }
}
