using UnityEngine;

[CreateAssetMenu(fileName = "New Music Data", menuName = "Music/Music Data Asset")]
public class MusicAsset : ScriptableObject 
{
    public string worldID;
    public GameWorlds world;
    public MusicCollection[] worldTracks;
    public AudioClip phonographClip, grownPlantClip;
    public int unlockPrice;
}

[System.Serializable]
public class MusicCollection
{
    public AudioClip startTrack, musicTrack;
}
