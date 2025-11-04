using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AudioEnums;
using R3;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;

public class AudioManager : Singleton<AudioManager>
{
    [Header("오디오 소스")]
    [SerializeField] private AudioSource mBgmSource;
    [SerializeField] private AudioSource mSfxSource;
    [SerializeField] private AudioSource mUiSource;

    [Header("오디오 클립")]
    [SerializeField] private AudioClip[] mUiClips;
    
    [Header("오브젝트 풀 클립")]
    [SerializeField] private AudioClip[] mPooledSfxClips;
    [SerializeField] private AudioClip[] mBgmClips; // 0: 인트로, 1: 마을, 2: 필드, 3: 던전

    [Header("오브젝트 풀 세팅")]
    [SerializeField] private int mSfxPoolSize = 16;
    [SerializeField] private int mBgmPoolSize = 2;
    private AudioSource[] mPooledSources;
    private AudioSource[] mBgmSources;
    private int mNextPool = 0;
    private int mNextPoolLoop = 0;
    private int mNextBgm = 0;

    // 루프용 식셔너리
    private Dictionary<ExSfxType, AudioSource> mLoopExSfxSources = new Dictionary<ExSfxType, AudioSource>();


    #region Player SFX 클립

    [Space(10)]
    [Header("SFX 클립")]
    public AudioClip[] footstepAudioClips;
    public AudioClip[] gruntVoiceAudioClips;
    public AudioClip[] landingVoiceAudioClips;
    public AudioClip[] landingAudioClips;
    public AudioClip[] attackVoiceAudioClips;
    public AudioClip[] swordSwingAudioClips;
    public AudioClip[] swordHitAudioClips;
    public AudioClip[] hitVoiceAudioClips;
    public AudioClip[] hitAudioClips;
    public AudioClip[] blockShieldAudioClips;
    public AudioClip[] stunVoiceAudioClips;
    public AudioClip[] deathVoiceAudioClips;
    public AudioClip[] skillVoiceAudioClips;
    public AudioClip[] projectileFireAudioClips;
    public AudioClip[] skill1AudioClips;
    public AudioClip[] skill2AudioClips;
    public AudioClip[] skill3AudioClips;
    public AudioClip[] skill4AudioClips;
    public AudioClip[] interactionVoiceAudioClips;
    
    private Dictionary<ESfxType, AudioClip[]> mSfxClips;

    #endregion
    
    
    
