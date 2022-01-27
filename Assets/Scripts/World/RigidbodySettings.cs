using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodySettings : MonoBehaviour
{

    [SerializeField]
    private Vector3 angularVelocity = Vector3.zero;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.angularVelocity = angularVelocity;
    }
}
