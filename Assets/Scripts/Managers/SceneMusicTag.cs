using UnityEngine;

public class SceneMusicTag : MonoBehaviour
{
    public enum SceneGroup { Menu, Backstory, Rooms, Boss, No_sound }
    [Header("What is this scene?")]
    public SceneGroup sceneGroup;

    [Header("Clips")]
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
                CheckpointGameData.ClearAll();
                MusicManager.Instance.StopMusic();
                MusicManager.Instance.SetTrack(menuClip, true);
                break;
            case SceneGroup.Backstory:
                MusicManager.Instance.SetTrack(backstoryClip, false);
                break;
            case SceneGroup.Rooms:
                if (roomsClip != null)
                {
                    if (CheckpointGameData.hasCheckpoint && MusicManager.Instance.IsPlayingLoopClip(roomsClip))
                        return;
                    MusicManager.Instance.SetTrack(roomsClip, false);
                }
                break;
            case SceneGroup.Boss:
                MusicManager.Instance.StopMusic();
                MusicManager.Instance.SetTrack(No_soundClip, true);
                break;
            case SceneGroup.No_sound:
                MusicManager.Instance.StopMusic();
                MusicManager.Instance.SetTrack(No_soundClip, true);
                break;
        }
    }
}
