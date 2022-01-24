using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractOnTrigger : MonoBehaviour
{
    [SerializeField]
    private AbstractInteractable interactable;
    [Tooltip("If empty, any object may cause the trigger.")]
    [SerializeField]
    private List<GameObject> acceptTriggerFrom;

    private void OnTriggerEnter(Collider other) {
        if (acceptTriggerFrom.Count != 0 && !acceptTriggerFrom.Contains(other.gameObject))
            return;

        interactable.Interact();
    }
}
