using UnityEngine;
using UnityEngine.Audio;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioMixer audioMixer;

    private bool masterOn = true;
    private bool sfxOn = true;

    public bool IsMasterSoundOn => masterOn;
    public bool IsSfxSoundOn => sfxOn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayClick()
    {
        if (!masterOn || !sfxOn) return;

        audioSource.PlayOneShot(clickSound);
    }

    public void ToggleMasterSound()
    {
        masterOn = !masterOn;

        audioMixer.SetFloat(
            "MasterVolume",
            masterOn ? 0f : -80f
        );
    }

    public void ToggleSfxSound()
    {
        sfxOn = !sfxOn;

        audioMixer.SetFloat(
            "SFXVolume",
            sfxOn ? 0f : -80f
        );
    }
}