using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform[] points; // 스폰 지점 배열
    public GameObject monsterPrefab; // 몬스터 프리팹
    public float createTime = 2.0f; // 생성 주기
    public int maxMonster = 10; // 최대 몬스터 수
    public bool isGameOver = false; // 게임 오버 여부

    int prevTime; // 이전 시간 정보
    float countTime; // 누적 시간

    public GameObject itemPrefab_bullet;
    public GameObject itemPrefab_battery;
    public GameObject itemPrefab_pills;
    public GameObject itemPrefab_HPPill;
    int prevItemCheck; // 이전 아이템 체크 시간

    public static GameManager instance = null; // 싱글톤 인스턴스

    private void Awake()
    {
        instance = this; // 싱글톤 인스턴스 초기화
    }

    // 시작 시 실행되는 함수
    void Start()
    {
        // 스폰 지점을 찾아 배열에 할당
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();
    }

    // GameOver 함수 추가
    public void GameOver()
    {
        isGameOver = true;

        // 추가적인 게임 종료 로직을 여기에 추가
    }

    // 매 프레임마다 실행되는 함수
    void Update()
    {
        if (isGameOver == false && points.Length > 0) // 게임 오버가 아니고 스폰 지점이 존재할 때
        {
            countTime += Time.deltaTime; // 누적 시간 증가

            int currTime = (int)(countTime % createTime); // 현재 시간 정보
            if (currTime == 0 && prevTime != currTime) // 일정 시간마다 몬스터 생성
            {
                int monsterCount = (int)GameObject.FindGameObjectsWithTag("MONSTER").Length; // 현재 몬스터 수

                if (monsterCount < maxMonster) // 최대 몬스터 수보다 적을 때만 생성
                {
                    int idx = Random.Range(1, points.Length); // 랜덤한 스폰 지점 선택

                    if (Random.Range(0, 9) < 8) // 80% 확률로 몬스터 생성
                    {
                        Instantiate(monsterPrefab, points[idx].position, points[idx].rotation); // 몬스터 생성
                    }
                }
            }
            prevTime = currTime; // 현재 시간 정보 갱신

            // 아이템 생성
            int curItemTime = (int)(countTime % 5f); // 아이템 생성 주기
            if (curItemTime == 0 && prevItemCheck != curItemTime) // 일정 시간마다 아이템 생성
            {
                Vector3 randpos = new Vector3(Random.Range(-25f, 25f), 4.0f, Random.Range(-25, 25f)); // 랜덤한 위치 생성

                // 랜덤으로 아이템 중 하나 선택하여 생성
                int randomItem = Random.Range(0, 4);
                switch (randomItem)
                {
                    case 0:
                        Instantiate(itemPrefab_bullet, randpos, Quaternion.identity); // 총알 아이템 생성
                        break;
                    case 1:
                        Instantiate(itemPrefab_battery, randpos, Quaternion.identity); // 배터리 아이템 생성
                        break;
                    case 2:
                        Instantiate(itemPrefab_pills, randpos, Quaternion.identity); // 피룰스 아이템 생성
                        break;
                    case 3:
                        Instantiate(itemPrefab_HPPill, randpos, Quaternion.identity); // 체력 아이템 생성
                        break;
                }
            }

            prevItemCheck = curItemTime; // 이전 아이템 체크 시간 갱신
        }
    }
}
