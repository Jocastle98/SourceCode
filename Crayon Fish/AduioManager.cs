using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class AudioManager : Singleton<AudioManager>
{
    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip[] bgmClips; // 0: 인트로, 1: 게임

    [Header("SFX")]
    // 0: 점수 획득 시 , 1: 팝업 열기 , 2: 팝업 닫기, 3: 물고기 진화 시 , 4: 물고기 죽을 때  , 5: 동료 물고기 , 6: 황금물고기(점수2배) , 7: 포션 마시기, 8: 무적 방울, 9: 컷씬 소리
    // 10: 문어 공격, 11: 해마 공격, 12: 복어 공격, 13: 게 공격, 14: 낚시꾼 공격, 15: 문어 등장, 16: 문어 터치 , 17: 플레이어 배고픔
    [SerializeField] private AudioClip[] sfxClips; 
    [SerializeField] private int sfxPoolSize = 10;
    private AudioSource[] sfxPool;
    private int nextSfxIndex = 0;
    


    private float bgmVolume;
    private float sfxVolume;
    private bool isBgmMuted;
    private bool isSfxMuted;
    protected override void Awake()
    {
        base.Awake();
        InitSfxPool();
        LoadPref(); 
        ApplyAudioSetting();
    }
    
    // 효과음 깨지는 거 방지 위한 오브젝트 풀링
    private void InitSfxPool()
    {
        sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            var go = new GameObject($"SFX_{i}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            sfxPool[i] = src;
        }
    }
    /// <summary>
    /// 볼륨 값 로드 
    /// </summary>
    private void LoadPref()
    {
        bgmVolume    = PlayerPrefs.GetFloat(Constants.BGMVolumeKey, 1f);
        sfxVolume    = PlayerPrefs.GetFloat(Constants.SFXVolumeKey, 1f);
    }
    /// <summary>
    /// 현재 볼륨 값 저장
    /// </summary>
    private void SavePref()
    {
        PlayerPrefs.SetFloat(Constants.BGMVolumeKey, bgmVolume);
        PlayerPrefs.SetFloat(Constants.SFXVolumeKey, sfxVolume);
        PlayerPrefs.Save();
    }
    /// <summary>
    /// 볼륨 적용
    /// </summary>
    private void ApplyAudioSetting()
    {
        // BGM
        bgmSource.volume = bgmVolume;                
        bgmSource.mute   = Mathf.Approximately(bgmVolume, 0f);

        // SFX
        foreach (var src in sfxPool)
        {
            src.volume = sfxVolume;
            src.mute   = Mathf.Approximately(sfxVolume, 0f);
        }
    }
    // 외부 호출용 Getter
    public float GetBgmVolume() => bgmVolume;
    public float GetSfxVolume() => sfxVolume;

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);       
        ApplyAudioSetting();
        SavePref();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyAudioSetting();
        SavePref();
    }

    public void SetBgmMute(bool mute)
    {
        isBgmMuted = mute;
        if (bgmSource != null)
        {
            bgmSource.mute = mute;
        }
    }

    public void SetSfxMute(bool mute)
    {
        isSfxMuted = mute;
        if (sfxPool != null)
        {
            foreach (var src in sfxPool)
            {
                if (src != null)
                    src.mute = mute;
            }
        }
    }

    // 효과음 재생 
    public void PlaySfx(int index)
    {
        if (sfxClips == null || index < 0 || index >= sfxClips.Length) return;
        var clip = sfxClips[index];
        if (clip == null) return;

        var src = sfxPool[nextSfxIndex];
        src.PlayOneShot(clip);

        nextSfxIndex = (nextSfxIndex + 1) % sfxPoolSize;
    }
    
    // 배경음 재생
    public void PlayBgm(int index)
    {
        if (bgmClips == null || index < 0 || index >= bgmClips.Length) return;
        var clip = bgmClips[index];
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Boot": 
                AudioManager.Instance.PlayBgm(0);     
                break;
            case "Tutorial":
                AudioManager.Instance.PlayBgm(0);
                break;
            case "StartScene":
                AudioManager.Instance.PlayBgm(0);
                break;
            case "PlayScene": 
                AudioManager.Instance.PlayBgm(0);     
                break;
            default:
                break;
        }
    }
    
    AudioSource GetFreeSfxSource()
    {
        var src = sfxPool[nextSfxIndex];
        nextSfxIndex = (nextSfxIndex + 1) % sfxPoolSize;
        return src;
    }
    
    // <summary>
    /// 해마처럼 '개별 Pause/UnPause'가 필요한 사운드용 함수
    /// </summary>
    public AudioSource PlaySfxReturn(int index, bool loop = false, float volume = 1f)
    {
        if (sfxClips == null || index < 0 || index >= sfxClips.Length) return null;

        var src = GetFreeSfxSource();
        src.clip   = sfxClips[index];
        src.loop   = loop;
        src.volume = volume;
        src.time   = 0f;
        src.Play();   // PlayOneShot 사용하지 않음

        return src;
    }
    public void ToggleAudioFromBridge(bool pause) => ToggleAudio(pause);

    void ToggleAudio(bool pause)
    {
        AudioListener.pause = pause;

        if (pause)
        {
            if (bgmSource && bgmSource.isPlaying) bgmSource.Pause();
            foreach (var s in sfxPool) if (s.isPlaying) s.Pause();
        }
        else
        {
            if (bgmSource && !bgmSource.isPlaying && bgmSource.clip) bgmSource.UnPause();
            foreach (var s in sfxPool) if (s.clip) s.UnPause();
        }
    }
}
