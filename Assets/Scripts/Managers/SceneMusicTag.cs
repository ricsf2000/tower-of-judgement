using UnityEngine;

public class SceneMusicTag : MonoBehaviour
{
    public enum SceneGroup { Menu, Backstory, Rooms }
    [Header("What is this scene?")]
    public SceneGroup sceneGroup;

    [Header("Clips (assign in Inspector)")]
    public AudioClip menuClip;
    public AudioClip backstoryClip;
    public AudioClip roomsClip;

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
        }
    }
}
