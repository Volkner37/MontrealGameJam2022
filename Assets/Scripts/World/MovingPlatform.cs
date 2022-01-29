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
    [SerializeField]
    private float audioRampUpDownTime = 0.5f;

    [Header("")]
    [HideInInspector]
    [SerializeField]
    private bool _waypointsLocked;
    [HideInInspector]
    [SerializeField]
    private bool _usingRelativeWaypoints;
    [HideInInspector]
    [SerializeField]
    private Quaternion _origRotation = Quaternion.identity;

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


    private AudioSource _audioSource;
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

        GameObject audioSourcePrefab = (GameObject)Resources.Load("Prefabs/PlatformAudioSource", typeof(GameObject));
        GameObject audioSourceGO = Instantiate(audioSourcePrefab);
        audioSourceGO.transform.parent = transform;
        _audioSource = audioSourceGO.GetComponent<AudioSource>();

        sourceWaypoint = 0;
        destWaypoint = 1;
        // transform to world space if needed
        UseRelativeWaypoints(false);

        transform.position = waypoints[sourceWaypoint];
        currentDuration = 0f;
        StartMoving();
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

    void StartMoving()
    {
        _audioSource.Play();
        currentState = PlatformState.Moving;
    }

    void DoMove()
    {
        float ratio = Mathf.Min(currentDuration, pointToPointDuration) / pointToPointDuration;

        ratio = movementCurve.Evaluate(ratio);
        transform.position = Vector3.Lerp(waypoints[sourceWaypoint], waypoints[destWaypoint], ratio);

        float remaining = pointToPointDuration - currentDuration;
        if (currentDuration <= audioRampUpDownTime || remaining <= audioRampUpDownTime)
        {
            ratio = Mathf.Min(currentDuration, remaining) / audioRampUpDownTime;
            _audioSource.volume = ratio;
        }
        else
        {
            _audioSource.volume = 1.0f;
        }

        if (currentDuration >= pointToPointDuration) {
            currentDuration -= pointToPointDuration;

            // swap waypoints (more sophistication needed for >2 waypoints)
            int tempWaypoint = sourceWaypoint;
            sourceWaypoint = destWaypoint;
            destWaypoint = tempWaypoint;

            StopMoving();
        }
    }

    void StopMoving()
    {
        _audioSource.Stop();
        currentState = PlatformState.Paused;
    }

    void DoPause()
    {
        if (currentDuration >= pauseDuration) {
            currentDuration -= pauseDuration;

            StartMoving();
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
            if (_usingRelativeWaypoints)
                wB =
                    transform.position
                    + (transform.rotation * Quaternion.Inverse(_origRotation)) * wB;
            Gizmos.DrawSphere(wB, 0.5f);

            if (i == 0) continue;

            var wA = waypoints[i-1];
            if (_usingRelativeWaypoints)
                wA =
                    transform.position
                    + (transform.rotation * Quaternion.Inverse(_origRotation)) * wA;
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

        if (useRelative) {
            SnapToWaypoint(0);
            _origRotation = transform.rotation;
        }

        for (int i = 0; i < waypoints.Length; ++i)
        {
            if (useRelative) {
                // convert to relative
                waypoints[i] = waypoints[i] - transform.position;
            } else {
                // convert to world
                waypoints[i] =
                    transform.position
                    + transform.rotation * (Quaternion.Inverse(_origRotation)) * waypoints[i];
            }
        }
        _usingRelativeWaypoints = useRelative;
    }
}
