using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] 
    private float forwardAcceleration = 1;
    [SerializeField] 
    private float backwardAcceleration = 1;
    [SerializeField] 
    private float sideAcceleration = 1;
    [SerializeField] 
    private float forwardDeceleration = 1;
    [SerializeField] 
    private float sideDeceleration = 1;
    [SerializeField]
    public float gravity = 9.8f;
    [SerializeField] 
    private float maxSpeed = 1;

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
        Debug.Log(maxDiagonalSpeed);
        
        _inputDirection = Vector3.ClampMagnitude(_camera.transform.right * horizontal + _camera.transform.forward * vertical, maxDiagonalSpeed);

        _dampeningDirection = _rigidbody.velocity * (-1 * forwardDeceleration);
        
        //Applying forces
        _rigidbody.AddForce((_inputDirection + _dampeningDirection) * Time.deltaTime, ForceMode.VelocityChange);
        _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxSpeed);
        
    #region DebugRays
        Debug.DrawRay(transform.position,  _inputDirection * 2.0f, Color.green);
        Debug.DrawRay(transform.position,  _rigidbody.velocity * 2.0f, Color.blue);
        Debug.DrawRay(transform.position, _dampeningDirection, Color.red);
    #endregion
        
    }
}
