using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CinematicManager : MonoBehaviour
{
    [SerializeField]
    private List<Texture> slides = new List<Texture>();

    [SerializeField] 
    private RawImage _image;
    private int _currentIndex = 0;

    [SerializeField] private RawImage rightArrow;
    [SerializeField] private RawImage leftArrow;
    
    void Start()
    {
        Debug.Log("Start");
        _image.texture = slides[_currentIndex];
    }

    private void Update()
    {
        if (_currentIndex < slides.Count)
        {
            leftArrow.enabled = _currentIndex != 0;
            rightArrow.enabled = _currentIndex != slides.Count-1;
        
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetMouseButtonDown(0))
            {
                _currentIndex++;

                if (_currentIndex >= slides.Count)
                {
                    SceneLoaderUtils.LoadScene("Level_00");
                    return;
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetMouseButtonDown(1))
            {
                _currentIndex--;

                if (_currentIndex < 0)
                    _currentIndex = 0;
            }
        
            _image.texture = slides[_currentIndex];
        }
    }
}
