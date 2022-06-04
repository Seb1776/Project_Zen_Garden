using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField] private WorldMusic[] assets;
    [SerializeField] private GameWorlds currentWorld;
    [SerializeField] private MusicAsset debugWorld;
    [SerializeField] private AudioClip ageTransitionEffect;

    private Coroutine playLoopCoroutine;
    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        SelectMusicAsset();
    }

    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
            ChangeMusicAge(debugWorld);
    }

    public void SelectMusicAsset()
    {
        foreach (WorldMusic wm in assets)
        {
            if (wm.world == currentWorld)
            {
                ChangeMusicAge(wm.asset);
                break;
            }
        }
    }

    public void ChangeMusicAge(MusicAsset worldMusic)
    {
        Debug.Log(playLoopCoroutine);

        if (playLoopCoroutine != null)
        {
            Debug.Log("stopped coroutine");
            StopCoroutine(playLoopCoroutine);
            playLoopCoroutine = StartCoroutine(SwapMusicAge(worldMusic));
        }

        else
            playLoopCoroutine = StartCoroutine(SwapMusicAge(worldMusic));
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
            Debug.Log("start track: " + ma.worldTracks[randomTrackIdx].startTrack + ", " + ma.worldTracks[randomTrackIdx].musicTrack);
            StartCoroutine(StartTrack(ma.worldTracks[randomTrackIdx].musicTrack));
        }

        else
        {
            Debug.Log(ma.worldTracks[randomTrackIdx].musicTrack);
            source.loop = true;
            source.clip = ma.worldTracks[randomTrackIdx].musicTrack;
            source.Play();
        }
    }

    IEnumerator StartTrack(AudioClip loopTrack)
    {
        Debug.Log(loopTrack.name);

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
