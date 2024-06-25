using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SetSound : MonoBehaviour
{
    public AudioMixer SFX_Mixer;
    public Slider SFX_Slider;
    void Start()
    {
        SFX_Slider.value = PlayerPrefs.GetFloat("SFX", 0.0001f);
    }
    public void setSFXVol(float sfx_value)
    {
        SFX_Mixer.SetFloat("SFX Vol", Mathf.Log10(sfx_value)*20);
        PlayerPrefs.SetFloat("SFX", sfx_value);
    }
}
