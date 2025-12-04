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
    
    [Header("Room Music Sequence")]
    public AudioClip roomFirstPartClip;
    public AudioClip roomLoopClip;

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
                if (roomFirstPartClip != null && roomLoopClip != null)
                {
                    bool isRespawn = CheckpointGameData.hasCheckpoint;
                    bool alreadyPlayingLoop = MusicManager.Instance.IsPlayingLoopClip(roomLoopClip);
                    bool isEditorLoad = !Application.isPlaying;
                    
                    if (isRespawn && alreadyPlayingLoop)
                    {
                        MusicManager.Instance.ContinuePlaying();
                    }
                    else if (isEditorLoad || (!isRespawn && !alreadyPlayingLoop))
                    {
                        MusicManager.Instance.PlayIntroThenLoop(roomFirstPartClip, roomLoopClip);
                    }
                    else if (roomLoopClip != null)
                    {
                        MusicManager.Instance.SetTrack(roomLoopClip, false);
                    }
                    else
                    {
                        MusicManager.Instance.SetTrack(roomsClip, false);
                    }
                }
                else
                {
                    if (roomLoopClip != null)
                        MusicManager.Instance.SetTrack(roomLoopClip, false);
                    else
                        MusicManager.Instance.SetTrack(roomsClip, false);
                }
                break;
            case SceneGroup.Boss:
                MusicManager.Instance.SetTrack(bossClip, true);
                break;
            case SceneGroup.No_sound:
                MusicManager.Instance.SetTrack(No_soundClip, true);
                break;
        }
    }
}
