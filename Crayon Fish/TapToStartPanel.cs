using UnityEngine;
using UnityEngine.SceneManagement;

public class TapToStartPanel : MonoBehaviour
{
    public void OnClickStartToTap()
    {
        SceneManager.LoadScene("PlayScene");
    }
}
