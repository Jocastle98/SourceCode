using UnityEngine;

public class ExitPanelController : MonoBehaviour
{

    public void CloseExitPanel()
    {
        UIManager.Instance.Close(gameObject, () =>
        {
            Time.timeScale = 1;
        });
    }

    public void ExitGameButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
        Application.Quit();                       // Android는 정상 종료
#else
        Application.Quit();
#endif
    }
}