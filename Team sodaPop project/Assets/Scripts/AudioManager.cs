using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer theMixer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (PlayerPrefs.HasKey("MasterVol")) theMixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
        if (PlayerPrefs.HasKey("MusicVol")) theMixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));
        if (PlayerPrefs.HasKey("SFXVol")) theMixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));
    }

    // Update is called once per frame
    private void Update()
    {
    }
}