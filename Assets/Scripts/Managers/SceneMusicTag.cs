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

    [Header("Rooms Music Sequence")]
    public AudioClip roomIntroClip;
    public AudioClip roomLoopClip;
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
                HandleRoomMusic();
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

    private void HandleRoomMusic()
    {
        // If no intro/loop provided, fallback to basic behavior
        if (roomIntroClip == null || roomLoopClip == null)
        {
            MusicManager.Instance.SetTrack(roomLoopClip, false);
            return;
        }

        // If retrying / respawning, then skip intro
        if (CheckpointGameData.hasCheckpoint &&
            MusicManager.Instance.IsPlayingLoopClip(roomLoopClip))
        {
            return; // music is already in loop mode
        }

        // Otherwise play intro then loop
        MusicManager.Instance.PlayIntroThenLoop(roomIntroClip, roomLoopClip);
    }
}
