using UnityEngine;

namespace Controls
{
    public class MouseHandler : MonoBehaviour
    {
        [SerializeField]
        [Range(0.5f, 10.0f)]
        private float horizontalSpeed;
        [SerializeField] 
        [Range(0.5f, 10.0f)]
        private float verticalSpeed;
    
        private float _xRotation = 0.0f;
        private float _yRotation = 0.0f;
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

            _yRotation += mouseX;
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90, 90);

            _camera.transform.eulerAngles = new Vector3(_xRotation, _yRotation, 0.0f);
        }
    }
}
