using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class GunAnimation : MonoBehaviour
{
    [SerializeField] 
    private int animationSpeed = 2;
    [SerializeField] 
    private Transform defaultLookPosition;
    private PlayerController _controller;

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion targetRotation;

        if (_controller.TargetPosition != Vector3.zero && Vector3.Distance(transform.position, _controller.TargetPosition) >= 2)
            targetRotation = Quaternion.LookRotation(_controller.TargetPosition - transform.position);
        else
            targetRotation = Quaternion.LookRotation(defaultLookPosition.position - transform.position);
    
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, animationSpeed * Time.deltaTime);
    }
}