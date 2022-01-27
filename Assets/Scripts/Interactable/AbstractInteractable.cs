using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractInteractable : MonoBehaviour
{
    [SerializeField] private AbstractInteractable next = null;

    public void Interact()
    {
        HandleInteraction();
        if (next)
            next.Interact();
    }

    protected abstract void HandleInteraction();
}
