using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField] private WorldMusic[] assets;
    [SerializeField] private GameWorlds currentWorld;
    [SerializeField] private AudioClip ageTransitionEffect;

    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        SelectMusicAsset();
    }

    public void SelectMusicAsset()
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

    public void ChangeMusicAge(MusicAsset worldMusic)
    {
        StartCoroutine(SwapMusicAge(worldMusic));
    }

    public IEnumerator SwapMusicAge(MusicAsset worldMusic)
    {
        source.Stop();
        source.clip = null;
        source.PlayOneShot(ageTransitionEffect);

        yield return new WaitForSeconds(ageTransitionEffect.length - 1f);

        PlayMusic(worldMusic);
    }

    void PlayMusic(MusicAsset ma)
    {
        int randomTrackIdx = Random.Range(0, ma.worldTracks.Length);

        if (ma.worldTracks[randomTrackIdx].startTrack != null)
        {
            source.loop = false;
            source.clip = ma.worldTracks[randomTrackIdx].startTrack;
            source.Play();
            //Debug.Log("start track: " + ma.worldTracks[randomTrackIdx].startTrack + ", " + ma.worldTracks[randomTrackIdx].musicTrack);
            StartCoroutine(StartTrack(ma.worldTracks[randomTrackIdx].musicTrack));
        }

        else
        {
            //Debug.Log(ma.worldTracks[randomTrackIdx].musicTrack);
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
