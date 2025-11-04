using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundCon : MonoBehaviour
{
    public GameObject settingsMenuPrefab;
    private GameObject settingsMenuInstance;

    public void OpenSettings()
    {
       
        if (settingsMenuInstance == null)
        {
            settingsMenuInstance = Instantiate(settingsMenuPrefab);
            settingsMenuInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);
            settingsMenuInstance.SetActive(true);

            // OptionPanel 내부의 컴포넌트 찾기
            Transform optionPanelTransform = settingsMenuInstance.transform.Find("OptionPannel");         

            Slider backgroundSlider = optionPanelTransform.Find("BGM Slider")?.GetComponent<Slider>();
            Slider effectsSlider = optionPanelTransform.Find("Effect Slider")?.GetComponent<Slider>();
            Button backButton = optionPanelTransform.Find("BackBtn")?.GetComponent<Button>();         

            // 슬라이더 값 설정 및 이벤트 리스너 추가
            if (backgroundSlider != null && effectsSlider != null)
            {
                backgroundSlider.value = PlayerPrefs.GetFloat("BackgroundVolume", 1.0f);
                effectsSlider.value = PlayerPrefs.GetFloat("SoundEffectsVolume", 1.0f);

                backgroundSlider.onValueChanged.AddListener(SetBackgroundVolume);
                effectsSlider.onValueChanged.AddListener(SetEffectsVolume);
            }

            // 뒤로가기 버튼 이벤트 리스너 추가
            if (backButton != null)
            {
                backButton.onClick.AddListener(CloseSettings);
            }
            // 게임 일시 정지
            Time.timeScale = 0f;
        }
        else
        {
            settingsMenuInstance.SetActive(true);
        }
    }

    private void CloseSettings()
    {
        if (settingsMenuInstance != null)
        {
            settingsMenuInstance.SetActive(false);
        }
        // 게임 재개
        Time.timeScale = 1f;
    }

    private void SetBackgroundVolume(float volume)
    {
        AudioManager.instance.SetBackgroundVolume(volume);
      
    }

    private void SetEffectsVolume(float volume)
    {
        AudioManager.instance.SetSoundEffectsVolume(volume);

    }
}
