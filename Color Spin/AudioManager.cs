    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        public AudioSource backgroundMusic;
        public AudioSource soundEffects;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                LoadVolumeSettings();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void SetBackgroundVolume(float volume)
        {
            backgroundMusic.volume = volume;
            PlayerPrefs.SetFloat("BackgroundVolume", volume);
            Debug.Log($"Background volume set to: {volume}");
        }

        public void SetSoundEffectsVolume(float volume)
        {
            soundEffects.volume = volume;
            PlayerPrefs.SetFloat("SoundEffectsVolume", volume);
            Debug.Log($"Sound effects volume set to: {volume}");
        }

        private void LoadVolumeSettings()
        {
            if (PlayerPrefs.HasKey("BackgroundVolume"))
            {
                backgroundMusic.volume = PlayerPrefs.GetFloat("BackgroundVolume");
            }
            if (PlayerPrefs.HasKey("SoundEffectsVolume"))
            {
                soundEffects.volume = PlayerPrefs.GetFloat("SoundEffectsVolume");
            }
        }

        public void PlayEffect(AudioClip clip)
        {
            soundEffects.PlayOneShot(clip);
        }
    }
