using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanelController : MonoBehaviour
{

    [Header("BGM")]
    [SerializeField] private Toggle bgmToggle;
    [SerializeField] private Image bgmIcon;            // 아이콘으로 사용할 Image
    [SerializeField] private Sprite bgmOnSprite;       // 켜짐 스프라이트
    [SerializeField] private Sprite bgmOffSprite;      // 꺼짐 스프라이트

    [Header("SFX")]
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Image sfxIcon;            // 아이콘으로 사용할 Image
    [SerializeField] private Sprite sfxOnSprite;       // 켜짐 스프라이트
    [SerializeField] private Sprite sfxOffSprite;      // 꺼짐 스프라이트

    private Joystick dynamicJoystick;
    private Joystick floatingJoystick;

    [Header("UI 참조 (Text, Buttons)")]
    public TMP_Text joystickNameText;
    public Button leftButton, rightButton, selectButton;

    private Joystick[] joysticks;
    private string[] names = { "다이나믹 조이스틱", "플로팅 조이스틱" };
    private int current, saved;

    private void Awake()
    {
        // PlayerPrefs에 BGM/SFX 볼륨이 0으로 저장되어 있으면 1로 초기화 (최초 실행 또는 강제 리셋)
        if (PlayerPrefs.HasKey(Constants.BGMVolumeKey) && PlayerPrefs.GetFloat(Constants.BGMVolumeKey) <= 0.01f)
        {
            PlayerPrefs.SetFloat(Constants.BGMVolumeKey, 1f);
            PlayerPrefs.Save();
        }
        if (PlayerPrefs.HasKey(Constants.SFXVolumeKey) && PlayerPrefs.GetFloat(Constants.SFXVolumeKey) <= 0.01f)
        {
            PlayerPrefs.SetFloat(Constants.SFXVolumeKey, 1f);
            PlayerPrefs.Save();
        }

        // 저장된 값으로 초기 토글 상태 세팅
        bool sfxOn = AudioManager.Instance.GetSfxVolume() > 0.01f;
        sfxToggle.SetIsOnWithoutNotify(sfxOn);
        sfxIcon.sprite = sfxOn ? sfxOnSprite : sfxOffSprite;
        AudioManager.Instance.SetSfxMute(!sfxOn);

        bool bgmOn = AudioManager.Instance.GetBgmVolume() > 0.01f;
        bgmToggle.SetIsOnWithoutNotify(bgmOn);
        bgmIcon.sprite = bgmOn ? bgmOnSprite : bgmOffSprite;
        AudioManager.Instance.SetBgmMute(!bgmOn);

        // 기존 리스너 제거 후 등록
        sfxToggle.onValueChanged.RemoveAllListeners();
        bgmToggle.onValueChanged.RemoveAllListeners();
        sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
        bgmToggle.onValueChanged.AddListener(OnBgmToggleChanged);

        // 1) UI 루트(Canvas) 찾기 (이름은 씬에 맞춰 바꿔주세요)
        var uiRoot = GameObject.Find("Joystick_Canvas")?.transform;
        Debug.Log($"uiRoot : {uiRoot}");
        if (uiRoot == null)
        {
            Debug.LogError("Joystick_Canvas를 찾을 수 없습니다.");
            return;
        }

        // 2) 자식으로 배치된 조이스틱 오브젝트 찾기
        //    (inactive 상태여도 Transform.Find는 탐색됩니다)
        var dynTf = uiRoot.Find("Dynamic Joystick");
        var fltTf = uiRoot.Find("Floating Joystick");
        if (dynTf == null || fltTf == null)
        {
            Debug.LogError("DynamicJoystick 또는 FloatingJoystick 오브젝트가 Joystick_Canvas 하위에 없습니다.");
            return;
        }

        dynamicJoystick = dynTf.GetComponent<DynamicJoystick>();
        floatingJoystick = fltTf.GetComponent<FloatingJoystick>();

        // 3) 배열 세팅
        joysticks = new Joystick[] { dynamicJoystick, floatingJoystick };

        // 4) 저장된 인덱스 불러오기
        saved = PlayerPrefs.GetInt("SelectedJoystick", 1);
        current = saved;

        UpdateUI();
        ApplySelection();

        // 5) 버튼 이벤트
        leftButton.onClick.AddListener(OnLeft);
        rightButton.onClick.AddListener(OnRight);
        selectButton.onClick.AddListener(OnSelect);
    }


    private void OnSfxToggleChanged(bool isOn)
    {
        // 볼륨, 뮤트 처리
        AudioManager.Instance.SetSfxVolume(isOn ? 1f : 0f);
        AudioManager.Instance.SetSfxMute(!isOn);
        // 아이콘 스프라이트 교체
        sfxIcon.sprite = isOn ? sfxOnSprite : sfxOffSprite;

        //  Toggle이 꺼지며 graphic.enabled=false로 바꾸면 다시 켜 준다
        if (sfxToggle.graphic != null)
            sfxToggle.graphic.enabled = true;
    }

    private void OnBgmToggleChanged(bool isOn)
    {
        AudioManager.Instance.SetBgmVolume(isOn ? 1f : 0f);
        AudioManager.Instance.SetBgmMute(!isOn);

        // 스프라이트 교체
        bgmIcon.sprite = isOn ? bgmOnSprite : bgmOffSprite;

        // Toggle이 꺼지며 graphic.enabled=false로 바꾸면 다시 켜 준다
        if (bgmToggle.graphic != null)
            bgmToggle.graphic.enabled = true;
    }

    // 조이스틱 관련 함수
    void UpdateUI()
    {
        joystickNameText.text = names[current];
    }

    void OnLeft()
    {
        current = (current - 1 + joysticks.Length) % joysticks.Length;
        UpdateUI();
    }

    void OnRight()
    {
        current = (current + 1) % joysticks.Length;
        UpdateUI();
    }

    void OnSelect()
    {
        saved = current;
        PlayerPrefs.SetInt("SelectedJoystick", saved);
        PlayerPrefs.Save();

        ApplySelection();
    }


    void ApplySelection()
    {
        for (int i = 0; i < joysticks.Length; i++)
        {
            if (joysticks[i] != null)
                joysticks[i].gameObject.SetActive(i == saved);
        }
        // PlayerController 쪽에도 알림
        GameEvents.RaiseJoystickChanged();
    }
}
