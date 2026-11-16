using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioSource CreateSFX(AudioClip clip, float volume = 1, float pitch = 1, bool loop = false)
    {
        GameObject sfxInstance = new GameObject("SFX_" + clip.name);
        AudioSource source = sfxInstance.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.Play();

        if (!loop)
        {
            Destroy(sfxInstance, clip.length + 0.5f);
        }
        return source;
    }
}