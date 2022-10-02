using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEffect
{
    public AudioClip clip;
    public float volume = 1.0f;
    public float pitchFluctuation;
}

public class AudioComponent : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioSource pitchySource;

    private void Awake()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if(sources.Length != 2)
        {
            Debug.LogError("Not 2 audio sources!");
        }
        audioSource = sources[0];
        pitchySource = sources[1];
    }

    private void Reset()
    {
        int numSources = GetComponents<AudioSource>().Length;
        for(int i = 0; i < 2 - numSources; i++)
        {
            gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySound(SoundEffect sound)
    {
        if(sound == null)
        {
            return;
        }
        if(sound.clip != null)
        {
            if (sound.pitchFluctuation != 0)
            {
                pitchySource.pitch = 1 + Random.Range(0f - (sound.pitchFluctuation), 0f + (sound.pitchFluctuation));
                pitchySource.PlayOneShot(sound.clip, sound.volume);
            } else
            {
                audioSource.PlayOneShot(sound.clip, sound.volume);
            }
        }
    }

    public void PlaySoundAnim(AnimationEvent sound)
    {
        if(sound.objectReferenceParameter is AudioClip clip)
        if (sound.intParameter != 0)
        {
            pitchySource.pitch = 1 + Random.Range(0f - (sound.intParameter * .1f), 0f + (sound.intParameter * .1f));
            pitchySource.PlayOneShot(clip, sound.floatParameter);
        } else
        {
            audioSource.PlayOneShot(clip, sound.floatParameter);
        }
    }

    public void PlayDisposable(SoundEffect sound)
    {
        PlayClipAt(sound, transform.position);
    }

    public AudioSource PlayClipAt(SoundEffect sound, Vector3 pos)
    {
        GameObject temp = new GameObject("TempAudio");
        temp.transform.position = pos;
        AudioSource tempSource = temp.AddComponent<AudioSource>();
        tempSource.clip = sound.clip;
        tempSource.volume = sound.volume;
        tempSource.pitch = 1 + Random.Range(0f - (sound.pitchFluctuation), 0f + (sound.pitchFluctuation));
        tempSource.Play();
        Destroy(temp, sound.clip.length);
        return tempSource;
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying || pitchySource.isPlaying;
    }
}
