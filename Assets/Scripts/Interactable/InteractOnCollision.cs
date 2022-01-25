using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractOnCollision : MonoBehaviour
{
    [SerializeField]
    private AbstractInteractable interactable;
    [Tooltip("If empty, any object may cause the trigger.")]
    [SerializeField]
    private List<GameObject> collidesWith;

    private void OnCollisionEnter(Collision other) {
        if (collidesWith.Count != 0 && !collidesWith.Contains(other.gameObject))
            return;

        interactable.Interact();
    }
}
