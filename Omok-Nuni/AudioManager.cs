using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AudioEnums;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _forestSource;

    [Header("Audio Clips")]
    // 0: 인트로, 1: 메인, 2: 바둑 둘 때, 3: 새 소리 4: 클릭소리, 5: 코인, 6: 많은 코인
    // 7: 나뭇가지 흔들림 , 8: 게임 씬 새 소리 
    [SerializeField] private AudioClip[] _audioClips;

    private void Start()
    {
        float bgmVolume = PlayerPrefs.GetFloat(Constants.BGMVolumeKey, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(Constants.SFXVolumeKey, 1f);

        if (_bgmSource != null)
        {
            _bgmSource.volume = bgmVolume;
        }
        if (_sfxSource != null)
        {
            _sfxSource.volume = sfxVolume;
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    /// <summary>
    /// SettingPanel 에서 슬라이더 값 변경시 호출
    /// </summary>
    public void InitSliders(Slider bgmSlider, Slider sfxSlider)
    {
        float bgmVolume = PlayerPrefs.GetFloat(Constants.BGMVolumeKey, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(Constants.SFXVolumeKey, 1f);

        if (bgmSlider != null)
        {
            bgmSlider.value = bgmVolume;
            bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
    }

    public void PlayIntroBgm()
    {
        if (_bgmSource != null && _audioClips.Length > 0 && _audioClips[0] != null)
        {
            _bgmSource.clip = _audioClips[0];
            _bgmSource.loop = true;
            _bgmSource.Play();
        }
    }

    public void PlayGameBgm()
    {
        if (_bgmSource != null && _audioClips.Length > 1 && _audioClips[1] != null)
        {
            _bgmSource.clip = _audioClips[1];
            _bgmSource.loop = true;
            _bgmSource.Play();
        }
        if (_forestSource != null && _audioClips.Length > 8 && _audioClips[8] != null)
        {
            _forestSource.clip = _audioClips[8];
            _forestSource.loop = true;
            _forestSource.Play();
        }
    }

    public void StopBgm()
    {
        if (_bgmSource != null && _audioClips.Length > 1 && _audioClips[1] != null)
        {
            _bgmSource.Stop();
        }
        if (_forestSource != null && _audioClips.Length > 8 && _audioClips[8] != null)
        {
            _forestSource.Stop();
        }
    }

    public void PlaySfxSound(int index)
    {
        if (_sfxSource != null && index >= 2 && index < _audioClips.Length && _audioClips[index] != null)
        {
            _sfxSource.PlayOneShot(_audioClips[index]);
        }
    }

    public void PlayAudioClip(ESfxType clipType)
    {
        if (clipType == ESfxType.IntroBGM)
        {
            PlayIntroBgm();
        }
        else if (clipType == ESfxType.GameBGM)
        {
            PlayGameBgm();
        }
        else
        {
            PlaySfxSound((int)clipType);
        }
    }

    private void OnBgmVolumeChanged(float volume)
    {
        if (_bgmSource != null)
        {
            _bgmSource.volume = volume;
            PlayerPrefs.SetFloat(Constants.BGMVolumeKey, volume);
            PlayerPrefs.Save();
        }
        if (_forestSource != null)
        {
            _forestSource.volume = volume;
            PlayerPrefs.SetFloat(Constants.BGMVolumeKey, volume);
            PlayerPrefs.Save();
        }
    }

    private void OnSfxVolumeChanged(float volume)
    {
        if (_sfxSource != null)
        {
            _sfxSource.volume = volume;
            PlayerPrefs.SetFloat(Constants.SFXVolumeKey, volume);
            PlayerPrefs.Save();
        }
    }
}
