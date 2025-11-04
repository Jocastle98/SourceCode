using TMPro;
using UnityEngine;
using Ricimi;

public class RankPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text[] rankScoreTexts;
    private float prevTimeScale;
    
    /// <summary>
    /// 팝업 열릴 때
    /// </summary>
    private void OnEnable()
    {
        prevTimeScale = Time.timeScale;  // 0이든 1이든 그대로 기억
        Time.timeScale = 0f;              // 랭킹 보는 동안 게임 일시정지
        RefreshRanking();
    }
    /// <summary>
    /// 점수 업데이트하기
    /// </summary>
    public void RefreshRanking()
    {
        for (int i = 0; i < 3; i++)
        {
            int score = PlayerPrefs.GetInt($"HighScore{i}", 0);
            if (rankScoreTexts != null && i < rankScoreTexts.Length)
            {
                rankScoreTexts[i].text = $"{score}";
            }
        }
    }
    
    public void RankPanelClose()
    {
        var popup = GetComponent<Popup>();       
        if (popup != null)
            popup.Close(resumeGameTime: false);                       
    
        Time.timeScale = prevTimeScale;         // Score 화면이면 0, 그 외엔 1
    }
}