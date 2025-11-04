using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserDataStructs; 

public class ReplayPanelController : PopupPanelController
{
    [SerializeField] private BoardCellController boardCellController;

    [SerializeField] private Image blackPlayerProfileImage;
    [SerializeField] private TextMeshProUGUI blackPlayerProfileText;  
    [SerializeField] private CanvasGroup blackTurnPanel;    

    [SerializeField] private Image whitePlayerProfileImage;
    [SerializeField] private TextMeshProUGUI whitePlayerProfileText;
    [SerializeField] private CanvasGroup whiteTurnPanel;

    private List<(int y, int x, Enums.EPlayerType stone)> _moves;
    private int _currentIndex = -1;

    private UserInfoResult blackUserInfo;
    private UserInfoResult whiteUserInfo;

    public void OpenReplayPanel(
        List<(int y, int x, Enums.EPlayerType stone)> moves,
        UserInfoResult blackInfo = default,
        UserInfoResult whiteInfo = default)
    {
        // moves가 null이면 빈리스트로 초기화
        _moves = moves ?? new List<(int, int, Enums.EPlayerType)>();
        _currentIndex = -1;

        blackUserInfo = blackInfo;
        whiteUserInfo = whiteInfo;

        boardCellController.InitBoard();

        SetPlayerProfiles();

        HighlightCurrentPlayer(Enums.EPlayerType.None);
    }

    private bool IsValidUserInfo(UserInfoResult info)
    {
        return !string.IsNullOrEmpty(info.username);
    }
    /// <summary>
    /// 흑/백 플레이어의 프로필 이미지를 세팅하고, 급수 ,닉네임 표시
    /// </summary>
    private void SetPlayerProfiles()
    {
        if (IsValidUserInfo(blackUserInfo))
        {
            blackPlayerProfileImage.sprite = GameManager.Instance.GetProfileSprite(blackUserInfo.profileimageindex);
            blackPlayerProfileText.text = $"{blackUserInfo.rank}급 {blackUserInfo.nickname}";
        }
        else
        {
            blackPlayerProfileImage.sprite = GameManager.Instance.GetProfileSprite(0);
            blackPlayerProfileText.text = "흑(Black)";
        }

        if (IsValidUserInfo(whiteUserInfo))
        {
            whitePlayerProfileImage.sprite = GameManager.Instance.GetProfileSprite(whiteUserInfo.profileimageindex);
            whitePlayerProfileText.text = $"{whiteUserInfo.rank}급 {whiteUserInfo.nickname}";
        }
        else
        {
            whitePlayerProfileImage.sprite = GameManager.Instance.GetProfileSprite(1);
            whitePlayerProfileText.text = "백(White)";
        }
    }


    /// <summary>
    /// 보드 전체 초기화하고 current까지 다시 두기 
    /// </summary>
    private void ReloadBoard()
    {
        boardCellController.InitBoard();

        for (int i = 0; i <= _currentIndex; i++)
        {
            var move = _moves[i];
            boardCellController.cells[move.y, move.x].SetMark(move.stone);
            boardCellController.cells[move.y, move.x].playerType = move.stone;
        }

        // 하이라이트
        if (_currentIndex >= 0)
        {
            HighlightCurrentPlayer(_moves[_currentIndex].stone);
        }
        else
        {
            HighlightCurrentPlayer(Enums.EPlayerType.None);
        }
    }

    /// <summary>
    /// 다음 수로 이동
    /// </summary>
    public void OnNextButtonClick()
    {
        // _moves가 null이거나 비어있으면 바로 confirm 창을 띄웁니다.
        if (_moves == null || _moves.Count == 0)
        {
            GameManager.Instance.OpenConfirmPanel("기록이 없습니다. \n시작화면으로 돌아가겠습니까?", () =>
            {
                GameManager.Instance.ChangeToMainScene();
            }, true);
            return;
        }

        if (_currentIndex + 1 < _moves.Count)
        {
            _currentIndex++;
            ReloadBoard();
        }
        else
        {
            GameManager.Instance.OpenConfirmPanel("마지막 수입니다. \n시작화면으로 돌아가겠습니까?", () =>
            {
                GameManager.Instance.ChangeToMainScene();
            }, true);
        }
    }

    /// <summary>
    /// 이전 수로 이동
    /// </summary>
    public void OnPrevButtonClick()
    {
        if (_currentIndex >= 0)
        {
            _currentIndex--;
            ReloadBoard();
        }
    }


    /// <summary>
    /// 흑/백 중 어느 쪽이 현재 수를 뒀는지 UI로 강조
    /// </summary>
    private void HighlightCurrentPlayer(Enums.EPlayerType currentStone)
    {
        float activeAlpha = 1f;
        float inactiveAlpha = 0.3f;

        if (currentStone == Enums.EPlayerType.Player_Black)
        {
            blackTurnPanel.alpha = activeAlpha;
            whiteTurnPanel.alpha = inactiveAlpha;
        }
        else if (currentStone == Enums.EPlayerType.Player_White)
        {
            blackTurnPanel.alpha = inactiveAlpha;
            whiteTurnPanel.alpha = activeAlpha;
        }
        else
        {
            blackTurnPanel.alpha = inactiveAlpha;
            whiteTurnPanel.alpha = inactiveAlpha;
        }
    }

    public void OnCloseButtonClick()
    {
        Hide();
    }
}
