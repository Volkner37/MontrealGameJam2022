using System;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    
    public class PauseEventArgs : EventArgs
    {
        public PauseEventArgs(bool isPaused)
        { 
            IsPaused = isPaused;
        }
            
        public readonly bool IsPaused;
    }
    
    
    public GameObject pauseMenuUI;
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(GameIsPaused){
                Resume();
            }
            else{
                Pause();
            }
        }
    }
    private void Disable(bool enable)
    {
        GameIsPaused = enable;
        GameObject player= GameObject.FindWithTag("Player");
        player.GetComponent<MouseHandler>().enabled = !enable;
        Cursor.visible = false;
        pauseMenuUI.SetActive(enable);
        if(enable)
           Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }
    public void Resume()
    {
        GetComponentInParent<PlayerControllerV2>().MuteAllSounds(false);
        Time.timeScale = 1f; 
        Disable(false);
    }
    public void Pause()
    {
        GetComponentInParent<PlayerControllerV2>().MuteAllSounds(true);
        Time.timeScale = 0f;
        Disable(true);
        Debug.Log("pause");
    }

    public void QuitLevel()
    {
        Time.timeScale = 1f; 
        SceneLoaderUtils.LoadScene("MainMenu");
    }

    public void ResetLevel()
    {
        Time.timeScale = 1f; 
        string name = SceneManager.GetActiveScene().name;
        SceneLoaderUtils.LoadScene(name);
    }
}