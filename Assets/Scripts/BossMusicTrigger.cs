using UnityEngine;

public class BossMusicTrigger : MonoBehaviour
{   
     public AudioClip bossClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayBossMusic()
    {
            MusicManager.Instance.SetTrack(bossClip, true);
    }
}
