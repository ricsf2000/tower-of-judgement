using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager manager;

    public GameObject deathPanel;

    private void Awake()
{
    
        manager = this;
}


    public void GameOver()
    {
        deathPanel.SetActive(true);
    }
    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
