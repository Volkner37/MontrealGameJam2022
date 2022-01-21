using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHandler : MonoBehaviour
{
    [SerializeField] 
    private float horizontalSpeed;
    [SerializeField] 
    private float verticalSpeed;
    
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private Camera _camera;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * horizontalSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSpeed;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        _camera.transform.eulerAngles = new Vector3(xRotation, yRotation, 0.0f);
    }
}
