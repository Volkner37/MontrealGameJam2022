using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : AbstractInteractable
{
    [Header("Movement Tuning")]
    [SerializeField]
    private float pointToPointDuration = 4.0f;
    [SerializeField]
    private float pauseDuration = 1.0f;
    [SerializeField]
    private bool onAtStart = true;
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
        Off,
        Moving,
        Paused,
    };
    private PlatformState currentState = PlatformState.Off;
    private PlatformState previousState = PlatformState.Moving;

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
        audioSourceGO.transform.localPosition = Vector3.zero;
        _audioSource = audioSourceGO.GetComponent<AudioSource>();

        sourceWaypoint = 0;
        destWaypoint = 1;
        // transform to world space if needed
        UseRelativeWaypoints(false);

        transform.position = waypoints[sourceWaypoint];
        currentDuration = 0f;

        if (onAtStart)
            Resume();
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case PlatformState.Off:
                OffState();
                break;
            case PlatformState.Moving:
                MovingState();
                break;
            case PlatformState.Paused:
                PausedState();
                break;
        }
    }

    // State transitions
    void Resume()
    {
        _audioSource.Play(); // restarts clip
        currentState = PlatformState.Moving;
    }

    void Pause()
    {
        currentState = PlatformState.Paused;
    }

    void TurnOff()
    {
        if (currentState == PlatformState.Off)
            return;

        Debug.Log("TurnOff: " + previousState);
        previousState = currentState;
        currentState = PlatformState.Off;
    }

    void TurnOn()
    {
        if (currentState != PlatformState.Off)
            return;

        Debug.Log("TurnOn: " + previousState);
        currentState = previousState;
        previousState = PlatformState.Off;
    }

    // States

    void OffState() {}

    void MovingState()
    {
        currentDuration += Time.deltaTime;

        float ratio = Mathf.Min(currentDuration, pointToPointDuration) / pointToPointDuration;

        ratio = movementCurve.Evaluate(ratio);
        transform.position = Vector3.Lerp(waypoints[sourceWaypoint], waypoints[destWaypoint], ratio);

        float remaining = pointToPointDuration - currentDuration;
        if (currentDuration <= audioRampUpDownTime || remaining <= audioRampUpDownTime)
        {
            ratio = Mathf.Min(currentDuration, remaining) / audioRampUpDownTime;
            _audioSource.volume = Mathf.Max(0, ratio);
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

            Pause();
        }
    }

    void PausedState()
    {
        currentDuration += Time.deltaTime;

        if (currentDuration >= pauseDuration) {
            currentDuration -= pauseDuration;

            Resume();
        }
    }

    // Interactable

    protected override void HandleInteraction(bool active)
    {
        // onAtStart=true  : active=true -> TurnOff, active=false -> TurnOn
        // onAtStart=false : active=true -> TurnOn,  active=false -> TurnOff
        bool turnOff = onAtStart == active;
        if (turnOff)
            TurnOff();
        else
            TurnOn();
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
