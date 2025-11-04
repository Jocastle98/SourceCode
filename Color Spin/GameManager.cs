using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string introSceneName = "Intro_Scene";
    public string mainSceneName = "Main_Scene";
    public Text timerText;
    public Text scoreText; // 점수 텍스트 추가
    public GameObject gameOverUI; // 게임 오버 UI
    private Ball ball; // Ball 스크립트 참조

    private float timer;
    public bool isGameStarted; // public으로 변경
    private int score;

    void Start()
    {
        gameOverUI.SetActive(false);
        ball = FindObjectOfType<Ball>(); // Ball 객체 찾기
        if (ball == null)
        {
            Debug.LogError("Ball object not found in the scene!");
        }
        InitializeGame();
    }

    void Update()
    {
        if (isGameStarted)
        {
            timer += Time.deltaTime;
            UpdateTimer();
        }
        else
        {
            CheckForInput();
        }
    }

    void InitializeGame()
    {
        isGameStarted = false;
        timer = 0.0f; // 타이머 초기값 0
        score = 0; // 점수 초기값 0
        UpdateTimer();
        UpdateScore(); // 초기 점수 업데이트
        if (ball != null)
        {
            ball.ResetPosition(); // 공 위치 초기화
        }
    }

    void CheckForInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        Debug.Log("StartGame called");
        isGameStarted = true;
        Time.timeScale = 1f; // 게임 속도 복원
        if (ball != null)
        {
            ball.StartMoving(); // 공 움직이기 시작
        }
    }

    void UpdateTimer()
    {
        // 최적화를 위해 문자열 포맷 대신 숫자 계산을 이용한 텍스트 업데이트
        int minutes = (int)(timer / 60);
        int seconds = (int)(timer % 60);
        timerText.text = $"{minutes:D2}:{seconds:D2}";
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScore();
    }

    void UpdateScore()
    {
        // 최적화를 위해 문자열 포맷 대신 간단한 연결 방식 사용
        scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        Debug.Log("Game over function called");
        SaveScoreAndTime();
        // 게임 오버 UI 활성화
        gameOverUI.SetActive(true);
        isGameStarted = false;
        Time.timeScale = 0f; // 게임을 멈춤
    }

    public void Retry()
    {
        Debug.Log("Retry button clicked");
        Time.timeScale = 1f; // 게임 속도 복원
        SceneManager.LoadScene(mainSceneName); // 메인 씬 다시 로드
    }

    public void Exit()
    {
        Debug.Log("Exit button clicked");
        Time.timeScale = 1f; // 게임 속도 복원
        SceneManager.LoadScene(introSceneName); // 인트로 씬으로 이동
    }

    void SaveScoreAndTime()
    {
        int[] topScores = new int[3];
        float[] topTimes = new float[3];

        // 기존 기록 불러오기
        for (int i = 0; i < 3; i++)
        {
            topScores[i] = PlayerPrefs.GetInt("TopScore" + i, 0);
            topTimes[i] = PlayerPrefs.GetFloat("TopTime" + i, 0f);
        }

        // 새로운 기록 추가
        int newScore = score;
        float newTime = timer;

        // 기록 정렬
        for (int i = 0; i < 3; i++)
        {
            if (newScore > topScores[i] || (newScore == topScores[i] && newTime < topTimes[i]))
            {
                // 현재 순위에서 밀려나는 기록을 저장
                int tempScore = topScores[i];
                float tempTime = topTimes[i];

                // 새로운 기록 삽입
                topScores[i] = newScore;
                topTimes[i] = newTime;

                // 밀려난 기록을 다음 순위로
                newScore = tempScore;
                newTime = tempTime;
            }
        }

        // 상위 3개 기록 저장
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.SetInt("TopScore" + i, topScores[i]);
            PlayerPrefs.SetFloat("TopTime" + i, topTimes[i]);
        }

        PlayerPrefs.Save();
    }
}
