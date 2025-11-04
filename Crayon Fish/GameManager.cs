using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : Singleton<GameManager>
{
    public Player player;
    public PlayerController playerController;


    [Header("패널")]
    [SerializeField] private GameObject settingsPanel;      // 세팅 패널
    private GameObject settingPanelIns;                     // 세팅패널 생성 변수
    [SerializeField] private GameObject scorePanel;    // 게임 끝날 때 패널
    private GameObject scorePanelIns;
    [SerializeField] private GameObject exitPanel; // 게임 종료 패널
    private GameObject exitPanelIns;
    [SerializeField] private GameObject rankingPanel;
    private GameObject rankingPanelIns;

    protected override void Awake()
    {
        base.Awake(); // Singleton<T>의 Awake 호출

        // player 데이터 초기화
        player = new Player(0, 1, 4);
        // player OnDie 이벤트 구독!
        player.OnDie += ScorePanelControl;
    }
    /// <summary>
    /// 씬 로드 시 재시작 하기 쉽게 새로운 플레이어로 할당
    /// </summary>
    void OnEnable()
    {
        SceneManager.sceneLoaded += (s, m) =>
        {
            playerController = FindObjectOfType<PlayerController>();
        };
    }
    /// <summary>
    /// 게임 상태를 초기화
    /// </summary>
    public void ResetState()
    {
        // 이전에 띄워뒀던 패널 날리기
        if (scorePanelIns != null)
        {
            Destroy(scorePanelIns);
            scorePanelIns = null;
        }

        if (rankingPanelIns != null)
        {
            Destroy(rankingPanelIns);
            rankingPanelIns = null;
        }
        // 이벤트 재구독
        player.OnDie -= ScorePanelControl;
        player = new Player(0, 1, 4);
        player.OnDie += ScorePanelControl;
    }

    /// <summary>
    /// 플레이어가 죽었을 때 호출
    /// </summary>
    private void ScorePanelControl(int finalScore)
    {
        player.OnDie -= ScorePanelControl;

        // 튜토리얼중엔 건너뛰기
        if (TutorialManager.IsPlaying)              
        {
            GameEvents.RaisePlayerHit();     // 튜토리얼 진행용
            player.OnDie += ScorePanelControl; // 다음 죽음에 대비해 재구독
            return;                           // 여기서 종료 → 패널 X
        }
        if (scorePanelIns == null) scorePanelIns = Instantiate(scorePanel, GetUiRoot());
        // 스크립트에 접근 
        var ctrl = scorePanelIns.GetComponent<ScorePanelController>();
        ctrl.Init(finalScore);
    }
    

    /// <summary>
    /// 환경에 맞는 Canvas 찾기 게임 씬에선 Joystick_canvas
    /// </summary>
    private Transform GetUiRoot()
    {
        // 기본 Canvas
        var canva = GameObject.Find("UI_Canvas");
        return canva ? canva.transform : null;
    }


    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
       playerController = FindObjectOfType<PlayerController>();

        // PlayerPrefs에서 선택된 조이스틱 인덱스 불러오기
        int savedIndex = PlayerPrefs.GetInt("SelectedJoystick", 1);

        // 씬에 배치된 Joystick들(배열 순서: Dynamic, Floating) 가져오기
        var dyn = FindObjectOfType<DynamicJoystick>(true); // true: 비활성화된 오브젝트도 찾기
        var flo = FindObjectOfType<FloatingJoystick>(true); // true: 비활성화된 오브젝트도 찾기
        Joystick[] joys = new Joystick[] { dyn, flo };

        // 1. 모든 조이스틱을 순회하며 선택된 조이스틱만 활성화
        for (int i = 0; i < joys.Length; i++)
        {
            if (joys[i] != null)
            {
                joys[i].gameObject.SetActive(i == savedIndex);
            }
        }

        // 2. 이제 활성화된 조이스틱을 PlayerController에 할당
        if (playerController != null && savedIndex >= 0 && savedIndex < joys.Length && joys[savedIndex] != null)
        {
            playerController.joystick = joys[savedIndex];
        }
        else
        {
            Debug.LogWarning($"SavedJoystick({savedIndex}) is invalid or PlayerController is null.");
        }
    }
}