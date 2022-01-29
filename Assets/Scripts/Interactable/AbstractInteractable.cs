using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractInteractable : MonoBehaviour
{
    [SerializeField] private AbstractInteractable next = null;

    public void Interact(bool active)
    {
        HandleInteraction(active);
        if (next)
            next.Interact(active);
    }

    protected abstract void HandleInteraction(bool active);
}
