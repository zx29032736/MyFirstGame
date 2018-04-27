using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class soundManager : MonoBehaviour {


    public float masterVolumePercent = 1;
    public float sfxVolumePercent = 1;
    public float backgroundMusicVolumePercent = 0.01f;
    public bool isSoundBanned = false;

   //public List<Sound> soundRepository;
    public AudioClip[] uiAudioClips;
    public Dictionary<string, AudioClip> audioClipLookUp = new Dictionary<string, AudioClip>();

    public AudioSource bgmSource;
    public AudioClip[] bgms;

    private void Start()
    {
        foreach(var go in uiAudioClips)
        {
            if (!audioClipLookUp.ContainsKey(go.name))
                audioClipLookUp.Add(go.name, go);
        }

        OnSettingChanged(PlayerPrefs.GetFloat("MasterVolume", 0.1f), PlayerPrefs.GetFloat("BgmVolume", 1), PlayerPrefs.GetFloat("SfxVolume", 1), System.Convert.ToBoolean(PlayerPrefs.GetInt("SoundEnable", 0)));

        PlayMusic(0);
    }

    public void PlaySound(Vector3 pos, string audioName)
    {
        if(audioClipLookUp.ContainsKey(audioName) && !isSoundBanned)
        {
            AudioSource.PlayClipAtPoint(audioClipLookUp[audioName], pos, sfxVolumePercent* masterVolumePercent);
        }
    }

    public void PlaySound(Vector3 pos, AudioClip clip)
    {
        if (clip != null && !isSoundBanned)
        {
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
        }
    }

    public void PlayMusic(int index)
    {
        //if (!isSoundBanned)
        //{
            bgmSource.clip = bgms[index];
            bgmSource.volume = (float)(backgroundMusicVolumePercent * masterVolumePercent);
            bgmSource.Play();
        //}
    }

    public void StopBackgroundMusic()
    {
        bgmSource.Stop();
    }

    public void OnSettingChanged(float masterV, float bgmV, float fxV, bool banned)
    {
        masterVolumePercent = masterV;
        backgroundMusicVolumePercent = bgmV;
        sfxVolumePercent = fxV;
        isSoundBanned = banned;

        bgmSource.volume = (float)(backgroundMusicVolumePercent * masterVolumePercent);
        bgmSource.mute = isSoundBanned;

    }
}

[System.Serializable]
public class Sound
{
    public string Name;
    public AudioClip clip;
    public SoundType soundType;
}
public enum SoundType { UI, Spell, Other }

