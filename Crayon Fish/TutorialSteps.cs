using UnityEngine;

[System.Serializable]
public struct TutorialStep
{
    public GameObject panel;        // 보여 줄 UI 패널(없으면 null)
    public StepCondition condition; // TapToClose, EatFish, PickItem …
    public GameObject toActivate;   // 씬에 미리 비활성화해 둔 월드 오브젝트
    [Tooltip("패널이 뜬 뒤 최소 유지 시간(초)")]
    public float minDisplay;
    
}

public enum StepCondition
{
    TapToClose =0,
    EatFish =1,
    PickItem =2,
    DragToClose =3,
    HitByMonster =4,
    SpeedPotion  = 5,
    PressButton  = 6,
    ClosePopup     = 7 
}
