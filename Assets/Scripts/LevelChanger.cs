using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    private string levelToLoad;
    private static LevelChanger _instance;

    public static LevelChanger Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    public void FadeToLevel (string levelIndex){
        Animator animator = GetComponentInChildren<Animator>();
        levelToLoad = levelIndex;
        animator.Play("Fade_Out");
        Debug.Log("test"+ levelIndex);
    }

    public void OnFadeComplete(){
        SceneManager.LoadScene(levelToLoad);
        Debug.Log("next"+ levelToLoad);
    }
}
