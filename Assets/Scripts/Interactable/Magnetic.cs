using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnetic : MonoBehaviour
{
    public bool IsStatic {get; private set;}

    private void Start()
    {
        IsStatic = GetComponent<Rigidbody>() == null;
    }
}
