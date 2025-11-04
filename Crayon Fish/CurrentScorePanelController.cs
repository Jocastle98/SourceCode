using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Xml.Linq;
public class CurrentScorePanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI expText;


    private void Start()
    {
        // 첫 화면에 현재 점수 표시
        UpdateScore(GameManager.Instance.player.Score);
        UpdateLevel(GameManager.Instance.player.Level);
        UpdateExp(); // 경험치 초기화

        // 점수 변경 이벤트 
        GameManager.Instance.player.OnCurrentScore += OnScoreChanged;
        GameManager.Instance.player.OnLevelUp += OnLevelUp;
    }

    private void OnDestroy()
    {
        GameManager.Instance.player.OnCurrentScore -= OnScoreChanged;
        GameManager.Instance.player.OnLevelUp -= OnLevelUp;
    }

    private void OnScoreChanged(int newScore)
    {
        UpdateScore(newScore);
        UpdateExp(); // 점수 변할 때마다 경험치 바 갱신
    }

    private void OnLevelUp()
    {
        UpdateLevel(GameManager.Instance.player.Level);
        UpdateExp(); // 레벨업하면 경험치도 초기화되므로 갱신
    }

    // 이벤트에서 호출될 콜백
    private void UpdateScore(int newScore)
    {
        scoreText.text = newScore.ToString();
    }

    private void UpdateLevel(int level)
    {
        levelText.text = "Lv." + level.ToString();
    }

    /// <summary>
    /// 경험치 슬라이더와 텍스트를 업데이트합니다.
    /// 최대 레벨일 경우 “Max” 표시하고 바를 비워 둡니다.
    /// </summary>
    private void UpdateExp()
    {
        var player = GameManager.Instance.player;

        // 최대 레벨 도달 시 처리
        if (player.Level >= Player.MaxLevel)
        {
            expText.text = "Max";
            expSlider.value = 1f;       // 바를 비워두고
            expSlider.interactable = false;    // 비활성화 (선택 사항)
            return;
        }

        // 그 외 일반 경험치 표시
        expSlider.interactable = true;
        int current = player.CurrentExp;
        int max = player.MaxExp;
        expText.text = $"{current} / {max}";
        expSlider.value = player.ExpProgress;
    }
}