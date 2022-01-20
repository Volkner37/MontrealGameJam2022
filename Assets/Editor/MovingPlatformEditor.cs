using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{

    public override void OnInspectorGUI()
    {
        MovingPlatform script = (MovingPlatform)target;
        DrawDefaultInspector();

        EditorGUILayout.HelpBox(
            "The platform will always start in the Moving state from waypoint 0 to 1. " +
            "Use 'Set Waypoint #' buttons below to set waypoint to current location of " +
            "the GameObject. Use 'Snap to Waypoint #' buttons to snap the GameObject to " +
            "the selected waypoint",
            MessageType.Info);

        for (int b = 0; b < script.waypoints.Length; ++b) {
            if (GUILayout.Button("Set Waypoint " + b.ToString()))
            {
                script.SetWaypoint(b);
            }
        }

        for (int b = 0; b < script.waypoints.Length; ++b) {
            if (GUILayout.Button("Snap to Waypoint " + b.ToString()))
            {
                script.SnapToWaypoint(b);
            }
        }
    }
}
