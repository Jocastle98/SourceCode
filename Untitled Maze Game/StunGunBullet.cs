using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunGunBullet : MonoBehaviour
{
    public int damage = 100;            // 총알 데미지
    public float bulletSpeed = 10f;      // 총알 속도
    public float bulletLifetime = 2f;    // 총알 수명
    public float stunDuration = 3f;      // 스턴 지속 시간

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, bulletLifetime);  // 일정 시간 후에 총알 삭제
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);  // 총알을 전진 방향으로 이동
    }

    void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);  // 다른 물체와 충돌하면 총알 삭제
    }

}
