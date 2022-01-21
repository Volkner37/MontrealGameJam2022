using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTester : MonoBehaviour
{
    private Rigidbody _rigidbody;

    [SerializeField] private float force;
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _rigidbody.AddForce(force * Vector3.up, ForceMode.Impulse);
            Debug.Log("FORCE");
        }

        
    }
}
