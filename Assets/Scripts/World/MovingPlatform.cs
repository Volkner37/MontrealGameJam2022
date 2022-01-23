using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Tuning")]
    [SerializeField]
    private float pointToPointDuration = 4.0f;
    [SerializeField]
    private float pauseDuration = 1.0f;
    [SerializeField]
    private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

    [Header("")]
    [HideInInspector]
    [SerializeField]
    private bool _waypointsLocked;
    [HideInInspector]
    [SerializeField]
    private bool _usingRelativeWaypoints;
    public bool WaypointsLocked
    {
        get => _waypointsLocked;
        set
        {
            UseRelativeWaypoints(value);
            _waypointsLocked = value;
        }
    }

    [Header("Movement Path")]
    [SerializeField]
    private Vector3[] waypoints = new Vector3[2];

    private int sourceWaypoint;
    private int destWaypoint;
    private float currentDuration;

    private enum PlatformState {
        Moving,
        Paused,
    };
    private PlatformState currentState;

    // Start is called before the first frame update
    void Start()
    {
        if (!WaypointsLocked)
        {
            Debug.LogWarning("Waypoints not locked", gameObject);
        }

        if (waypoints.Length != 2)
        {
            Debug.LogError(
                "Invalid waypoints array length (" + waypoints.Length.ToString() +
                ")", gameObject);
        }

        sourceWaypoint = 0;
        destWaypoint = 1;

        currentDuration = 0f;
        currentState = PlatformState.Moving;

        // transform to world space if needed
        UseRelativeWaypoints(false);

        transform.position = waypoints[sourceWaypoint];
    }

    void FixedUpdate()
    {
        currentDuration += Time.deltaTime;

        switch (currentState)
        {
            case PlatformState.Moving:
                DoMove();
                break;
            case PlatformState.Paused:
                DoPause();
                break;
        }
    }

    void DoMove()
    {
        float ratio = Mathf.Min(currentDuration, pointToPointDuration) / pointToPointDuration;
        ratio = movementCurve.Evaluate(ratio);
        transform.position = Vector3.Lerp(waypoints[sourceWaypoint], waypoints[destWaypoint], ratio);

        if (currentDuration >= pointToPointDuration) {
            currentDuration -= pointToPointDuration;

            // swap waypoints (more sophistication needed for >2 waypoints)
            int tempWaypoint = sourceWaypoint;
            sourceWaypoint = destWaypoint;
            destWaypoint = tempWaypoint;

            currentState = PlatformState.Paused;
        }
    }

    void DoPause()
    {
        if (currentDuration >= pauseDuration) {
            currentDuration -= pauseDuration;
            currentState = PlatformState.Moving;
        }
    }

    // Editor

    void OnDrawGizmos()
    {
        if (!WaypointsLocked)
        {
            Gizmos.DrawIcon(transform.position + new Vector3(1, 1, 1), "warning.png", true);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        for (int i = 0; i < waypoints.Length; ++i)
        {
            var wB = waypoints[i];
            if (_usingRelativeWaypoints) { wB = transform.position + wB; }
            Gizmos.DrawSphere(wB, 0.5f);

            if (i == 0) { continue; }

            var wA = waypoints[i-1];
            if (_usingRelativeWaypoints)
            {
                wA = transform.position + wA;
            }
            Gizmos.DrawLine(wA, wB);
        }
    }

    public int GetWaypointsLength()
    {
        return waypoints.Length;
    }

    public void SetWaypoint(int waypoint)
    {
        waypoints[waypoint] = transform.position;
    }

    private void SnapToWaypoint(int waypoint)
    {
        transform.position = waypoints[waypoint];
    }

    private void UseRelativeWaypoints(bool useRelative)
    {
        if (useRelative == _usingRelativeWaypoints)
            return;

        if (useRelative)
            SnapToWaypoint(0);

        for (int i = 0; i < waypoints.Length; ++i)
        {
            if (useRelative) {
                // convert to relative
                waypoints[i] = waypoints[i] - transform.position;
            } else {
                // convert to world
                waypoints[i] = transform.position + waypoints[i];
            }
        }
        _usingRelativeWaypoints = useRelative;
    }
}
