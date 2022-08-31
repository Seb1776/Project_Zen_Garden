using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class Pinata : MonoBehaviour
{
    [SerializeField] private PinataAsset pinataData;
    [SerializeField] private SpriteRenderer pinataImage;
    [SerializeField] private string debugSize;
    [SerializeField] private GameObject pinataExplosion;

    private List<GameObject> createdRewards = new List<GameObject>();
    private PinataSize selectedCategory;
    private int squishes, currentSquishes;
    private int totalGivenSeeds;
    private XRGrabInteractable grab;
    private Vector3 startPos;
    private bool returnToPos, squishCooldown, canGrab;
    public SpriteRenderer sr;
    private Collider coll;
    private Transform objTo;
    private Animator anim;

    void Awake()
    {
        grab = transform.GetChild(0).GetComponent<XRGrabInteractable>();
        objTo = transform.GetChild(0).transform;
        coll = objTo.GetComponent<Collider>();
        anim = GetComponent<Animator>();
        sr = objTo.GetComponent<SpriteRenderer>();

        startPos = objTo.position;
        StartCoroutine(InitialAnimation());
    }

    [System.Obsolete]
    void Start()
    {
        grab.onSelectEnter.AddListener(SendPinataToPlayer);
        grab.onSelectExit.AddListener(UnSendPinataToPlayer);
    }

    void Update()
    {
        {
            if (!grab.isSelected && Vector3.Distance(objTo.position, startPos) > 0.1f)
                returnToPos = true;

            if (returnToPos)
                GetBackPos();
        }
    }

    void SetRewardPlants()
    {
        List<List<RewardedPlants>> pinataRewards = UIManager.instance.GetFullPinataRewards();

        for (int i = 0; i < pinataRewards.Count; i++)
            for (int j = 0; j < pinataRewards[i].Count; j++)
                Debug.Log(pinataRewards[i][j].plant.name);

        List<Animator> seedPacketsAnimator = new List<Animator>();

        for (int i = 0; i < pinataRewards.Count; i++)
        {
            for (int j = 0; j < pinataRewards[i].Count; j++)
            {
                GameObject seedP = Instantiate(Resources.Load<GameObject>("Prefabs/UI/" + pinataRewards[i][j].plant.name), transform.position, Quaternion.identity, UIManager.instance.pinataGridPanel.transform);
                seedPacketsAnimator.Add(seedP.GetComponent<Animator>());
                createdRewards.Add(seedP);
                seedP.transform.GetChild(3).GetComponent<Text>().text = "x " + pinataRewards[i][j].givenSeeds.ToString();
                seedP.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                seedP.transform.localPosition = new Vector3(seedP.transform.position.x, seedP.transform.position.y, 0f);
            }
        }

        StartCoroutine(ShowRewards(pinataRewards, seedPacketsAnimator));
    }

    public void DeleteAllCreatedRewards()
    {
        foreach (GameObject g in createdRewards)
            Destroy(g.gameObject);
    }

    public IEnumerator ShowRewards(List<List<RewardedPlants>> pinataRewards, List<Animator> spawnedSeeds)
    {
        for (int i = 0; i < pinataRewards.Count; i++)
        {
            for (int j = 0; j < pinataRewards[i].Count; j++)
            {
                SeedDatabase.instance.BuyPlant(pinataRewards[i][j].plant, pinataRewards[i][j].givenSeeds);
                totalGivenSeeds += pinataRewards[i][j].givenSeeds;
            }
        }
        
        foreach (Animator anim in spawnedSeeds)
            anim.SetTrigger("show_seedpacket");
        
        yield return new WaitForSeconds(3.5f);

        for (int i = 0; i < spawnedSeeds.Count; i++)
        {
            spawnedSeeds[i].SetTrigger("show_plant");
            SoundEffectsManager.instance.PlaySoundEffectNC("selectplant");
            yield return new WaitForSeconds(SoundEffectsManager.instance.GetSoundEffect("selectplant").length + .5f);
        }

        SoundEffectsManager.instance.PlaySoundEffectNC("prize");
        UIManager.instance.pinataRewardText.text = "You got " + totalGivenSeeds + " seeds!";
        UIManager.instance.ActivatePinataContinueButton();

        /*for (int i = 0; i < showPlants.Count; i++)
            totalGivenSeeds += showPlants[i].gotSeeds;

        for (int i = 0; i < showPlants.Count; i++)
        {
            SoundEffectsManager.instance.PlaySoundEffectNC("selectplant");
            GameObject seedPacket = Resources.Load<GameObject>("Prefabs/UI/" + showPlants[i].plant.name);
            GameObject prp = Instantiate(seedPacket, seedPacket.transform.position, Quaternion.identity, UIManager.instance.pinataGridPanel);
            createdRewards.Add(prp);
            prp.transform.GetChild(3).GetComponent<Text>().text = "x " + showPlants[i].gotSeeds.ToString();
            prp.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            prp.transform.localPosition = new Vector3(prp.transform.position.x, prp.transform.position.y, 0f);
            SeedDatabase.instance.BuyPlant(showPlants[i].plant, showPlants[i].gotSeeds);
            yield return new WaitForSeconds(SoundEffectsManager.instance.GetSoundEffect("selectplant").length + .5f);
        }

        */
    }

    public void SetPinataSize(string size)
    {
        switch (size)
        {
            case "s": selectedCategory = pinataData.sizes[0]; break;
            case "m": selectedCategory = pinataData.sizes[1]; break;
            case "l": selectedCategory = pinataData.sizes[2]; break;
            case "xl": selectedCategory = pinataData.sizes[3]; break;
        }

        for (int i = 0; i < pinataData.sizes.Length; i++)
        {   
            if (pinataData.sizes[i].pinataSize == selectedCategory.pinataSize)
            {
                squishes = Random.Range(pinataData.sizes[i].squishesRange.x, pinataData.sizes[i].squishesRange.y);
                break;
            }
        }

        //SetRewardPlants();
    }

    public void Squish()
    {   
        if (!squishCooldown)
        {
            if (currentSquishes >= squishes)
            {
                coll.enabled = false;
                grab.enabled = false;
                sr.enabled = false;
                Player.instance.RecievePinata(null);
                SoundEffectsManager.instance.PlaySoundEffectNC("explosion");
                SoundEffectsManager.instance.PlaySoundEffectNC("prize");
                Instantiate(pinataExplosion, objTo.transform.position, Quaternion.identity);
                SetRewardPlants();
            }

            else
            {
                currentSquishes++;
                SoundEffectsManager.instance.PlaySoundEffectNC("coffee");

                if (GameHelper.GetRandomBool()) anim.SetTrigger("squishA");
                else anim.SetTrigger("squishB");
            }
        }
    }

    IEnumerator SquishCooldown()
    {
        squishCooldown = true;
        yield return new WaitForSeconds(.15f);
        squishCooldown = false;
    }
    
    IEnumerator InitialAnimation()
    {
        yield return new WaitForSeconds(0.25f);
        canGrab = true;
    }

    void SendPinataToPlayer(XRBaseInteractor inter)
    {
        Player.instance.RecievePinata(this);
    }

    void UnSendPinataToPlayer(XRBaseInteractor inter)
    {
        Player.instance.RecievePinata(null);
    }

    void GetBackPos()
    {
        objTo.position = Vector3.Lerp(objTo.position, startPos, 2.5f * Time.deltaTime);
        coll.enabled = false;

        if (Vector3.Distance(objTo.position, startPos) <= 0.01f)
        {
            objTo.position = startPos;
            returnToPos = false;

            if (currentSquishes < squishes)
                coll.enabled = true;
        }
    }
}
