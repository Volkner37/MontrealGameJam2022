using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class InteractableSFX : AbstractInteractable
{
    private AudioSource _audioSource;
    private bool _active = false;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    protected override void HandleInteraction(bool active)
    {
        if (active == _active)
            return;

        if (active)
            _audioSource.Play();
        else
            _audioSource.Stop();
    }
}
