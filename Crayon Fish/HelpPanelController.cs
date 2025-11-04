using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanelController : MonoBehaviour
{
    [Header("패널 목록")]
    [SerializeField] private List<GameObject> helpPanels;

    [Header("버튼")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [Header("Title")]
    [SerializeField] private TMP_Text title;
    private int currentIndex = 0;

    void Start()
    {
        // 버튼 리스너 등록
        prevButton.onClick.AddListener(OnClickPrev);
        nextButton.onClick.AddListener(OnClickNext);

        ShowPanel(currentIndex);
    }

    private void OnClickPrev()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowPanel(currentIndex);
        }
    }

    private void OnClickNext()
    {
        if (currentIndex < helpPanels.Count - 1)
        {
            currentIndex++;
            ShowPanel(currentIndex);
        }
    }

    private void ShowPanel(int index)
    {
        for (int i = 0; i < helpPanels.Count; i++)
        {
            helpPanels[i].SetActive(i == index);
        }

        // 버튼 상태 업데이트 (첫/마지막 페이지일 경우 비활성화)
        prevButton.interactable = (index > 0);
        nextButton.interactable = (index < helpPanels.Count - 1);
        if (index == 0)
        {
            title.text = "Fish";
        }
        else
        {
            title.text = "방해 요소";
        }
    }
    
    
}