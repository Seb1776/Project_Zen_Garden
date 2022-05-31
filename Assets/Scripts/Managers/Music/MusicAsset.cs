using UnityEngine;

[CreateAssetMenu(fileName = "New Music Data", menuName = "Music/Music Data Asset")]
public class MusicAsset : ScriptableObject 
{
    public MusicCollection[] worldTracks;
}

[System.Serializable]
public class MusicCollection
{
    public AudioClip startTrack, musicTrack;
}
