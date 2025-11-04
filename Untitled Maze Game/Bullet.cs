using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damgage = 1000;
    public float bulletSpeed = 10f;
    public float bulletLifetime = 2f;

    // Start is called before the first frame update
    void Start()
    {
        // Start 메서드 구현
        // 객체가 생성될 때 초기화 또는 동작 수행
        Destroy(gameObject, bulletLifetime);  // 일정 시간 후에 총알 삭제
    }

    void Update()
    {
        // Update 메서드 구현
        // 프레임마다 호출되는 업데이트
        transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);  // 총알을 전진 방향으로 이동
    }

    void OnTriggerEnter(Collider other)
    {
        // OnTriggerEnter 메서드 구현
        // 총알이 다른 콜라이더와 충돌했을 때 호출
        Destroy(gameObject);  // 다른 물체와 충돌하면 총알 삭제
    }

    
}
