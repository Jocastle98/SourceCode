using UnityEngine;

public class SpinHandle : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public Transform[] tilemapLayers; // 각 타일맵 레이어를 참조합니다.
    public Vector3 rotationCenter = Vector3.zero; // 고정된 회전 중심점

    void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                // 화면의 왼쪽 절반 터치 시 반시계 방향으로 회전
                if (touchPosition.x < Screen.width / 2)
                {
                    RotateLeft();
                }
                // 화면의 오른쪽 절반 터치 시 시계 방향으로 회전
                else if (touchPosition.x >= Screen.width / 2)
                {
                    RotateRight();
                }
            }
        }
    }

    void RotateLeft()
    {
        foreach (var layer in tilemapLayers)
        {
            layer.RotateAround(rotationCenter, Vector3.forward, 60f); // 고정된 중심을 기준으로 60도 회전
        }
    }

    void RotateRight()
    {
        foreach (var layer in tilemapLayers)
        {
            layer.RotateAround(rotationCenter, Vector3.back, 60f); // 고정된 중심을 기준으로 60도 회전
        }
    }
}
