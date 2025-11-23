using UnityEngine;

public class SceneMusicTag : MonoBehaviour
{
    public enum SceneGroup { Menu, Backstory, Rooms, Boss, No_sound }
    [Header("What is this scene?")]
    public SceneGroup sceneGroup;

    [Header("Clips (assign in Inspector)")]
    public AudioClip menuClip;
    public AudioClip backstoryClip;
    public AudioClip roomsClip;
    public AudioClip bossClip;
    public AudioClip No_soundClip;

    void Start()
    {
        if (MusicManager.Instance == null) return;

        switch (sceneGroup)
        {
            case SceneGroup.Menu:
                MusicManager.Instance.SetTrack(menuClip, false);
                break;
            case SceneGroup.Backstory:
                MusicManager.Instance.SetTrack(backstoryClip, false);
                break;
            case SceneGroup.Rooms:
                MusicManager.Instance.SetTrack(roomsClip, false);
                break;
            case SceneGroup.Boss:
                MusicManager.Instance.SetTrack(bossClip, false);
                break;
            case SceneGroup.No_sound:
                MusicManager.Instance.SetTrack(No_soundClip, false);
                break;
        }
    }
}
