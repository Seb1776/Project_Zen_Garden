using UnityEngine;

[CreateAssetMenu(fileName = "New Music Data", menuName = "Music/Music Data Asset")]
public class MusicAsset : ScriptableObject 
{
    public MusicCollection[] worldTracks;
    public AudioClip phonographClip, grownPlantClip;
}

[System.Serializable]
public class MusicCollection
{
    public AudioClip startTrack, musicTrack;
}
