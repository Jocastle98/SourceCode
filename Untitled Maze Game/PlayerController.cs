using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Text itemText; 
    public float speedBoostMultiplier = 2f; // 스피드 포션 사용시 속도 증가 배수
    private bool isSpeedBoostActive = false;
    private float speedBoostDuration = 5f; // 스피드 포션 지속 시간 (초)
    private float speedBoostTimer = 0f;

    private int bulletCount = 0;
    private int speedUpCount = 0;
    private int stunGunCount = 0;

    public Text hudBulletsText;
    public Text hudSpeedUpText;
    public Text hudStunGunText;

    // FirstPersonController 스크립트를 참조하기 위한 변수
    private FirstPersonController firstPersonController;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 10f; // 총알 속도 추가
    
    public GameObject stunGunPrefab;
    public Transform stunGunSpawnPoint;
    public float stunGunSpeed = 15f; // 전기 충격기 발사 속도


    void Start()
    {
        // UI Text 초기화
        UpdateHUDText();

        // FirstPersonController 스크립트를 참조
        firstPersonController = GetComponent<FirstPersonController>();
    }

    void Update()
    {
        // 1을 누르면 총알 발사
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShootBullet();
        }

        // 2를 누르면 스피드 포션 사용
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseSpeedUp();
        }

        // 스피드 포션 사용 중인 경우 타이머 감소
        if (isSpeedBoostActive)
        {
            speedBoostTimer -= Time.deltaTime;

            if (speedBoostTimer <= 0f)
            {
                // 스피드 포션 지속 시간이 끝난 경우 원래 속도로 복구
                isSpeedBoostActive = false;
                firstPersonController.walkSpeed /= speedBoostMultiplier; // walkSpeed 수정
            }
        }

        // 3을 누르면 전기 충격기 발사
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ShootStunGun();
        }
    }

    void ShootStunGun()
    {
        if (stunGunCount > 0) // 전기 충격기 개수가 0보다 큰 경우에만 발사 가능
        {
            stunGunCount--;
            UpdateHUDText();

            // 전기 충격기를 생성하고 발사 방향 및 속도 설정
            GameObject stunGun = Instantiate(stunGunPrefab, stunGunSpawnPoint.position, stunGunSpawnPoint.rotation);

            // 전기 충격기에 Rigidbody 컴포넌트가 있을 경우 속도 설정
            Rigidbody stunGunRigidbody = stunGun.GetComponent<Rigidbody>();
            if (stunGunRigidbody != null)
            {
                stunGunRigidbody.velocity = stunGun.transform.forward * stunGunSpeed; // 전기 충격기 속도 설정
            }
        }
    }

    void ShootBullet()
    {
        if (bulletCount > 0) // 총알 개수가 0보다 큰 경우에만 발사 가능
        {
            bulletCount--;
            UpdateHUDText();

            // 총알을 생성하고 발사 방향 설정
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            
            // 총알에 Rigidbody 컴포넌트가 있을 경우 속도 설정
            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            if (bulletRigidbody != null)
            {
                bulletRigidbody.velocity = bullet.transform.forward * bulletSpeed;
            }
        }
    }

    void UseSpeedUp()
    {
        if (speedUpCount > 0) // 스피드 포션 개수가 0보다 큰 경우에만 사용 가능
        {
            speedUpCount--;
            UpdateHUDText();

            // 스피드 포션 사용 중인 경우 기존 타이머 리셋
            if (isSpeedBoostActive)
            {
                speedBoostTimer = speedBoostDuration;
            }
            else
            {
                // 스피드 포션 사용 중이 아닌 경우 속도 증가
                isSpeedBoostActive = true;
                firstPersonController.walkSpeed *= speedBoostMultiplier; // walkSpeed 수정
                speedBoostTimer = speedBoostDuration;
            }
        }
    }

    void UpdateHUDText()
    {
        if (hudBulletsText != null)
        {
            hudBulletsText.text = $"1.Bullets: {bulletCount}";
        }

        if (hudSpeedUpText != null)
        {
            hudSpeedUpText.text = $"2.Speed Pills: {speedUpCount}";
        }

        if (hudStunGunText != null)
        {
            hudStunGunText.text = $"3.Stun Guns: {stunGunCount}";
        }
    }

    public void IncreaseItemCount(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Bullet:
                bulletCount++;
                UpdateHUDText();
                break;
            case ItemType.SpeedUp:
                speedUpCount++;
                UpdateHUDText();
                break;
            case ItemType.StunGun:
                stunGunCount++;
                UpdateHUDText();
                break;
            // 다른 아이템 타입에 대한 처리 추가 가능
        }
    }
}
