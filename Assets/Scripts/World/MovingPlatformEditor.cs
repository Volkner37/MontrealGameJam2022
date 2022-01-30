using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR) 
[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{

    public override void OnInspectorGUI()
    {
        MovingPlatform script = (MovingPlatform)target;
        DrawDefaultInspector();

        if (script.WaypointsLocked)
        {
            EditorGUILayout.HelpBox(
                "The waypoints are locked. Unlock them to move the platform without " +
                "moving the waypoints.",
                MessageType.Info);
            if (GUILayout.Button("Unlock Waypoints"))
            {
                script.WaypointsLocked = false;
                EditorUtility.SetDirty( script );
            }
            GUI.enabled = false;
        }
        else
        {
            EditorGUILayout.HelpBox(
                "The waypoints are not locked. Moving the platform will not move the " +
                "waypoints with it. Lock the waypoints using the button below once they " +
                "are placed.",
                MessageType.Warning);
            if (GUILayout.Button("Lock Waypoints"))
            {
                script.WaypointsLocked = true;
                EditorUtility.SetDirty( script );
            }
        }

        EditorGUILayout.HelpBox(
            "The platform will always start in the Moving state from waypoint 0 to 1. " +
            "Use 'Set Waypoint #' buttons below to set waypoint to current location of " +
            "the GameObject. Use 'Snap to Waypoint #' buttons to snap the GameObject to " +
            "the selected waypoint.",
            MessageType.Info);

        for (int b = 0; b < script.GetWaypointsLength(); ++b) {
            if (GUILayout.Button("Set Waypoint " + b.ToString()))
            {
                script.SetWaypoint(b);
                EditorUtility.SetDirty( script );
            }
        }

        if (script.WaypointsLocked)
        {
            GUI.enabled = true;
        }
    }
}
#endif