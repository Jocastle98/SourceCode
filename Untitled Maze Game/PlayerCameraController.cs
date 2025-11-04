using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public float sensitivity = 2.0f; // 마우스 감도
    public Transform playerBody; // 플레이어의 회전을 조작할 대상

    float rotationX = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서를 중앙에 고정
    }

    void Update()
    {
        // 마우스 입력 감지
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // 수평 회전
        playerBody.Rotate(Vector3.up * mouseX);

        // 수직 회전
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // 수직 회전 각도 제한
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}