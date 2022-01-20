using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float pointToPointDuration = 4.0f;
    public float pauseDuration = 1.0f; 
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

    public Vector3[] waypoints = new Vector3[2];

    int sourceWaypoint;
    int destWaypoint;
    float currentDuration;

    enum PlatformState {
        Moving,
        Paused,
    };
    PlatformState currentState;

    // Start is called before the first frame update
    void Start()
    {
        if (waypoints.Length != 2) {
            Debug.LogError(
                "Invalid waypoints array length (" + waypoints.Length.ToString() +
                ") on " + gameObject.name);
        }

        sourceWaypoint = 0;
        destWaypoint = 1;

        currentDuration = 0f;
        currentState = PlatformState.Moving;

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

    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Gizmos.DrawLine(waypoints[0], waypoints[1]);
        Gizmos.DrawSphere(waypoints[0], 0.5f);
        Gizmos.DrawSphere(waypoints[1], 0.5f);
    }

    public void SetWaypoint(int waypoint)
    {
        waypoints[waypoint] = transform.position;
    }

    public void SnapToWaypoint(int waypoint)
    {
        transform.position = waypoints[waypoint];
    }
}
