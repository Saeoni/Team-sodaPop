using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class optionsScreen : MonoBehaviour
{
    public Slider masterSlider, musicSlider, sfxSlider;
    public AudioMixer theMixer;
    public TMP_Text masterLabel, musicLabel, sfxLabel;
    public TMP_Text resoultionLabel;
    public Toggle fullscreenTog, vsyncTog;
    public List<ResItem> resoultions = new();

    public Slider sensitivitySlider;
    private int selectedResoultion;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        fullscreenTog.isOn = Screen.fullScreen;
        if (QualitySettings.vSyncCount == 0)
            vsyncTog.isOn = false;
        else
            vsyncTog.isOn = true;

        var foundRes = false;
        for (var i = 0; i < resoultions.Count; i++)
            if (Screen.width == resoultions[i].horizontial && Screen.height == resoultions[i].vertical)
            {
                foundRes = true;

                selectedResoultion = i;

                UpdateResLabel();
            }

        if (!foundRes)
        {
            var newRes = new ResItem();
            newRes.horizontial = Screen.width;
            newRes.vertical = Screen.height;

            resoultions.Add(newRes);
            selectedResoultion = resoultions.Count - 1;
            UpdateResLabel();
        }

        var vol = 0f;
        theMixer.GetFloat("MasterVol", out vol);
        masterSlider.value = vol;
        theMixer.GetFloat("MusicVol", out vol);
        musicSlider.value = vol;
        theMixer.GetFloat("SFXVol", out vol);
        sfxSlider.value = vol;
        masterLabel.text = Mathf.RoundToInt(masterSlider.value + 80).ToString();
        musicLabel.text = Mathf.RoundToInt(musicSlider.value + 80).ToString();
        sfxLabel.text = Mathf.RoundToInt(sfxSlider.value + 80).ToString();
        sensitivitySlider.value = PlayerPrefs.GetFloat("mouseSensitivity", 300f);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void ResLeft()
    {
        selectedResoultion--;
        if (selectedResoultion < 0) selectedResoultion = 0;
        UpdateResLabel();
    }

    public void ResRight()
    {
        selectedResoultion++;
        if (selectedResoultion > resoultions.Count - 1) selectedResoultion = resoultions.Count - 1;
        UpdateResLabel();
    }

    public void UpdateResLabel()
    {
        resoultionLabel.text = resoultions[selectedResoultion].horizontial + " x " +
                               resoultions[selectedResoultion].vertical;
    }

    public void ApplyGraphics()
    {
        //Screen.fullScreen = fullscreenTog.isOn;

        if (vsyncTog.isOn)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;

        Screen.SetResolution(resoultions[selectedResoultion].horizontial, resoultions[selectedResoultion].vertical,
            fullscreenTog.isOn);
    }

    public void SetMasterVol()
    {
        masterLabel.text = Mathf.RoundToInt(masterSlider.value + 80).ToString();
        theMixer.SetFloat("MasterVol", masterSlider.value);

        PlayerPrefs.SetFloat("MasterVol", masterSlider.value);
    }

    public void SetMusicVol()
    {
        musicLabel.text = Mathf.RoundToInt(musicSlider.value + 80).ToString();
        theMixer.SetFloat("MusicVol", musicSlider.value);

        PlayerPrefs.SetFloat("MusicVol", musicSlider.value);
    }

    public void SetSFXVol()
    {
        sfxLabel.text = Mathf.RoundToInt(sfxSlider.value + 80).ToString();
        theMixer.SetFloat("SFXVol", sfxSlider.value);

        PlayerPrefs.SetFloat("SFXVol", sfxSlider.value);
    }

     public void OnSensitivityChanged(float value)
    {
        //mouseSensitivity = value;
        PlayerPrefs.SetFloat("mouseSensitivity", value);
    }
}

[Serializable]
public class ResItem
{
    public int horizontial, vertical;
}

