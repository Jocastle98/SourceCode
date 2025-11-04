using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;


public class ScorePanelController : PopupPanelController
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI upgradeText;
    [SerializeField] private TextMeshProUGUI leftScoreText;
    [SerializeField] private TextMeshProUGUI rightScoreText;
    [SerializeField] private Button ResignButton;
    [SerializeField] private Image rankUpGaugeAdd;    // 0 이상
    [SerializeField] private Image rankUpGaugeDelete; // 0 이하

    public void InitializePanel(int currentScore, bool isWin, int addDelete, int rank, int rankuppoints)
    {
        Show();

        ResignButton.gameObject.SetActive(GameManager.Instance.bIsMultiplay);
        GameManager.Instance.OnCloseScorePanel += this.Hide;
        
        // 승/패 메시지
        if (isWin)
            messageText.text = $"오목에서 승리했습니다.\n {addDelete*10}점을 획득!";
        else
            messageText.text = $"오목에서 패배했습니다.\n {Mathf.Abs(addDelete)*10}점을 잃었습니다.";

        int minScore, maxScore, threshold;
        if (rank >= 10) 
        {
            minScore = -3; 
            maxScore = 3;  
            threshold = 3;
            leftScoreText.text = "-30";
            rightScoreText.text = "30";
        }
        else if (rank >= 5) // 9~5급
        {
            minScore = -5; 
            maxScore = 5;
            threshold = 5;   
            leftScoreText.text = "-50";
            rightScoreText.text = "50";
        }
        else // 4~1급
        {
            minScore = -10; 
            maxScore = 10;
            threshold = 10;  
            leftScoreText.text = "-100";
            rightScoreText.text = "100";
        }

        // 18급 예외 처리 (rankuppoints가 -3보다 작아지지 않도록)
        if (rank == 18 && rankuppoints < -3)
        {
            rankuppoints = -3;
        }
        rankuppoints = Mathf.Clamp(rankuppoints, minScore, maxScore);

        rankUpGaugeAdd.fillAmount    = 0f;
        rankUpGaugeDelete.fillAmount = 0f;

        float fillTargetAdd = 0f;
        float fillTargetDelete = 0f;

        if (rankuppoints > 0)
        {
            fillTargetAdd = (float) rankuppoints / threshold;
        }
        else if (rankuppoints < 0)
        {
            fillTargetDelete = (float) Mathf.Abs(rankuppoints) / threshold;
        }

        
        if (fillTargetAdd > 0f)
        {
            rankUpGaugeAdd.DOFillAmount(fillTargetAdd, 1f)
                .SetEase(Ease.Linear); 
        }
        if (fillTargetDelete > 0f)
        {
            rankUpGaugeDelete.DOFillAmount(fillTargetDelete, 1f)
                .SetEase(Ease.Linear);
        }

        if (rank > 1 && rankuppoints >= threshold)
        {
            upgradeText.text = $"승급합니다!\n\n현재 등급: {rank}급.";

            // 승급 시 게이지가 가득 차도록
            rankUpGaugeAdd.DOFillAmount(1f, 1f).SetEase(Ease.Linear);
            return; 
        }
        else if (rank < 18 && rankuppoints <= -threshold)
        {
            upgradeText.text = "강등합니다!";

            // 강등 시 delete 게이지를 가득
            rankUpGaugeDelete.DOFillAmount(1f, 1f).SetEase(Ease.Linear);
            return; 
        }
        else
        {
            int remain;
            if (rankuppoints >= 0)
            {
                remain = threshold - rankuppoints;
                upgradeText.text = $"승급까지 {remain}번 남음\n현재 등급: {rank}급.";
            }
            else
            {
                remain = threshold - Mathf.Abs(rankuppoints);
                upgradeText.text = $"강등까지 {remain}번 남음\n현재 등급: {rank}급.";
            }
        }
    }

    public void OnClickCloseButton()
    {
        Hide(() =>
        {
            GameManager.Instance.ChangeToMainScene();
            Debug.Log("메인씬으로 전환");
        });
    }

    public void OnClickRematchButton()
    {
        Hide(() =>
        {
            UniTask.Void(async () =>
            {
                GameManager.Instance.OnRematchGame?.Invoke();
                
                await NetworkManager.Instance.ConsumeCoin(Constants.ConsumeCoin, 
                    successCallback: (remainingCoins) => 
                    {
                        GameManager.Instance.OpenConfirmPanel($"남은 코인은 {remainingCoins} 입니다.",
                            () =>
                            {
                                if (!GameManager.Instance.bIsStartGame)
                                {
                                    GameManager.Instance.OpenWaitingPanel();
                                }
                            }, false);
                    },
                    failureCallback: () =>
                    {
                        GameManager.Instance.OpenConfirmPanel("코인이 부족합니다.", () =>
                        {
                            GameManager.Instance.ChangeToMainScene();
                        }, false);
                    });
            });
        });
    }

    private void Hide()
    {
        base.Hide();
    }
}
