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
    public GameWorlds currentWorld;
    [SerializeField] private GameObject[] modernDayFullRoster;
    [SerializeField] private AudioClip ageTransitionEffect;
    [SerializeField] private MusicAsset throwback;
    [Header ("Tutorial Things")]
    [SerializeField] private GameObject modernDayUnlockPanel;
    [SerializeField] private GameObject fullModernDayPanel, modernDayTutorialPanel, tutorialGarden, tutorialGardening, fullGardening;

    //NormalMusic
    private List<AudioClip> musicOrder = new List<AudioClip>();
    private int currentMusicOrderIdx; private bool playingMusicOrder;
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
    private string playingContext = string.Empty;

    void Awake()
    {
        source = GetComponent<AudioSource>();

        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        SetWorldsPrices();
    }

    void Update()
    {
        if (panning)
            PanMusicLevel();
    }

    public void ChangeMusic(MusicAsset newAge)
    {
        currentAsset = newAge;
        SelectMusicAsset(currentAsset.world);
        ChooseNewMusicOrder();
    }

    void SetWorldsPrices()
    {
        foreach (WorldMusic wm in assets)
        {   
            if (wm.asset != null && wm.world != GameWorlds.ModernDay && wm.world != GameWorlds.ThrowbackToThePresent && wm.world != GameWorlds.Tutorial)
            {
                wm.worldButton.interactable = false;
                wm.worldPrice.text = "$ " + wm.asset.unlockPrice.ToString("0,0");

                foreach (Image i in wm.plantImages)
                    i.color = new Color(0f, 0f, 0f, 1f);
            }
        }
    }

    public void ChooseNewMusicOrder()
    {
        currentMusicOrderIdx = 0;
        playingMusicOrder = false;
        musicOrder.Clear();

        if (!TimeTravelManager.instance.HasEnteredBefore(currentAsset.worldID))
        {
            musicOrder = currentAsset.GetMusicClips(SetMusicMode.Start, false);
            playingContext = "main";
        }
        
        else if (TimeTravelManager.instance.HasEnteredBefore(currentAsset.worldID))
        {
            if (playingContext == string.Empty || playingContext == "")
            {   
                if (currentAsset.GetMusicClips(SetMusicMode.Main, false).Count > 0)
                {
                    if (GameHelper.GetRandomBool())
                    {
                        musicOrder = currentAsset.GetMusicClips(SetMusicMode.Main, false);
                        playingContext = "main";
                    }

                    else
                    {
                        musicOrder = currentAsset.GetMusicClips(SetMusicMode.Special, true);
                        playingContext = "special";
                    }
                }

                else
                {
                    musicOrder = currentAsset.GetMusicClips(SetMusicMode.Special, true);
                    playingContext = "special";
                }
            }

            else
            {   
                if (currentAsset.GetMusicClips(SetMusicMode.Main, false).Count > 0)
                {
                    if (playingContext == "main")
                    {
                        musicOrder = currentAsset.GetMusicClips(SetMusicMode.Special, true);
                        playingContext = "special";
                    }
                    
                    else
                    {
                        musicOrder = currentAsset.GetMusicClips(SetMusicMode.Main, false);
                        playingContext = "main";
                    }
                }

                else
                {
                    musicOrder = currentAsset.GetMusicClips(SetMusicMode.Special, true);
                    playingContext = "special";
                }
            }
        }

        playingMusicOrder = true;
        PlayMusicOrder();

        TimeTravelManager.instance.SetAsEntered(currentAsset.worldID);
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
            fps.InitUI();
    }

    public void PlayMusicOrder()
    {
        if (playingMusicOrder)
        {
            PlayMusicSet(musicOrder[currentMusicOrderIdx]);
            
            if (musicOrder.Count != 0)
                currentMusicOrderIdx++;
        }
    }

    void PlayMusicSet(AudioClip song)
    {
        source.clip = song;
        source.Play();

        if (mainCoroutine == null)
            mainCoroutine = StartCoroutine(WaitForMainToEnd());
        
        else
        {
            StopCoroutine(mainCoroutine);
            mainCoroutine = StartCoroutine(WaitForMainToEnd());
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
    }

    IEnumerator WaitForMainToEnd()
    {
        while (source.isPlaying)
            yield return null;
        
        CheckForNextSong();
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

    public void UnlockWorld(MusicAsset world)
    {
        foreach (WorldMusic wm in assets)
        {
            if (Player.instance.CanSpendMoney(wm.asset.unlockPrice))
            {
                if (wm.asset == world)
                {
                    DataCollector.instance.AddUnlockedWorld(world.world);
                    StartCoroutine(WorldLock(world));
                }
            }
        }
    }

    public void DeactivateWorldLock(MusicAsset world)
    {
        foreach (Image i in GetWorldData(world).plantImages)
            i.color = new Color(1f, 1f, 1f, 1f);

        if (GetWorldData(world).worldLock != null)
            GetWorldData(world).worldLock.gameObject.SetActive(false);

        if (GetWorldData(world).unlockButton != null)
            GetWorldData(world).unlockButton.SetActive(false);

        GetWorldData(world).worldButton.interactable = true;

        if (GetWorldData(world).plantsButton != null)
            GetWorldData(world).plantsButton.interactable = true;

        if (GetWorldData(world).unlockFirstPanel != null)
            GetWorldData(world).unlockFirstPanel.SetActive(false);
    }

    public void CheckNextWorldPanelToOpen(MusicAsset worldToCheck)
    {
        if (GetNextWorld(worldToCheck) != null && !TimeTravelManager.instance.HasEnteredBefore(worldToCheck.worldID))
        {   
            if (worldToCheck.world != GameWorlds.ModernDay)
            {
                if (GetNextWorld(worldToCheck).unlockFirstPanel.activeSelf)
                    GetNextWorld(worldToCheck).unlockFirstPanel.SetActive(false);
            }

            else
                EnableFinalModernDayRoster();
        }
    }

    IEnumerator WorldLock(MusicAsset world)
    {
        SoundEffectsManager.instance.PlaySoundEffectNC("prize");
        Player.instance.SpendMoney(world.unlockPrice);
        SoundEffectsManager.instance.PlaySoundEffectNC("money");
        GetWorldData(world).worldLock.SetTrigger("lock");
        yield return new WaitForSeconds(1.250f);

        foreach (Image i in GetWorldData(world).plantImages)
            i.color = new Color(1f, 1f, 1f, 1f);

        GetWorldData(world).unlockButton.SetActive(false);
        GetWorldData(world).worldButton.interactable = true;
    }

    WorldMusic GetWorldData(MusicAsset worldAsset)
    {
        for (int i = 0; i < assets.Length; i++)
            if (assets[i].world == worldAsset.world)
                return assets[i];
        
        return null;
    }

    WorldMusic GetNextWorld(MusicAsset _currentWorld)
    {   
        if (_currentWorld.world != GameWorlds.AncientEgypt)
        {
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i].world == _currentWorld.world)
                {   
                    return assets[i + 1];
                }
            }
        }

        return null;
    }

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

    public void ChangeWithoutTransition(MusicAsset worldMusic)
    {   
        if (worldMusic.world != GameWorlds.Tutorial)
        {
            TimeTravelManager.instance.SetAsEntered(worldMusic.worldID);
        }
        
        currentWorld = worldMusic.world;

        ChangeMusic(worldMusic);
        SetTablesInTimeTravel(worldMusic);
        TimeTravelManager.instance.ChangeScenario(worldMusic.worldID, false);
    }

    public void TriggerPanMusicLevel(float setTo, float duration)
    {
        _setTo = setTo;
        _duration = duration;
        panning = true;
    }

    bool panning;
    float _setTo, _duration, _currDur;
    public void PanMusicLevel()
    {
        if (_currDur >= _duration)
        {
            if (source.volume != 1f)
                source.volume += Time.deltaTime;
            
            else
            {
                _currDur = 0f;
                panning = false;
            }
        }

        else
        {
            _currDur += Time.deltaTime;

            if (source.volume != _setTo)
                source.volume = Mathf.Lerp(source.volume, _setTo, 1f * Time.deltaTime);
        }
    }

    public void SetTablesInTimeTravel(MusicAsset active)
    {
        AgeTime[] _ages = TimeTravelManager.instance.worlds;

        for (int i = 0; i < _ages.Length; i++)
        {
            if (_ages[i].worldMusic != active && _ages[i].worldMusic.world != GameWorlds.ThrowbackToThePresent)
            {
                if (_ages[i].worldMusic.world != GameWorlds.ModernDay)
                    _ages[i].worldTables.transform.localPosition += new Vector3(0f, TimeTravelManager.instance.worldTablesYOffset, 0f);
                
                else
                    _ages[i].worldTables.transform.localPosition += new Vector3(0f, TimeTravelManager.instance.modernDayGardenTablesYOffset, 0f);
            }
        }
    }

    public IEnumerator SwapMusicAge(MusicAsset worldMusic)
    {
        if (playLoopCoroutine != null) StopCoroutine(playLoopCoroutine);
        if (mainCoroutine != null) StopCoroutine(mainCoroutine);

        source.Stop();
        source.clip = null;
        source.PlayOneShot(ageTransitionEffect);
        currentWorld = worldMusic.world;
        TimeTravelManager.instance.TriggerTransition(true);
        playingContext = string.Empty;

        Player.instance.SafetyNetsWhenHolding();
        Player.instance.TogglePlayerHands(false);

        yield return new WaitForSeconds(.45f);

        TimeTravelManager.instance.ChangeScenario(worldMusic.worldID, true);

        if (GetNextWorld(worldMusic) != null && !TimeTravelManager.instance.HasEnteredBefore(worldMusic.worldID))
        {   
            if (worldMusic.world != GameWorlds.ModernDay)
            {
                GetWorldData(worldMusic).plantsButton.interactable = true;
                GetNextWorld(worldMusic).unlockFirstPanel.SetActive(false);
            }

            else
            {
                foreach (GameObject g in modernDayFullRoster)
                    g.SetActive(true);
            }
        }

        yield return new WaitForSeconds(ageTransitionEffect.length - 1f);

        if (GameManager.instance.onTutorial)
            RemoveTutorialStuff();

        TimeTravelManager.instance.TriggerTransition(false);

        Player.instance.TogglePlayerHands(true);

        ChangeMusic(worldMusic);

        if (PlantsManager.instance.ChangesInWorld(currentWorld))
            UIManager.instance.TriggerWhileYouWhereGone(currentWorld);

        DataCollector.instance.SetLastVisitedWorld(currentWorld);

        UIManager.instance.SetBankMoneyText(PlantsManager.instance.GetCurrentWorldMoney(currentWorld));
        UIManager.instance.SetBankWorldPanel(currentWorld);
        UIManager.instance.SetBankWorldText(GetFormatedWorldName(currentWorld));
    }

    public string GetFormatedWorldName(GameWorlds world)
    {
        switch (world)
        {
            case GameWorlds.ModernDay: return "Modern Day";
            case GameWorlds.JurassicMarsh: return "Jurassic Marsh";
            case GameWorlds.NeonMixtapeTour: return "Neon Mixtape Tour";
            case GameWorlds.DarkAges: return "Dark Ages";
            case GameWorlds.PirateSeas: return "Pirate Seas";
            case GameWorlds.FarFuture: return "Far Future";
            case GameWorlds.LostCity: return "Lost City";
            case GameWorlds.WildWest: return "Wild West";
            case GameWorlds.BigWaveBeach: return "Big Wave Beach";
            case GameWorlds.FrostbiteCaves: return "Frostbite Caves";
            case GameWorlds.AncientEgypt: return "Ancient Egypt";
            case GameWorlds.Tutorial: return "Tutorial";
        }

        return string.Empty;
    }

    public void RemoveTutorialStuff()
    {
        GameManager.instance.onTutorial = false;
        DataCollector.instance.SetTutorialState(false);

        SeedDatabase.instance.TutorialPlantsSet(true);

        if (!GameManager.instance.unlockedModernDay)
            modernDayUnlockPanel.SetActive(true);

        fullModernDayPanel.SetActive(true);
        modernDayTutorialPanel.SetActive(false);
        tutorialGarden.SetActive(false);
        tutorialGardening.transform.localPosition = new Vector3(0f, -1.75f, 0f);
        fullGardening.transform.localPosition = new Vector3(0f, 0f, 0f);
    }

    public void EnableFinalModernDayRoster()
    {
        foreach (GameObject g in modernDayFullRoster)
            g.SetActive(true);
        
        foreach (Image i in assets[0].plantImages)
            i.color = Color.white;
    }

    public void UnlockFinalModernDay()
    {
        GameManager.instance.unlockedModernDay = true;
        modernDayUnlockPanel.SetActive(false);
    }
}

[System.Serializable]
public class WorldMusic
{
    public GameWorlds world;
    public MusicAsset asset;
    public Button worldButton;
    public Animator worldLock;
    public Text worldPrice;
    public Image[] plantImages;
    public Button plantsButton;
    public GameObject unlockFirstPanel;
    public GameObject unlockButton;
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
