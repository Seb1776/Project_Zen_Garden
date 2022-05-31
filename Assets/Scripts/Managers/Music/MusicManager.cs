using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField] private WorldMusic[] assets;
    [SerializeField] private GameWorlds currentWorld;

    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        SelectMusicAsset();
    }

    void SelectMusicAsset()
    {
        foreach (WorldMusic wm in assets)
        {
            if (wm.world == currentWorld)
            {
                PlayMusic(wm.asset);
                break;
            }
        }
    }

    void ChangeMusicAge()
    {
        
    }

    void PlayMusic(MusicAsset ma)
    {
        int randomTrackIdx = Random.Range(0, ma.worldTracks.Length);

        if (ma.worldTracks[randomTrackIdx].startTrack != null)
        {
            source.loop = false;
            source.clip = ma.worldTracks[randomTrackIdx].startTrack;
            source.Play();
            StartCoroutine(StartTrack(ma.worldTracks[randomTrackIdx].musicTrack));
        }

        else
        {
            source.loop = true;
            source.clip = ma.worldTracks[randomTrackIdx].musicTrack;
            source.Play();
        }
    }

    IEnumerator StartTrack(AudioClip loopTrack)
    {
        while (source.isPlaying)
            yield return null;

        source.loop = true;
        source.clip = loopTrack;
        source.Play();
    }
}

[System.Serializable]
public class WorldMusic
{
    public GameWorlds world;
    public MusicAsset asset;
}
