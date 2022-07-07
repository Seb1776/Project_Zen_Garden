using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Music Data", menuName = "Music/Music Data Asset")]
public class MusicAsset : ScriptableObject 
{
    public string worldID;
    public GameWorlds world;
    public MusicCollection worldTracks;
    public AudioClip phonographClip, grownPlantClip;
    public int unlockPrice;

    public MusicSet GetMusicSet(string set)
    {
        switch (set)
        {
            case "seeds": return worldTracks.enteredFirstTime;
            case "first": return worldTracks.firstWave;
            case "mida": return worldTracks.midWaveA;
            case "midb": return worldTracks.midWaveB;
            case "final": return worldTracks.finalWave;
            case "ultimate": return worldTracks.ultimateBattle;
            case "minigame": return worldTracks.minigame;
            case "loonboon": return worldTracks.loonboon;
        }

        return null;
    }

    public List<MusicSet> GetRandomChallenges()
    {
        List<MusicSet> ret = new List<MusicSet>();
        List<MusicSet> unu = new List<MusicSet>();

        unu.Add(GetMusicSet("ultimate")); unu.Add(GetMusicSet("minigame"));
        unu.Add(GetMusicSet("loonboon"));

        for (int i = 0; i < 3; i++)
        {
            int rand = Random.Range(0, unu.Count);
            ret.Add(unu[rand]);
            unu.Remove(unu[rand]);
        }

        return ret;
    }
}

[System.Serializable]
public class MusicCollection
{
    public MusicSet enteredFirstTime;
    public MusicSet firstWave;
    public MusicSet midWaveA, midWaveB;
    public MusicSet finalWave;
    public MusicSet ultimateBattle, minigame, loonboon;
}

[System.Serializable]
public class MusicSet
{
    public AudioClip start, main;
}
