using System;

public static class GameEvents
{
    public static event Action FishEaten;   // 물고기를 먹으면 호출
    public static event Action ItemPicked;  // 아이템(버프 등)을 먹으면 호출
    public static event Action PlayerHit; // 무적상태에서 타격 당하면 
    public static event Action SpeedPotionPicked; // 스피드 포션 먹으면
    public static event System.Action OnTutorialBubbleEaten; // 방울 먹으면 
    public static void RaiseFishEaten() => FishEaten?.Invoke();
    public static void RaiseItemPicked() => ItemPicked?.Invoke();
    public static void RaisePlayerHit()  => PlayerHit?.Invoke();
    public static void RaiseSpeedPotionPicked() => SpeedPotionPicked?.Invoke();
    public static void TutorialBubbleEaten() => OnTutorialBubbleEaten?.Invoke();

    public static event Action JoystickChanged;
    public static void RaiseJoystickChanged() => JoystickChanged?.Invoke();
    public static event Action OnTutorialPopupClosed;
    public static void RaiseTutorialPopupClosed() => OnTutorialPopupClosed?.Invoke();
}