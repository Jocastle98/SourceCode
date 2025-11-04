using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Bullet,
    SpeedUp,
    StunGun,
    HPPill
}

public class Item : MonoBehaviour
{
    public float rotationSpeed = 50f; // 회전 속도
    public float moveDistance = 0.25f; // 상하 움직임 거리
    public float moveSpeed = 1f; // 상하 움직임 속도
    public ItemType itemType; // 아이템의 종류

    private float initialY; // 초기 Y 위치
    //public int healAmount = 20;

    // Start is called before the first frame update
    void Start()
    {
        initialY = transform.position.y;
        // 아이템을 30도로 기울이기
        transform.rotation = Quaternion.Euler(30f, 0f, 30f);
    }

    // Update is called once per frame
    void Update()
    {
        // 아이템 회전
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        // 아이템 상하 움직임
        float newY = initialY + Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌했을 때 아이템 획득
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.IncreaseItemCount(itemType);
                Destroy(gameObject); // 아이템 소멸
            }
        }
    }
}
