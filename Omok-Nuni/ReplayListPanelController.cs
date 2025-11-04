using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UserDataStructs;

public class ReplayListPanelController : PopupPanelController
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject historyButtonPrefab;
    [SerializeField] private GameObject replayPanel;

    private async void OnEnable()
    {
        await RefreshList();
    }

    private async Task RefreshList()
    {
        //  서버에서 기보 목록 가져오기
        var records = await NetworkManager.Instance.GetOmokRecords(
            successCallback: () => { Debug.Log("기보 목록 가져오기 성공"); },
            failureCallback: () => { Debug.LogWarning("기보 목록 가져오기 실패"); }
        );

        // 만약 기보가 15개보다 많다면 최신 15개만 사용 
        if(records.Count > 15)
        {
            records = records.OrderByDescending(r => r.createdAt).Take(15).ToList();
        }
        foreach (Transform child in content) 
        {
            Destroy(child.gameObject);
        }

        foreach (var record in records)
        {
            var buttonObj = Instantiate(historyButtonPrefab, content);
            var textTMP = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textTMP != null)
            {
                textTMP.text = $"기보: {record.recordId}";
            }

            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                OpenReplay(record);
            });
        }
    }

    private async void OpenReplay(OmokRecord record)
    {
        var moveList = new List<(int, int, Enums.EPlayerType)>();
        foreach (var m in record.moves)
        {
            moveList.Add((m.y, m.x, (Enums.EPlayerType)m.stone));
        }
        UserInfoResult blackInfo = default;
        UserInfoResult whiteInfo = default;

        if (!string.IsNullOrEmpty(record.blackUserId))
        {
            blackInfo = await NetworkManager.Instance.GetUserInfoByUserId(record.blackUserId);
        }
        if (!string.IsNullOrEmpty(record.whiteUserId))
        {
            whiteInfo = await NetworkManager.Instance.GetUserInfoByUserId(record.whiteUserId);
        }

        var replayPanelObj = Instantiate(replayPanel, transform.parent);
        var replayCtrl = replayPanelObj.GetComponent<ReplayPanelController>();
        if (replayCtrl != null)
        {
            replayCtrl.OpenReplayPanel(moveList, blackInfo, whiteInfo);
        }

        gameObject.SetActive(false);
    }

    public void OnCloseButtonClick()
    {
        Hide();
    }
    
    public override void Hide(Action OnPanelControllerHide = null)
    {
        FindObjectOfType<MainButtonAnimation>().ShowAllStone();

        base.Hide(OnPanelControllerHide);
    }
}