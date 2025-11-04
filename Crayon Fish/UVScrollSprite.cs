using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UVScrollSprite : MonoBehaviour
{
    [SerializeField] float speed = 1f;

    // 텍스처 오프셋이 +로 증가할 때 실제 '오른쪽'으로 보이면 true, 아니면 false 한 번만 맞춰두세요.
    [SerializeField] bool texPositiveGoesRight = false; 

    float lookDir = 1f;   // 해마가 보는 방향(오른쪽=+1, 왼쪽=-1)
    float flipDir = 1f;   // 스프라이트를 Y로 뒤집었는지(+1/-1)

    Material runtimeMat;
    Vector2 offset;

    public void InitDirs(float lookSign, float spriteFlipSign)
    {
        lookDir  = Mathf.Sign(lookSign);
        flipDir  = Mathf.Sign(spriteFlipSign); // Y스케일로 뒤집었을 때 -1
    }

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        runtimeMat = Instantiate(sr.sharedMaterial);
        sr.material = runtimeMat;
    }

    void Update()
    {
        float baseTexSign = texPositiveGoesRight ? 1f : -1f;
        float final = lookDir * flipDir * baseTexSign;

        offset.x += speed * final * Time.deltaTime;
        runtimeMat.mainTextureOffset = offset;
    }
}