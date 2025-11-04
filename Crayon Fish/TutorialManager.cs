using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("튜토리얼 단계 설정")]
    [SerializeField] TutorialStep[] steps;
    [SerializeField] bool   pauseGameDuringPanel = true;
    [SerializeField] string nextScene = "StartScene";
    [SerializeField] private CutSceneRotation cutSceneRotation;

    // [Header("UI 설정")] [SerializeField] private RectTransform safeAreaPanel;
    
    private CutSceneRotation cutSceneRotationIns;
    // ───────── 상태값
    float stepShownTime;
    int   cur;
    bool  waiting;          // 현재 스텝 조건 충족 대기 중
    bool  pendingClose;     // minDisplay 끝나면 자동 종료 예약
    public static bool IsPlaying { get; private set; }   // 외부에서 참조

    // ───────── 이벤트 버퍼 ★
    bool fishEatenBuffered;
    bool itemPickedBuffered;
    bool playerHitBuffered;
    bool speedPotionBuffered;
    bool buttonPressedBuffered;

    // ──────────────────────────────────────────── Awake / Enable / Disable

    void Awake()
    {
        IsPlaying = true;

        // 버퍼 초기화 ★
        buttonPressedBuffered = false; 
        fishEatenBuffered = itemPickedBuffered = playerHitBuffered = speedPotionBuffered = false;

        if (steps == null || steps.Length == 0)
        {
            Debug.LogError("[Tutorial] steps 비어 있음");
            enabled = false;
            return;
        }

        // 모든 패널 끄고 첫 번째만 켜기
        for (int i = 0; i < steps.Length; i++)
            if (steps[i].panel) steps[i].panel.SetActive(i == 0);

        // 첫 스텝 월드 오브젝트 활성
        ActivateWorldObjects(steps[0]);

        waiting       = true;
        pendingClose  = false;
        cur           = 0;
        stepShownTime = Time.unscaledTime;

        if (pauseGameDuringPanel && steps[0].condition == StepCondition.TapToClose)
            Time.timeScale = 0f;
        var canvas = GameObject.Find("UI_Canvas")?.transform;
        if (canvas != null && cutSceneRotation != null)
        {
            cutSceneRotationIns = Instantiate(cutSceneRotation, canvas);
            cutSceneRotationIns.gameObject.SetActive(false);
        }
        
        /*
        // 씬이 로드될 때, DontDestoryOnLoad로 살아남은 SafeAreaManager를 찾아 이 씬의 UI 패널을 전달하고 SafeArea를 적용해달라 요청
        if (SafeAreaManager.instance != null)
        {
            SafeAreaManager.instance.UpdateSafeAreaOnNewScene(safeAreaPanel);
        }
        */
    }

    void OnEnable()
    {
        GameEvents.FishEaten          += OnFishEaten;
        GameEvents.ItemPicked         += OnItemPicked;
        GameEvents.PlayerHit          += OnPlayerHit;
        GameEvents.SpeedPotionPicked  += OnSpeedPotionPicked;
        GameEvents.OnTutorialPopupClosed += OnPopupClosed;
    }

    void OnDisable()
    {
        GameEvents.FishEaten          -= OnFishEaten;
        GameEvents.ItemPicked         -= OnItemPicked;
        GameEvents.PlayerHit          -= OnPlayerHit;
        GameEvents.SpeedPotionPicked  -= OnSpeedPotionPicked;
        GameEvents.OnTutorialPopupClosed -= OnPopupClosed;
        Time.timeScale = 1f;
    }

    // ──────────────────────────────────────────── Update (Tap / Drag)

    void Update()
    {
        if (!waiting) return;

        // 최소 표시 시간
        if (Time.unscaledTime - stepShownTime < steps[cur].minDisplay)
            return;

        // ── Drag 감지
        bool dragged = false;
        if (Input.touchCount > 0)
            dragged = Input.GetTouch(0).phase == TouchPhase.Moved;
        else
            dragged = Input.GetMouseButton(0) &&
                      (Mathf.Abs(Input.GetAxisRaw("Mouse X")) > 0.005f ||
                       Mathf.Abs(Input.GetAxisRaw("Mouse Y")) > 0.005f);

        var cond = steps[cur].condition;

        if ((cond == StepCondition.TapToClose  && Input.GetMouseButtonDown(0)) ||
            (cond == StepCondition.DragToClose && dragged))
        {
            TryCloseStep();   // minDisplay 검사 포함
        }
    }

    // ──────────────────────────────────────────── 스텝 제어

    void CloseStep()
    {
        if (steps[cur].panel) steps[cur].panel.SetActive(false);
        cur++;

        if (cur >= steps.Length) { EndTutorial(); return; }
        OpenStep(cur);
    }

    void OpenStep(int idx)
    {
        var step = steps[idx];

        if (step.panel) step.panel.SetActive(true);
        ActivateWorldObjects(step);

        bool pause = step.condition == StepCondition.TapToClose;
        Time.timeScale = (pauseGameDuringPanel && pause) ? 0f : 1f;

        stepShownTime  = Time.unscaledTime;
        waiting        = true;
        pendingClose   = false;

        ApplyBufferedEventIfAny();   // ★ 새 스텝 열리자마자 버퍼 검사
    }

    void ActivateWorldObjects(TutorialStep step)
    {
        if (step.toActivate) step.toActivate.SetActive(true);
    }

    // ──────────────────────────────────────────── 이벤트 콜백 + 버퍼 ★

    void OnFishEaten()
    {
        if (waiting && steps[cur].condition == StepCondition.EatFish)
            TryCloseStep();
        else
            fishEatenBuffered = true;          // ★ 버퍼
    }

    void OnItemPicked()
    {
        if (waiting && steps[cur].condition == StepCondition.PickItem)
            TryCloseStep();
        else
            itemPickedBuffered = true;         // ★ 버퍼
    }

    void OnPlayerHit()
    {
        if (waiting && steps[cur].condition == StepCondition.HitByMonster)
            TryCloseStep();
        else
            playerHitBuffered = true;          // ★ 버퍼
    }

    void OnSpeedPotionPicked()
    {
        if (waiting && steps[cur].condition == StepCondition.SpeedPotion)
            StartCoroutine(DelayCloseStep(3f));
        else
            speedPotionBuffered = true;        // ★ 버퍼
    }

    // 버퍼 적용 함수 ★
    void ApplyBufferedEventIfAny()
    {
        switch (steps[cur].condition)
        {
            case StepCondition.EatFish:
                if (fishEatenBuffered)
                {
                    fishEatenBuffered = false;
                    TryCloseStep();
                }
                break;
            case StepCondition.PickItem:
                if (itemPickedBuffered)
                {
                    itemPickedBuffered = false;
                    TryCloseStep();
                }
                break;
            case StepCondition.HitByMonster:
                if (playerHitBuffered)
                {
                    playerHitBuffered = false;
                    TryCloseStep();
                }
                break;
            case StepCondition.SpeedPotion:
                if (speedPotionBuffered)
                {
                    speedPotionBuffered = false;
                    StartCoroutine(DelayCloseStep(3f));
                }
                break;
            case StepCondition.PressButton:
                if (buttonPressedBuffered)
                {
                    buttonPressedBuffered = false;
                    TryCloseStep();
                }
                break;
        }
    }

    // ──────────────────────────────────────────── 공통 로직

    void TryCloseStep()
    {
        float remain = steps[cur].minDisplay -
                       (Time.unscaledTime - stepShownTime);

        if (remain <= 0f)
        {
            waiting = false;
            CloseStep();
        }
        else if (!pendingClose)
        {
            pendingClose = true;
            StartCoroutine(DelayCloseStep(remain));
        }
    }

    IEnumerator DelayCloseStep(float delay)
    {
        waiting = false;
        yield return new WaitForSecondsRealtime(delay);
        CloseStep();
    }

    // ──────────────────────────────────────────── 튜토리얼 종료

    void EndTutorial()
    {
        IsPlaying = false;
        PlayerPrefs.SetInt(Constants.TutorialDone, 1);
        PlayerPrefs.Save();

        GameManager.Instance?.ResetState();
        Time.timeScale = 1f;
        if (cutSceneRotationIns != null)
        {
            cutSceneRotationIns.Play(() => {
                SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
            });
        }
        else
        {
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }
    }

    public void OnTutorialButtonPressed()
    {
        if (waiting && steps[cur].condition == StepCondition.PressButton)
            TryCloseStep();
        else
            buttonPressedBuffered = true;        // 다음 스텝용 버퍼
    }
    void OnPopupClosed()
    {
        if (waiting && steps[cur].condition == StepCondition.ClosePopup)
            TryCloseStep();
    }
    // ──────────────────────────────────────────── 기타

    public void StartPlayScene() => SceneManager.LoadScene("StartScene");
    void OnDestroy() => IsPlaying = false;
}
