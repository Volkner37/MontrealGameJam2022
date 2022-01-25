using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Playlevel : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown dropdown;


    public void LoadLevel()
    {
        Debug.Log(dropdown.options[dropdown.value].text);
        SceneManager.LoadScene(dropdown.options[dropdown.value].text);

    }
}