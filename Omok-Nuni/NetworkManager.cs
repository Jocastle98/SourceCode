using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using UserDataStructs;

/// <summary>
/// 클라이언트에서 서버로 기보를 전송할 때(POST /users/addomokrecord) 요청(Request) 바디로 사용하는 구조체
/// </summary>
public class OmokRecordPayload
{
    public string recordId;
    public List<OmokMove> moves;
    public string blackUserId;  
    public string whiteUserId;
}
/// <summary>
/// 여러 기보를 한 번에 받아올 때 배열 래핑 용도
/// </summary>
[Serializable]
public class OmokRecordWrapper
{
    public OmokRecord[] records;
}

/// <summary>
/// 서버에서 받아오는 기보를 표현하는 응답용 클래스
/// </summary>
[Serializable]
public class OmokRecord
{
    public string blackUserId;
    public string whiteUserId;
    public string recordId;
    public List<OmokMove> moves;
    public string createdAt;
}
/// <summary>
/// 각 수(돌을 놓은 좌표 y,x 와 돌 색 stone)를 표현하는 기본 자료구조
/// </summary>
[Serializable]
public class OmokMove
{
    public int y;
    public int x;
    public int stone;
}


public class NetworkManager : Singleton<NetworkManager>
{
    // 로그아웃
    // 급수 조회
    // 코인 조회
    // 리더보드 조회

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    /// <summary>
    /// 회원가입
    /// </summary>
    /// <param name="signupData"></param>
    /// <param name="successCallback"></param>
    /// <param name="failureCallback"></param>
    public async UniTask Signup(SignupData signupData, Action successCallback, Action failureCallback)
    {
        string jsonStr = JsonUtility.ToJson(signupData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonStr);

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/signup", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await www.SendWebRequest().ToUniTask();
            }
            catch (Exception ex)
            {
                Debug.Log("Exception caught: " + ex.Message);

                if (www.responseCode == 409)
                {
                    GameManager.Instance.OpenConfirmPanel("이미 존재하는 사용자입니다.", () => { failureCallback?.Invoke(); },
                        false);
                }
                else
                {
                    GameManager.Instance.OpenConfirmPanel("서버와 통신 중 오류가 발생했습니다.", () => { failureCallback?.Invoke(); },
                        false);
                }

                return;
            }

            var result = www.downloadHandler.text;
            Debug.Log("Result: " + result);