    protected override void Awake()
    {
        base.Awake();
        InitPlayerSfx();
        mPooledSources = new AudioSource[mSfxPoolSize];
        for (int i = 0; i < mSfxPoolSize; i++)
        {
            var go = new GameObject($"PooledSfx_{i}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            mPooledSources[i] = src;
        }
        mBgmSources = new AudioSource[mBgmPoolSize];
        mBgmSources[0] = mBgmSource;
        for (int i = 1; i < mBgmPoolSize; i++)
        {
            var go = new GameObject($"BgmSrc_{i}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            mBgmSources[i] = src;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
  
    /// <summary>
    /// Init volume and mute data from UserData in ManagerHub
    /// </summary>
    public void InitAudioDataFromUserData()
    {
        AudioListener.volume = UserData.Instance.MasterVolume;
        mBgmSource.volume = UserData.Instance.BgmVolume;
        mSfxSource.volume = UserData.Instance.SeVolume;
        mUiSource.volume = UserData.Instance.UiVolume;

        AudioListener.pause = UserData.Instance.IsMasterMuted;
        mBgmSource.mute = UserData.Instance.IsBgmMuted;
        mSfxSource.mute = UserData.Instance.IsSeMuted;
        mUiSource.mute = UserData.Instance.IsUiMuted;

        foreach (var src in mBgmSources)
        {
            src.volume = UserData.Instance.BgmVolume;
            src.mute = UserData.Instance.IsBgmMuted;
            src.loop = true;
        }
        foreach (var src in mPooledSources)
        {
            src.volume = UserData.Instance.SeVolume;
            src.mute = UserData.Instance.IsSeMuted;
        }


        subscribeUserAudioData();
    }

    private void subscribeUserAudioData()
    {
        UserData.Instance.ObsMasterVolume.Subscribe(e => AudioListener.volume = e).AddTo(this);
        UserData.Instance.ObsBgmVolume
            .Subscribe(e =>
            {
                mBgmSource.volume = e;
                foreach (var src in mBgmSources)
                {
                    src.volume = e;
                }
            }).AddTo(this);
        UserData.Instance.ObsSeVolume
            .Subscribe(e =>
            {
                mSfxSource.volume = e;
                foreach (var src in mPooledSources)
                {
                    src.volume = e;
                }
            }).AddTo(this);
        UserData.Instance.ObsUiVolume.Subscribe(e => mUiSource.volume = e).AddTo(this);


        UserData.Instance.ObsMasterMuted.Subscribe(e => AudioListener.pause = e).AddTo(this);
        UserData.Instance.ObsBgmMuted
            .Subscribe(e =>
            {
                mBgmSource.mute = e;
                foreach (var src in mBgmSources)
                {
                    src.mute = e;
                }
            }).AddTo(this);
        UserData.Instance.ObsSeMuted
            .Subscribe(e =>
            {
                mSfxSource.mute = e;
                foreach (var src in mPooledSources)
                {
                    src.mute = e;
                }
            }).AddTo(this);
        UserData.Instance.ObsUiMuted.Subscribe(e => mUiSource.mute = e).AddTo(this);
    }

    private void InitPlayerSfx()
    {
        mSfxClips = new Dictionary<ESfxType, AudioClip[]>
        {
            { ESfxType.FootstepEffect, footstepAudioClips },
            { ESfxType.GruntVoice, gruntVoiceAudioClips },
            { ESfxType.LandVoice, landingVoiceAudioClips },
            { ESfxType.LandEffect, landingAudioClips },
            { ESfxType.AttackVoice, attackVoiceAudioClips },
            { ESfxType.SwordSwingEffect, swordSwingAudioClips },
            { ESfxType.EnemyHitEffect, swordHitAudioClips },
            { ESfxType.PlayerHitVoice, hitVoiceAudioClips },
            { ESfxType.PlayerHitEffect, hitAudioClips },
            { ESfxType.ShieldBlockEffect, blockShieldAudioClips },
            { ESfxType.StunVoice, stunVoiceAudioClips },
            { ESfxType.DeathVoice, deathVoiceAudioClips },
            { ESfxType.SkillVoice, skillVoiceAudioClips },
            { ESfxType.ProjectileFire, projectileFireAudioClips },
            { ESfxType.Skill1Effect, skill1AudioClips },
            { ESfxType.Skill2Effect, skill2AudioClips },
            { ESfxType.Skill3Effect, skill3AudioClips },
            { ESfxType.Skill4Effect, skill4AudioClips },
            { ESfxType.InteractionVoice, interactionVoiceAudioClips }
        };
    }

    public void PlayBgm(EBgmType type)
    {
        int idx = (int)type;
        if (mBgmClips == null || idx < 0 || idx >= mBgmClips.Length) return;
        var clip = mBgmClips[idx];
        if (clip == null) return;

        // 이전 BGM 정지
        int prev = (mNextBgm + mBgmPoolSize - 1) % mBgmPoolSize;
        mBgmSources[prev].Stop();

        // 다음 풀 소스에서 재생
        var src = mBgmSources[mNextBgm];
        src.clip = clip;
        src.Play();
        mNextBgm = (mNextBgm + 1) % mBgmPoolSize;
    }

    public void StopBgm()
    {
        foreach (var src in mBgmSources)
            if (src.isPlaying) src.Stop();
    }

    public void PlaySfx(ESfxType type)
    {
        if (mSfxClips == null || !mSfxClips.TryGetValue(type, out var clips) || clips == null || clips.Length == 0) return;
        var clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;
        mSfxSource.PlayOneShot(clip);
    }

    public void PlayPoolSfx(ExSfxType type)
    {
        int idx = (int)type;
        if (mPooledSfxClips == null || idx < 0 || idx >= mPooledSfxClips.Length) return;
        var clip = mPooledSfxClips[idx];
        if (clip == null) return;
        var src = mPooledSources[mNextPool];
        mNextPool = (mNextPool + 1) % mSfxPoolSize;
        src.PlayOneShot(mPooledSfxClips[idx]);
    }
    public void PlayUi(EUiType type)
    {
        int idx = (int)type;
        if (mUiClips == null || idx < 0 || idx >= mUiClips.Length) return;
        var clip = mUiClips[idx];
        if (clip == null) return;
        mUiSource.PlayOneShot(clip);
    }

    // ExSfx에서 특정 SFX 루프
    public void PlayLoopPoolSfx(ExSfxType type)
    {
        if (mLoopExSfxSources.ContainsKey(type))
            return;

        int idx = (int)type;
        if (mPooledSfxClips == null || idx < 0 || idx >= mPooledSfxClips.Length) return;
        var clip = mPooledSfxClips[idx];
        if (clip == null) return;

        var src = mPooledSources[mNextPoolLoop];
        mNextPoolLoop = (mNextPoolLoop + 1) % mSfxPoolSize;

        src.clip = mPooledSfxClips[idx];
        src.loop = true;
        src.Play();
        mLoopExSfxSources[type] = src;
    }

    public void StopLoopPoolSfx(ExSfxType type)
    {
        if (!mLoopExSfxSources.TryGetValue(type, out var src))
            return;

        src.Stop();
        src.loop = false;
        src.clip = null;
        mLoopExSfxSources.Remove(type);
    }
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case Constants.MainMenuScene:
                PlayBgm(EBgmType.Intro);
                break;
            case Constants.TownScene:
                PlayBgm(EBgmType.Town);
                break;
            case  Constants.AbyssFieldScene:
                PlayBgm(EBgmType.Field);
                break;
            case Constants.AbyssDungeonScene:
                PlayBgm(EBgmType.Dungeon);
                break;
            default:
                StopBgm();
                break;
        }
    }
}