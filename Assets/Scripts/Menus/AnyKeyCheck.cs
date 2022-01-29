using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnyKeyCheck : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    
    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;    
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            mainMenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
