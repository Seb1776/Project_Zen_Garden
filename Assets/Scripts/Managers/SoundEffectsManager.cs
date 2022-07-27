using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundEffectsManager : MonoBehaviour
{
    public static SoundEffectsManager instance;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    [SerializeField] private SoundEffect[] soundEffects;
    [SerializeField] private AudioMixerGroup sxfGroup;

    public void PlaySoundEffectNC(string _id)
    {
        StartCoroutine(PlaySoundEffect(_id));
    }

    public AudioClip GetSoundEffect (string _id)
    {
        foreach (SoundEffect se in soundEffects)
            if (se.audioID == _id)
                return se.clip;

        return null;
    }

    public IEnumerator PlaySoundEffect(string _id)
    {
        AudioClip _p = null;

        foreach (SoundEffect se in soundEffects)
        {   
            if (se.audioID == _id)
            {
                _p = se.clip;
                break;
            }
        }

        if (_p == null)
        {
            Debug.LogWarning("Sound not found");
            yield return null;
        }
        
        AudioSource _src = gameObject.AddComponent<AudioSource>();
        _src.outputAudioMixerGroup = sxfGroup;
        _src.PlayOneShot(_p);

        yield return new WaitForSeconds(_p.length);
        Destroy(_src);
    }
}

[System.Serializable]
public class SoundEffect
{
    public AudioClip clip;
    public string audioID; 
}
