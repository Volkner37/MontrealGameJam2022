using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    public Animator animator;

    private int levelToLoad;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
            FadeToLevel(1);
    }

    public void FadeToLevel (int levelIndex){
        animator.SetTrigger("FadeOut");
    }

    public void OnFadeComplete(){
        SceneManager.LoadScene(levelToLoad);
    }
}
