using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> musics = new List<AudioClip>();
    [SerializeField] private AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        
        SceneLoaderUtils.OnSceneLoaded+= SceneLoaderUtilsOnOnSceneLoaded;
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        SceneLoaderUtilsOnOnSceneLoaded(null, new SceneLoaderUtils.SceneLoadEventArgs("MainMenu"));
    }

    private void SceneLoaderUtilsOnOnSceneLoaded(object sender, SceneLoaderUtils.SceneLoadEventArgs e)
    {
        int index = SceneLoaderUtils.GetSceneIndex(e.Name);
        
        
        int totalNumberOfScene = SceneManager.sceneCountInBuildSettings;
        int songIndex = index * musics.Count / totalNumberOfScene;
        
        Debug.Log($"Loading song index : {songIndex}");

        if (audioSource.clip == musics[songIndex]) return;
        
        audioSource.clip = musics[songIndex];    
        audioSource.Play();
        Debug.Log("play Music Main Music");
    }
}
