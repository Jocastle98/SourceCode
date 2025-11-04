using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Watercannon : MonoBehaviour
{
    [Header("WaterCannon Settings")] 
    [SerializeField] [Tooltip("플레이어를 밀어내는 힘의 크기")]
    private float pushForce = 10f;
    
    [SerializeField][Tooltip("이펙트가 앞으로 나아가는 속도")] 
    private float speed = 3f;
    
    [SerializeField][Tooltip("이펙트가 사라지기까지의 시간")] 
    private float lifetime = 1.5f;

    private BoxCollider2D waterCannonCollider;

    private void Awake()
    {
        waterCannonCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 충돌한 대상이 'Player' 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어의 현재 위치 가져옴
            Vector3 playerPosition = other.transform.position;

            // 물대포 콜라이더의 경계선을 계산
            Bounds colliderBounds = waterCannonCollider.bounds;
            float topEdge = colliderBounds.max.y; // 콜라이더의 윗부분 Y좌표
            float bottomEdge = colliderBounds.min.y; // 콜라이더의 아랫부분 Y좌표
            
            // 플레이어가 물대포의 중심보다 위에 있는지 아래에 있는지 판단
            if (playerPosition.y > transform.position.y)
            {
                // 플레이어가 위에서 집입 -> 플레이어의 Y위치를 물대포의 윗쪽 경계선으로 강제 고정
                other.transform.position = new Vector3(playerPosition.x, topEdge, playerPosition.z);
            }
            else
            {
                // 플레이어가 아래에서 진입 -> 플레이어의 Y위치를 물대포의 아랫쪽 경계선으로 강제 고정
                other.transform.position = new Vector3(playerPosition.x, bottomEdge, playerPosition.z);
            }
        }
    }
}
