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

        //We want to skip the splashscreen if we come back to main menu
        if (!GameObject.Find("MusicManager"))
        {
            GameObject manager = Instantiate((GameObject) Resources.Load("Prefabs/MusicManager"));
            manager.name = "MusicManager";
        }
        else
        {
            mainMenu.SetActive(true);
            gameObject.SetActive(false);
        }
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
