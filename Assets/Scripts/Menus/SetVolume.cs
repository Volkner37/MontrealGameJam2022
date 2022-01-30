using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class SetVolume : MonoBehaviour
{
    public Slider _slider;
    public void Start()
    {
        _slider = GetComponent<Slider>();
        _slider.SetValueWithoutNotify(AudioListener.volume);
    }
    
    public void SetLevel(float sliderValue)
    {
        AudioListener.volume = sliderValue;
    }
}
