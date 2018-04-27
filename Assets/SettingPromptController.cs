using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPromptController : MonoBehaviour {

    public enum SoundType { Master, BGM, SFX};
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle enableSoundToggle;
    public Button leaveButton;

    public Text playfabID;
    public Text playfabName;

    private void Awake()
    {
        leaveButton.onClick.RemoveAllListeners();
        enableSoundToggle.onValueChanged.RemoveAllListeners();
        masterVolumeSlider.onValueChanged.RemoveAllListeners();
        bgmVolumeSlider.onValueChanged.RemoveAllListeners();
        sfxVolumeSlider.onValueChanged.RemoveAllListeners();

        leaveButton.onClick.AddListener(() => Leave());
        enableSoundToggle.onValueChanged.AddListener((bool isEnable) => OnSliderValueChange());
        masterVolumeSlider.onValueChanged.AddListener((float valueToChange) => OnSliderValueChange());
        bgmVolumeSlider.onValueChanged.AddListener((float valueToChange) => OnSliderValueChange());
        sfxVolumeSlider.onValueChanged.AddListener((float valueToChange) => OnSliderValueChange());
    }

    public void Init()
    {
        masterVolumeSlider.value = GameController.Instance.soundManager.masterVolumePercent;
        bgmVolumeSlider.value = GameController.Instance.soundManager.backgroundMusicVolumePercent;
        sfxVolumeSlider.value = GameController.Instance.soundManager.sfxVolumePercent;
        enableSoundToggle.isOn = GameController.Instance.soundManager.isSoundBanned;

        UpdatePlayerStat();
        gameObject.SetActive(true);
    }

    public void UpdatePlayerStat()
    {
        if(PF_PlayerData.accountInfo != null)
        {
            playfabID.text = PF_PlayerData.accountInfo.PlayFabId;
            playfabName.text = PF_PlayerData.accountInfo.Username;
        }
    }

    public void Leave()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_LEAVE_SOUND_EFFECT);
        gameObject.SetActive(false);
    }

    public void OnSliderValueChange()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("BgmVolume", bgmVolumeSlider.value);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetInt("SoundEnable", System.Convert.ToInt32(enableSoundToggle.isOn));

        GameController.Instance.soundManager.OnSettingChanged(masterVolumeSlider.value, bgmVolumeSlider.value, sfxVolumeSlider.value, enableSoundToggle.isOn);
    }
}
