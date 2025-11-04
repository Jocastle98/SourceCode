using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMgr : MonoBehaviour
{
    public void OnClickStartBtn()
    {
        // 이 함수가호출되면 유니티 UI 화면에서 Start 버튼을 누른 상태임
        // 그렇기 때문에 여기서 씬 화면은 scPlay 화면으로 전환해야 한다!
        Debug.Log("스타트 버튼을 클릭!");

        SceneManager.LoadScene("scPlay");
    }
}
