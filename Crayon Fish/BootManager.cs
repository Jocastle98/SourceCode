using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BootManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider progressBar;     
    [SerializeField] private TMP_Text   progressText;     

    [Header("Settings")]
    [SerializeField] private float  displaySpeed = 1f;    // 게이지 시간 조절
    [SerializeField] private float  minShowTime  = 4.0f;    // 로딩 화면 최소 표시 시간

    private IEnumerator Start()
    {
        
        bool tutorialDone = PlayerPrefs.GetInt(Constants.TutorialDone, 0) == 1;
        // bool tutorialDone =  true;
        string nextScene  = tutorialDone ? "StartScene" : "Tutorial";
        
        float shownTime = 0f;          // 경과 시간
        float visual    = 0f;          // 화면에 보이는 값

        //씬 비동기 로드 시작
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        //로딩 루프
        while (visual < 1f)            
        {
            shownTime += Time.unscaledDeltaTime;

            // 실제 진행도(0~0.9)는 op.progress, 마지막 활성화는 0.9~1.0 구간
            float target = (op.progress < 0.9f)
                ? op.progress              
                : 0.9f + 0.1f * (shownTime / minShowTime); 

            // 화면에 보이는 값을 천천히 따라가도록 제한
            visual = Mathf.MoveTowards(visual, target, displaySpeed * Time.unscaledDeltaTime);

            // UI 반영
            progressBar.value  = visual;
            progressText.text  = $"{Mathf.RoundToInt(visual * 100f)}%";

            // 실제 로딩이 끝났고(0.9) 최소 표시 시간도 지난 경우 전환 허용
            if (op.progress >= 0.9f && shownTime >= minShowTime)
                op.allowSceneActivation = true;

            yield return null;
        }
    }
}