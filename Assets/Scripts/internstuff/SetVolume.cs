using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class SetVolume : MonoBehaviour
{
    public void Start()
    {
        mixer.SetFloat("MainVolume", Mathf.Log10(GetComponent<Slider>().value) * 20);
    }
    public AudioMixer mixer;
    public void SetLevel(float sliderValue)
    {
        mixer.SetFloat("MainVolume",Mathf.Log10(sliderValue)*20);
    }
}
