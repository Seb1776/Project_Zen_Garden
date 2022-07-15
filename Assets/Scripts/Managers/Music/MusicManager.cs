using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [SerializeField] private WorldMusic[] assets;
    [SerializeField] private GameWorlds currentWorld;
    [SerializeField] private AudioClip ageTransitionEffect;
    [SerializeField] private GameWorlds changeTo;
    [SerializeField] private MusicAsset throwback;

    //NormalMusic
    private List<MusicOrder> musicOrder = new List<MusicOrder>();
    private int currentMusicOrderIdx; private bool playingMusicOrder;
    private List<MusicOrder> s_musicOrder = new List<MusicOrder>();
    private int currentSMusicOrderIdx; private bool playingSMusicOrder;
    private List<MusicOrder> challengesMusic = new List<MusicOrder>();
    private int currentChallengeOrderIdx; private bool playingChallengeOrder;
    //PresentMusic
    private List<MusicOrder> currentPresentOrder = new List<MusicOrder>();
    private int currentPresentOrderIdx; private bool playingPresentOrder;
    public string currentPresentContext;
    private Coroutine playLoopCoroutine;
    private Coroutine swapCoroutine;
    private Coroutine mainCoroutine;
    private AudioSource source;
    private bool playingLevel;
    private MusicAsset currentAsset;

    void Awake()
    {
        source = GetComponent<AudioSource>();

        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        SetWorldsPrices();
        SelectMusicAsset(currentWorld);

        if (currentAsset.world == GameWorlds.ThrowbackToThePresent)
        {
            PopulatePresentMusic("day");
        }

        else
        {
            GetMusicOrdersFromWorld(currentAsset);
            ChooseNewMusicOrder();
        }
    }

    public void ChangeMusic(MusicAsset newAge)
    {
        currentAsset = newAge;
        SelectMusicAsset(currentAsset.world);
        GetMusicOrdersFromWorld(currentAsset);
        ChooseNewMusicOrder();
    }

    void SetWorldsPrices()
    {
        /*foreach (WorldMusic wm in assets)
        {   
            if (wm.asset != null)
            {
                wm.worldButton.interactable = false;
                wm.worldPrice.text = "$ " + wm.asset.unlockPrice.ToString("0,0");
            }
        }*/
    }

    public void GetMusicOrdersFromWorld(MusicAsset ma)
    {
        s_musicOrder.Clear();
        musicOrder.Clear();
        challengesMusic.Clear();

        if (!TimeTravelManager.instance.HasEnteredBefore(ma.worldID) && ma.GetMusicSet("first").main != null)
        {
            s_musicOrder.Add(new MusicOrder(ma.GetMusicSet("seeds"), false));
            s_musicOrder.Add(new MusicOrder(ma.GetMusicSet("first"), false));
            s_musicOrder.Add(new MusicOrder(ma.GetMusicSet("mida"), false));
            s_musicOrder.Add(new MusicOrder(ma.GetMusicSet("midb"), false));
            s_musicOrder.Add(new MusicOrder(ma.GetMusicSet("final"), true));
        }

        else if (!TimeTravelManager.instance.HasEnteredBefore(ma.worldID) && ma.GetMusicSet("first").main == null)
        {
            s_musicOrder.Add(new MusicOrder(ma.GetMusicSet("seeds"), false));
        }
        
        else if (TimeTravelManager.instance.HasEnteredBefore(ma.worldID) && ma.GetMusicSet("first").main != null)
        {
            musicOrder.Add(new MusicOrder(ma.GetMusicSet("first"), false));
            musicOrder.Add(new MusicOrder(ma.GetMusicSet("mida"), false));
            musicOrder.Add(new MusicOrder(ma.GetMusicSet("midb"), false));
            musicOrder.Add(new MusicOrder(ma.GetMusicSet("final"), true));
        }

        List<MusicSet> chall = ma.GetRandomChallenges();

        for (int i = 0; i < chall.Count; i++)
        {   
            if (i < chall.Count)
                challengesMusic.Add(new MusicOrder(chall[i], false));
            
            else if (i == chall.Count - 1)
                challengesMusic.Add(new MusicOrder(chall[i], true));
        }
    }

    public float GetListDuration()
    {
        float full = 0f;

        foreach (MusicOrder mo in currentPresentOrder)
            full += mo.musicSet.main.length;

        return full;
    }

    public void ChooseNewMusicOrder()
    {
        currentMusicOrderIdx = 0;
        playingMusicOrder = false;
        currentSMusicOrderIdx = 0;
        playingSMusicOrder = false;
        currentChallengeOrderIdx = 0;
        playingChallengeOrder = false;

        if (TimeTravelManager.instance.HasEnteredBefore(currentAsset.worldID))
        {
            if (GameHelper.GetRandomBool() && musicOrder.Count > 0)
                playingMusicOrder = true;

            else
                playingChallengeOrder = true;
        }

        else
        {
            playingSMusicOrder = true;
            TimeTravelManager.instance.PlayerEnteredWorld(currentAsset.worldID);
        }

        PlayMusicOrder();
    }

    public void SelectMusicAsset(GameWorlds worldTo)
    {   
        foreach (WorldMusic wm in assets)
        {
            if (wm.world == worldTo)
            {
                currentAsset = wm.asset;
                break;
            }
        }

        foreach (FlowerPots fps in SeedDatabase.instance.flowerPots)
        {
            fps.InitUI();
        }
    }

    public void PopulatePresentMusic(string time)
    {
        playingChallengeOrder = false;
        currentChallengeOrderIdx = 0;
        playingMusicOrder = false;
        currentMusicOrderIdx = 0;
        playingSMusicOrder = false;
        currentSMusicOrderIdx = 0;

        currentPresentOrder.Clear();
        currentPresentOrderIdx = 0;
        playingPresentOrder = false;

        if (!TimeTravelManager.instance.HasEnteredBefore(currentAsset.worldID))
            currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("first"), false));

        currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("seeds"), false));

        switch (currentPresentContext)
        {
            case "front":
                if (time == "day")
                {
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("fronta"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("loonboon"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("ultimatea"), false));
                }

                else
                {
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("frontb"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("cerebrawl"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("minigame"), false));
                }
            break;

            case "back":
                if (time == "day")
                {
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("backa"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("loonboon"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("ultimateb"), false));
                }

                else
                {
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("backb"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("cerebrawl"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("minigame"), false));
                }
            break;

            case "roof":
                if (time == "day")
                {
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("roofa"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("minigame"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("ultimateb"), false));
                }

                else
                {
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("roofb"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("ultimatea"), false));
                    currentPresentOrder.Add(new MusicOrder(currentAsset.GetMusicSetP("brainiac"), false));
                }
            break;
        }

        Debug.Log(currentPresentOrder.Count);
        playingPresentOrder = true;
        if (time == "day") DayNightManager.instance.TriggerDay();
        else DayNightManager.instance.TriggerNight();
        PlayMusicOrder();
    }

    public void PlayMusicOrder()
    {
        if (playingMusicOrder)
        {
            PlayMusicSet(musicOrder[currentMusicOrderIdx].musicSet);
            currentMusicOrderIdx++;
        }

        else if (playingSMusicOrder)
        {
            PlayMusicSet(s_musicOrder[currentSMusicOrderIdx].musicSet);
            currentSMusicOrderIdx++;
        }

        else if (playingChallengeOrder)
        {
            PlayMusicSet(challengesMusic[currentChallengeOrderIdx].musicSet);
            currentChallengeOrderIdx++;
        }

        else if (playingPresentOrder)
        {
            Debug.Log("here");
            PlayMusicSet(currentPresentOrder[currentPresentOrderIdx].musicSet);
            currentPresentOrderIdx++;
        }
    }

    public void CheckForNextSong()
    {
        if (playingMusicOrder)
        {
            if (currentMusicOrderIdx >= musicOrder.Count)
            {
                playingMusicOrder = false;
                ChooseNewMusicOrder();
            }

            else
                PlayMusicOrder();
        }

        if (playingChallengeOrder)
        {
            if (currentChallengeOrderIdx >= challengesMusic.Count)
            {
                playingChallengeOrder = false;
                ChooseNewMusicOrder();
            }

            else
                PlayMusicOrder();
        }

        if (playingSMusicOrder)
        {
            if (currentSMusicOrderIdx >= s_musicOrder.Count)
            {
                playingSMusicOrder = false;
                ChooseNewMusicOrder();
            }

            else
                PlayMusicOrder();
        }

        if (playingPresentOrder)
        {
            if (currentPresentOrderIdx < currentPresentOrder.Count)
                PlayMusicOrder();
            
            else
            {
                if (DayNightManager.instance.day) DayNightManager.instance.TriggerNight(currentPresentContext == "back");
                else DayNightManager.instance.TriggerDay(currentPresentContext == "back");

                PopulatePresentMusic(DayNightManager.instance.day ? "day" : "night");
            }
        }
    }

    IEnumerator WaitForMainToEnd()
    {
        while (source.isPlaying)
            yield return null;
        
        CheckForNextSong();
    }

    void PlayMusicSet(MusicSet song)
    {
        if (song.start != null)
        {
            source.loop = false;
            source.clip = song.start;
            source.Play();

            if (playLoopCoroutine != null)
            {
                StopCoroutine(playLoopCoroutine);
                playLoopCoroutine = StartCoroutine(StartTrack(song.main));
            }

            else
                playLoopCoroutine = StartCoroutine(StartTrack(song.main));
        }

        else
        {
            source.clip = song.main;
            source.Play();
            
            if (mainCoroutine == null)
            {
                mainCoroutine = StartCoroutine(WaitForMainToEnd());
            }

            else
            {
                StopCoroutine(mainCoroutine);
                mainCoroutine = StartCoroutine(WaitForMainToEnd());
            }
        }
    }

    IEnumerator StartTrack(AudioClip loopTrack)
    {
        while (source.isPlaying)
            yield return null;

        source.clip = loopTrack;
        source.Play();

        if (mainCoroutine == null)
        {
            mainCoroutine = StartCoroutine(WaitForMainToEnd());
        }

        else
        {
            StopCoroutine(mainCoroutine);
            mainCoroutine = StartCoroutine(WaitForMainToEnd());
        }
    }

    public MusicAsset GetCurrentMusic()
    {
        return currentAsset;
    }

    /*public void UnlockWorld(MusicAsset world)
    {
        foreach (WorldMusic wm in assets)
        {
            if (wm.asset == world)
            {
                wm.unlockButton.SetActive(false);
                wm.worldButton.interactable = true;
            }
        }
    }*/

    public void ChangeMusicAge(MusicAsset worldMusic)
    {   
        if (currentAsset != worldMusic)
        {
            if (swapCoroutine != null)
            {
                StopCoroutine(swapCoroutine);
                swapCoroutine = StartCoroutine(SwapMusicAge(worldMusic));
            }

            else
                swapCoroutine = StartCoroutine(SwapMusicAge(worldMusic));
        }

        else
            SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
    }

    public void ChangeToPresent(string presentContext)
    {
        if (currentPresentContext != presentContext)
        {
            currentPresentContext = presentContext;

            if (swapCoroutine != null)
            {
                StopCoroutine(swapCoroutine);
                swapCoroutine = StartCoroutine(SwapMusicAge(throwback));
            }

            else
                swapCoroutine = StartCoroutine(SwapMusicAge(throwback));
        }

        else
            SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
    }

    public IEnumerator SwapMusicAge(MusicAsset worldMusic)
    {
        if (playLoopCoroutine != null) StopCoroutine(playLoopCoroutine);
        if (mainCoroutine != null) StopCoroutine(mainCoroutine);

        source.Stop();
        source.clip = null;
        source.PlayOneShot(ageTransitionEffect);
        TimeTravelManager.instance.TriggerTransition(true);

        yield return new WaitForSeconds(.2f);

        if (worldMusic.world != GameWorlds.ThrowbackToThePresent)
            TimeTravelManager.instance.ChangeScenario(worldMusic.worldID, false);
        
        else
            TimeTravelManager.instance.ChangeScenario(currentPresentContext, true);

        yield return new WaitForSeconds(ageTransitionEffect.length - 1f);

        TimeTravelManager.instance.TriggerTransition(false);
        
        if (worldMusic.world != GameWorlds.ThrowbackToThePresent)
        {
            ChangeMusic(worldMusic);
        }
        
        else
        {
            PopulatePresentMusic("day");
        }
    }
}

[System.Serializable]
public class WorldMusic
{
    public GameWorlds world;
    public MusicAsset asset;
    public Button worldButton;
    public Text worldPrice;
    public GameObject unlockButton;
    public bool hasTravelledToThisWorld;
}

[System.Serializable]
public class MusicOrder
{
    public MusicSet musicSet;
    public bool lastInGroup;

    public MusicOrder (MusicSet musicSet, bool lastInGroup)
    {
        this.musicSet = musicSet;
        this.lastInGroup = lastInGroup;
    }
}
