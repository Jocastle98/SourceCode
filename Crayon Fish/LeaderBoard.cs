using System.Runtime.InteropServices;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{

    public void OnClickOpenLeaderboardButton()
    {
        TossBridge.OpenLeaderBoard();
    }
}