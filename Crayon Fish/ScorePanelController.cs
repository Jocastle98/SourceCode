using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ScorePanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    //[SerializeField] CutsceneAnimator cutscenePrefab;   // 파란 큐브
    //CutsceneAnimator cutsceneIns;   
    [SerializeField] private CutSceneRotation cutSceneRotation;
    CutSceneRotation cutSceneRotationIns;
    
    void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();

        // 프리팹 Instantiate
        cutSceneRotationIns = Instantiate(cutSceneRotation, canvas.transform);
        cutSceneRotationIns.gameObject.SetActive(false);
    }

    public void Init(int finalScore)
    {
        scoreText.alpha = 0;
        UIManager.Instance.Open(
            gameObject,
            () => {
                scoreText.text = $"{finalScore}";
                scoreText.DOFade(1f, 0.25f)          
                    .SetUpdate(true);
                // 패널 애니메이션 끝난 뒤에야 멈추기
                Time.timeScale = 0f;
            }
        );
    }

    public void RestartGame()
    {
        UIManager.Instance.Close(
            gameObject,
            () => {
                cutSceneRotationIns.Play(() =>
                {
                    Time.timeScale = 1f;
                    GameManager.Instance.ResetState();
                    SceneManager.LoadScene(
                        SceneManager.GetActiveScene().name);

                });
            }
        );
    }
    public void ShareMyScore()
    {
        int score = int.Parse(scoreText.text);
        string shareText = $"🎮 크레용 피시 {score}점! 너도 해봐";

    #if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("ShareScoreFromUnity", shareText);
    #else
        // 에디터나 PC 환경에서는 복사만 진행 (디버깅용)
        GUIUtility.systemCopyBuffer = shareText;
        PlayerStateTextManager.Instance.Show("📋 자랑 문구가 복사됐어요!", transform.position + Vector3.up * 1.5f);
    #endif
    }
}