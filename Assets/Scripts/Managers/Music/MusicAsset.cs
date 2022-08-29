using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Music Data", menuName = "Music/Music Data Asset")]
public class MusicAsset : ScriptableObject 
{
    public string worldID;
    public GameWorlds world;
    public List<AudioClip> normalTracks = new List<AudioClip>();
    public List<AudioClip> specialTracks = new List<AudioClip>();
    public AudioClip phonographClip, grownPlantClip;
    public bool isPresent;
    public int unlockPrice;

    public List<AudioClip> GetMusicClips(SetMusicMode context, bool random)
    {
        List<AudioClip> retList = new List<AudioClip>();

        switch (context)
        {
            case SetMusicMode.Start:
                foreach (AudioClip ac in normalTracks)
                    retList.Add(ac);
            break;

            case SetMusicMode.Main:
                for (int i = 1; i < normalTracks.Count; i++)
                    retList.Add(normalTracks[i]);
            break;

            case SetMusicMode.Special:

                if (!random)
                {
                    foreach (AudioClip ac in specialTracks)
                        retList.Add(ac);
                }
                
                else
                {
                    List<AudioClip> unused = new List<AudioClip>();

                    foreach (AudioClip ac in specialTracks)
                        unused.Add(ac);
                    
                    for (int i = 0; i < unused.Count + 2; i++)
                    {
                        int randInt = Random.Range(0, unused.Count);
                        retList.Add(unused[randInt]);
                        unused.Remove(unused[randInt]);
                    }
                }
            break;
        }

        return retList;
    }
}

[System.Serializable]
public class MusicCollection
{
    public List<AudioClip> musicCollectionTracks = new List<AudioClip>();
    public MusicSet enteredFirstTime;
    public MusicSet firstWave;
    public MusicSet midWaveA, midWaveB;
    public MusicSet finalWave;
    public MusicSet ultimateBattle, minigame, loonboon;
}

[System.Serializable]
public class MusicCollectionPresent
{
    [Header ("Entered")]
    public MusicSet entered;
    public MusicSet seeds;
    [Header ("Front")]
    public MusicSet frontDay;
    public MusicSet frontNight;
    [Header ("Back")]
    public MusicSet backDay;
    public MusicSet backNight;
    [Header ("Roof")]
    public MusicSet roofDay;
    public MusicSet roofNight;
    [Header ("Special")]
    public MusicSet minigame; 
    public MusicSet ultimateA, ultimateB, loonboon, cerebrawl, brainiac;
}

[System.Serializable]
public class MusicSet
{
    public AudioClip start, main;
}

public enum SetMusicMode
{
    Start, Main, Special
}
