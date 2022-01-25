using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MagneticField : MonoBehaviour
{

    [SerializeField]
    private float magnitude = 10.0f;
    [SerializeField]
    private bool repels = true;

    private Vector3 _forceDirection;
    private Collider _zone;
    private Vector3 _bottom;
    private Plane _source;

    private List<Rigidbody> _affected = new List<Rigidbody>();

    private void Start() {

        _forceDirection = transform.up * (repels ? 1 : -1);

        _zone = GetComponent<Collider>();
        _zone.isTrigger = true;

        Vector3 center = Vector3.zero;
        Vector3 extents = Vector3.one * 0.5f;
        _bottom = transform.TransformPoint(new Vector3(0, -extents.y, 0));
        _source = new Plane(_forceDirection, _bottom); // a point on the bottom plane
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out Magnetic m) && other.TryGetComponent(out Rigidbody rb))
            _affected.Add(rb);
    }

    private void OnTriggerExit(Collider other) {
        if (other.TryGetComponent(out Rigidbody rb))
            _affected.Remove(rb);
    }

    private void FixedUpdate() {
        foreach (var rb in _affected)
        {
            // Caveat: Currently larger objects suffer from reduced magnetism since
            // their pivot (read: center of mass) is further away from the source of
            // the magnetism. Consider calculating closest point instead.
            // Interesting exercise: Can we AddForceAtPosition for a sample of points
            // in the object? This could better simulate magnetism by adding rotation
            // to the object based on proximity of edges to the magnet.
            float delta = _source.GetDistanceToPoint(rb.transform.position);
            float force = 1f / (delta * delta) * magnitude;
            rb.AddForce(_forceDirection * force);
        }
    }

    private void OnDrawGizmos() {
        foreach (var rb in _affected)
        {
            Vector3 closestPoint = _source.ClosestPointOnPlane(rb.transform.position);
            Gizmos.DrawLine(closestPoint, closestPoint + _forceDirection * (repels ? 1 : -1));
            Gizmos.DrawLine(rb.transform.position, rb.transform.position + rb.velocity);
        }
    }
}