            GameManager.Instance.OpenConfirmPanel("회원 가입이 완료 되었습니다.", () => { successCallback?.Invoke(); }, false);
        }
    }

    /// <summary>
    /// 로그인
    /// </summary>
    /// <param name="signinData"></param>
    /// <param name="successCallback"></param>
    /// <param name="failureCallback"></param>
    public async UniTask SigninWithSigninData(SigninData signinData, Action<string> successCallback,
        Action<int> failureCallback)
    {
        string jsonString = JsonUtility.ToJson(signinData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/signin", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await www.SendWebRequest().ToUniTask();
            }
            catch (Exception ex)
            {
                Debug.Log("Exception caught: " + ex.Message);

                return;
            }

            // 쿠키 저장
            var cookie = www.GetResponseHeader("set-cookie");
            if (!string.IsNullOrEmpty(cookie))
            {
                int lastIndex = cookie.LastIndexOf(";");
                if (lastIndex > 0)
                {
                    string sid = cookie.Substring(0, lastIndex);
                    PlayerPrefs.SetString("sid", sid);
                }
            }

            var resultString = www.downloadHandler.text;
            var result = JsonUtility.FromJson<SigninResult>(resultString);

            if (result.result == 0)
            {
                // 유저네임이 유효하지 않음
                GameManager.Instance.OpenConfirmPanel("유저네임이 유효하지 않습니다.",
                    () => { failureCallback?.Invoke(result.result); }, false);
            }
            else if (result.result == 1)
            {
                // 패스워드가 유효하지 않음
                GameManager.Instance.OpenConfirmPanel("패스워드가 유효하지 않습니다.",
                    () => { failureCallback?.Invoke(result.result); }, false);
            }
            else if (result.result == 2)
            {
                // 로그인 성공
                GameManager.Instance.OpenConfirmPanel("로그인에 성공하였습니다.",
                    () => { successCallback?.Invoke(result.nickname); }, false);
            }
        }
    }

    public async UniTask AutoSignin(Action successCallback, Action failureCallback)
    {
        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/autosignin", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
           
            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }
            
            try
            {
                await www.SendWebRequest().ToUniTask();
            }
            catch (Exception ex)
            {
                Debug.Log("Exception caught: " + ex.Message);
                if (www.responseCode == 403)
                {
                    GameManager.Instance.OpenConfirmPanel("로그인 기록이 없습니다.\n로그인이 필요합니다.", () =>
                    {
                        failureCallback?.Invoke();
                    });
                }
                return;
            }
            
            var result = www.downloadHandler.text;
            var userInfo = JsonUtility.FromJson<UserInfoResult>(result);
               
            GameManager.Instance.OpenConfirmPanel($"{userInfo.nickname}: \n자동 로그인 성공", () =>
            {
                successCallback?.Invoke();
            }, false);
        }
    }

    public async UniTask Signout(Action successCallback, Action failureCallback)
    {
        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/signout", UnityWebRequest.kHttpVerbPOST))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            
            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }
            
            try
            {
                await www.SendWebRequest().ToUniTask();
            }
            catch (Exception ex)
            {
                Debug.Log("Exception caught: " + ex.Message);
                GameManager.Instance.OpenConfirmPanel($"로그아웃에 실패하였습니다.: {www.error}.", () =>
                {
                    failureCallback?.Invoke();
                }, false);
                return;
            }
            
            PlayerPrefs.DeleteKey("sid");
            GameManager.Instance.OpenConfirmPanel("로그아웃 하였습니다.", () =>
            {
                successCallback?.Invoke();
            }, false);
        }
    }
    
    // 비동기 방식
    public async UniTask<UserInfoResult> GetUserInfo(Action successCallback, Action failureCallback)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (sid == null)
        {
            Debug.Log("유저 데이터 불러오기에 실패했습니다. \n" +
                      "세션 데이터가 없습니다.");
            failureCallback?.Invoke();
            return new UserInfoResult();
        }

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/userinfo", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);
            try
            {
                await www.SendWebRequest().ToUniTask();
            }
            catch (Exception ex)
            {
                Debug.Log("Exception caught: " + ex.Message);
                if (www.responseCode == 400)
                {
                    GameManager.Instance.OpenConfirmPanel("사용자 검증 실패", () => { failureCallback?.Invoke(); },
                        false);
                }
                failureCallback?.Invoke();
            }

            var resultStr = www.downloadHandler.text;
            UserInfoResult userInfo = JsonUtility.FromJson<UserInfoResult>(resultStr);
            
            successCallback?.Invoke();
            
            return userInfo;
        }
    }
    
    // 동기적 방식 GetUserInfo 버전
    public UserInfoResult GetUserInfoSync(Action successCallback, Action failureCallback)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (string.IsNullOrEmpty(sid))
        {
            Debug.Log("유저 데이터 불러오기에 실패했습니다. \n" +
                      "세션 데이터가 없습니다.");
            failureCallback?.Invoke();
            return new UserInfoResult();
        }

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/userinfo", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);

            // 동기적으로 요청 보내기
            www.SendWebRequest();

            // 요청이 완료될 때까지 대기
            while (!www.isDone) { }

            // 요청 결과 처리
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("유저 데이터 불러오기에 실패했습니다. \n" +
                          $"에러: {www.error}");
                if (www.responseCode == 400)
                {
                    GameManager.Instance.OpenConfirmPanel("사용자 검증 실패", () => { failureCallback?.Invoke(); },
                        false);
                }
                failureCallback?.Invoke();
                return new UserInfoResult();
            }

            var resultStr = www.downloadHandler.text;
            UserInfoResult userInfo = JsonUtility.FromJson<UserInfoResult>(resultStr);

            successCallback?.Invoke();
            return userInfo;
        }
    }
    
    public async UniTask ChangeProfileImage(ProfileImageData profileImageData, Action successCallback, Action failureCallback)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (sid == null)
        {
            Debug.Log("유저 데이터 불러오기에 실패했습니다. \n" +
                      "세션 데이터가 없습니다.");
            return;
        }

        string jsonString = JsonUtility.ToJson(profileImageData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
        
        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/changeprofileimage", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);
            www.SetRequestHeader("Content-Type", "application/json");
            
            try
            {
                await www.SendWebRequest().ToUniTask();
            }
            catch (Exception ex)
            {
                Debug.Log("Exception caught: " + ex.Message);
                if (www.responseCode == 400)
                {
                    GameManager.Instance.OpenConfirmPanel("사용자 검증 실패", () => { failureCallback?.Invoke(); },
                        false);
                }
            }
        }
    }
    
    public async UniTask<UsersRankInfo> GetUsersRank(Action successCallback, Action failureCallback)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (sid == null)
        {
            Debug.Log("유저 데이터 불러오기에 실패했습니다. \n" +
                      "세션 데이터가 없습니다.");
            failureCallback?.Invoke();
            return new UsersRankInfo();
        }

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/leaderboard", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);
            try
            {
                await www.SendWebRequest().ToUniTask();
            }
            catch (Exception ex)
            {
                Debug.Log("Exception caught: " + ex.Message);
                if (www.responseCode == 400)
                {
                    GameManager.Instance.OpenConfirmPanel("사용자 검증 실패", () => { failureCallback?.Invoke(); },
                        false);
                }
                failureCallback?.Invoke();
            }

            var resultStr = www.downloadHandler.text;
            UsersRankInfo userInfo = JsonUtility.FromJson<UsersRankInfo>(resultStr);
            
            successCallback?.Invoke();
            
            return userInfo;
        }
    }

    // 승리 카운트 업데이트
    public async UniTask AddWinCount(Action successCallback = null, Action failureCallback = null)
    {
        string sid = PlayerPrefs.GetString("sid"); 
        if (string.IsNullOrEmpty(sid))
        {
            Debug.LogWarning("세션 쿠키가 없습니다. 승리 카운트 업데이트 불가.");
            failureCallback?.Invoke();
            return;
        }
    
        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/addwincount", UnityWebRequest.kHttpVerbPOST))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);
    
            try
            {
                await www.SendWebRequest().ToUniTask();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("서버 승리 카운트 업데이트 성공");
                    successCallback?.Invoke();
                }
                else
                {
                    Debug.LogWarning("승리 카운트 업데이트 실패: " + www.error);
                    failureCallback?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("승리 카운트 업데이트 중 예외 발생: " + ex.Message);
                failureCallback?.Invoke();
            }
        }
    }
    
    // 패배 카운트 업데이트
    public async UniTask AddLoseCount(Action successCallback = null, Action failureCallback = null)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (string.IsNullOrEmpty(sid))
        {
            Debug.LogWarning("세션 쿠키가 없습니다. 패배 카운트 업데이트 불가.");
            failureCallback?.Invoke();
            return;
        }
    
        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/addlosecount", UnityWebRequest.kHttpVerbPOST))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);
    
            try
            {
                await www.SendWebRequest().ToUniTask();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("서버 패배 카운트 업데이트 성공");
                    successCallback?.Invoke();
                }
                else
                {
                    Debug.LogWarning("패배 카운트 업데이트 실패: " + www.error);
                    failureCallback?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("패배 카운트 업데이트 중 예외 발생: " + ex.Message);
                failureCallback?.Invoke();
            }
        }
    }
    
    // 오목 기록 추가
    public async UniTask AddOmokRecord(
        string recordId,
        string blackUserId, // 멀티플레이 시 흑 플레이어의 userId
        string whiteUserId, // 멀티플레이 시 백 플레이어의 userId
        List<(int y, int x, Enums.EPlayerType stone)> moves,
        Action successCallback = null,
        Action failureCallback = null)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (string.IsNullOrEmpty(sid))
        {
            Debug.LogWarning("세션 쿠키가 없습니다. 기보 저장 불가.");
            failureCallback?.Invoke();
            return;
        }

        List<OmokMove> moveList = new List<OmokMove>();
        foreach (var m in moves)
        {
            moveList.Add(new OmokMove { y = m.y, x = m.x, stone = (int)m.stone });
        }

        OmokRecordPayload requestBody = new OmokRecordPayload
        {
            recordId    = recordId,
            moves       = moveList,
            blackUserId = blackUserId, 
            whiteUserId = whiteUserId
        };

        string json   = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(Constants.ServerURL + "/users/addomokrecord", "POST"))
        {
            www.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);
            www.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await www.SendWebRequest().ToUniTask();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("오목 기보 저장 성공");
                    successCallback?.Invoke();
                }
                else
                {
                    Debug.LogWarning("오목 기보 저장 실패: " + www.error);
                    failureCallback?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("오목 기보 저장 중 예외: " + ex.Message);
                failureCallback?.Invoke();
            }
        }
    }
    
    // 오목 기록 가져오기
    public async UniTask<List<OmokRecord>> GetOmokRecords(
        Action successCallback = null, Action failureCallback = null)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (string.IsNullOrEmpty(sid))
        {
            Debug.LogWarning("세션 쿠키가 없습니다. 기보 조회 불가.");
            failureCallback?.Invoke();
            return new List<OmokRecord>();
        }

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/getomokrecords", "GET"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);

            try
            {
                await www.SendWebRequest().ToUniTask();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var resultStr = www.downloadHandler.text;

                    OmokRecordWrapper wrapper = 
                        JsonUtility.FromJson<OmokRecordWrapper>(resultStr);

                    successCallback?.Invoke();
                    return new List<OmokRecord>(wrapper.records);
                }
                else
                {
                    Debug.LogWarning("오목 기보 조회 실패: " + www.error);
                    failureCallback?.Invoke();
                    return new List<OmokRecord>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("오목 기보 조회 중 예외: " + ex.Message);
                failureCallback?.Invoke();
                return new List<OmokRecord>();
            }
        }
    }
    
    // 상대 정보 가져오기
    public async UniTask<UserInfoResult> GetUserInfoByUserId(string targetUserId,
        Action successCallback = null, Action failureCallback = null)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (string.IsNullOrEmpty(sid))
        {
            Debug.LogWarning("세션 쿠키가 없습니다.");
            failureCallback?.Invoke();
            return default;
        }

        string url = $"{Constants.ServerURL}/users/userinfo/{targetUserId}";
        using (UnityWebRequest www =
               new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);
            try
            {
                await www.SendWebRequest().ToUniTask();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var resultStr = www.downloadHandler.text;
                    UserInfoResult userInfo = JsonUtility.FromJson<UserInfoResult>(resultStr);
                    successCallback?.Invoke();
                    return userInfo;
                }
                else
                {
                    Debug.LogWarning("상대 정보 가져오기 실패: " + www.error);
                    failureCallback?.Invoke();
                    return default;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("상대 정보 가져오는 중 예외: " + ex.Message);
                failureCallback?.Invoke();
                return default;
            }
        }
    }

    /// <summary>
    /// 코인 추가
    /// </summary>
    /// <param name="amount">추가할 코인 수량</param>
    /// <param name="successCallback"></param>
    /// <param name="failureCallback"></param>
    /// <returns>비동기</returns>
    public async UniTask<CoinResult> AddCoin(int amount, Action<int> successCallback = null,
        Action failureCallback = null)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (string.IsNullOrEmpty(sid))
        {
            Debug.LogWarning("세션 쿠기가 없습니다. 코인 추가 불가");
            failureCallback?.Invoke();
            return new CoinResult { success = false, coin = 0, message = "세션 쿠키가 없습니다" };
        }

        CoinData coinData = new CoinData { amount = amount };
        string jsonStr = JsonUtility.ToJson(coinData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonStr);

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/addcoin", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);
            www.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await www.SendWebRequest().ToUniTask();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("코인 추가 실패: " +www.error);
                    failureCallback?.Invoke();
                    return new CoinResult { success = false, coin = 0, message = www.error };
                }

                var resultStr = www.downloadHandler.text;
                CoinResult coinResult = JsonUtility.FromJson<CoinResult>(resultStr);

                if (coinResult.success)
                {
                    successCallback?.Invoke(coinResult.coin);
                }
                else
                {
                    failureCallback?.Invoke();
                }

                return coinResult;
            }
            catch (Exception ex)
            {
                Debug.LogError("코인 추가 중 예외 발생: " + ex.Message);
                failureCallback?.Invoke();
                return new CoinResult { success = false, coin = 0, message = ex.Message };
            }
        }
    }

    /// <summary>
    /// 코인이 부족한지 확인 후 소비하는 메서드
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="successCallback"></param>
    /// <param name="failureCallback"></param>
    /// <returns></returns>
    public async UniTask<CoinResult> ConsumeCoin(int amount, Action<int> successCallback = null, 
        Action failureCallback = null)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (string.IsNullOrEmpty(sid))
        {
            Debug.LogWarning("세션 쿠키가 없습니다. 코인 소비 불가");
            failureCallback?.Invoke();
            return new CoinResult { success = false, coin = 0, message = "세션 쿠키가 없습니다" };
        }
        
        // 유저 정보를 가져와서 코인 잔액을 확인합니다.
        UserInfoResult userInfo = await GetUserInfo(() => { }, () => { });

        if (userInfo.coin < amount)
        {
            Debug.LogWarning("코인이 부족합니다.");
            failureCallback?.Invoke();
            return new CoinResult { success = false, coin = 0, message = "코인이 부족합니다." };
        }
        
        CoinData coinData = new CoinData { amount = amount };
        string jsonStr = JsonUtility.ToJson(coinData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonStr);
        
        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/consumecoin", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);
            www.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await www.SendWebRequest().ToUniTask();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("코인 소비 실패: " + www.error);
                    failureCallback?.Invoke();
                    return new CoinResult { success = false, coin = 0, message = www.error };
                }

                var resultStr = www.downloadHandler.text;
                CoinResult coinResult = JsonUtility.FromJson<CoinResult>(resultStr);

                if (coinResult.success)
                {
                    successCallback?.Invoke(coinResult.coin);
                }
                else
                {
                    failureCallback?.Invoke();
                }

                return coinResult;
            }
            catch (Exception ex)
            {
                Debug.LogError("코인 소비 중 예외 발생: " + ex.Message);
                failureCallback?.Invoke();
                return new CoinResult { success = false, coin = 0, message = ex.Message };
            }
        }
    }

    public async UniTask RemoveAds(Action successCallback, Action failureCallback)
    {
        string sid = PlayerPrefs.GetString("sid");
        if (string.IsNullOrEmpty(sid))
        {
            failureCallback?.Invoke();
            return;
        }

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/removeads", UnityWebRequest.kHttpVerbPOST))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);

            try
            {
                await www.SendWebRequest().ToUniTask();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    successCallback?.Invoke();
                }
                else
                {
                    failureCallback?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("RemoveAds 예외 발생" + ex.Message);
                failureCallback?.Invoke();
            }
        }
    }
    
}