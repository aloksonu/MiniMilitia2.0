using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
public class LevelManager : MonoBehaviour
{
    private event Action<string> NextLevel;

    [Header("Next Level Setup")]
    [SerializeField] private string nextScene;
    [SerializeField] private float loadDuration = 1f;

    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager>();
            }
            return _instance;
        }
       
    }
    // Start is called before the first frame update
    void Start()
    {
        NextLevel += LevelChange;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LevelChange(string scene)
    {
        SceneManager.LoadScene(scene);
    }
    public void GoToNextLevel()
    {
        Invoke(nameof(NextLevelInvokeMethod), loadDuration);
    }
    private void NextLevelInvokeMethod()
    {
        NextLevel?.Invoke(nextScene);
        NextLevel -= LevelChange;
    }
    public async void QuitGame()
    {
        await NetworkManager.Instance.CleanupRunner();
        Application.Quit();
    }
}
