using UnityEngine;
using UnityEngine.Tilemaps; // Tilemaps 네임스페이스 추가

public class Ball : MonoBehaviour
{
    public float bounceForce = 500f;
    public float gravityTime = 3f; // 중력이 증가하는 시간 간격
    public float gravityAmount = 1f; // 중력이 증가하는 양
    public float minHeight = 5f; // 최소 튕김 높이
    private Vector2 lastPosition;

    private Rigidbody2D rb;
    private Vector2 initialPosition = new Vector2(960, 540); // 초기 위치 고정
    private float initialGravityScale;
    private float upAmount;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;
    private bool hasCollided; // 충돌 상태 추적
    private bool isMoving; // 공이 움직이고 있는지 확인
    public AudioClip collisionSound; // 충돌 효과음

    private Color[] colors = new Color[]
    {
        new Color(160/255f, 0/255f, 255/255f), // (160,0,255)
        new Color(255/255f, 0/255f, 0/255f),   // (255,0,0)
        new Color(0/255f, 255/255f, 0/255f),   // (0,255,0)
        new Color(0/255f, 0/255f, 255/255f),   // (0,0,255)
        new Color(90/255f, 90/255f, 90/255f),  // (90,90,90)
        new Color(255/255f, 255/255f, 255/255f) // (255,255,255)
    };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>(); // GameManager 스크립트 참조
        initialGravityScale = rb.gravityScale; // 초기 중력 스케일 저장
    }

    void Start()
    {
        lastPosition = transform.position = initialPosition; // 초기 위치 설정
        upAmount = 0f;
        hasCollided = false;
        isMoving = false; // 초기 상태에서 공은 움직이지 않음

        // 초기 색상 설정
        spriteRenderer.color = GetRandomColor();
    }

    void Update()
    {
        if (gameManager.isGameStarted && isMoving)
        {
            float deltaTime = Time.deltaTime; // Time.deltaTime을 한번만 호출
            // 시간이 지나면 중력 증가
            upAmount += deltaTime;
            if (upAmount >= gravityTime)
            {
                rb.gravityScale += gravityAmount;
                upAmount = 0f;
            }

            // 매 프레임 공의 현재 위치를 저장
            lastPosition = transform.position;
        }
    }

    public void ResetPosition()
    {
        transform.position = initialPosition;
        rb.velocity = Vector2.zero;
        rb.gravityScale = initialGravityScale; // 중력 스케일 초기화
        spriteRenderer.color = GetRandomColor();
        upAmount = 0f;
        hasCollided = false;
        isMoving = false; // 공은 초기 상태에서 움직이지 않음
    }

    public void StartMoving()
    {
        isMoving = true; // 공이 움직이기 시작함
    }

    private void ChangeColor()
    {
        spriteRenderer.color = GetRandomColor();
    }

    private Color GetRandomColor()
    {
        return colors[Random.Range(0, colors.Length)];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Tilemap"))
        {
            // 효과음 재생
            if (collisionSound != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlayEffect(collisionSound);
            }

            // 공을 마지막 위치로 되돌림
            transform.position = lastPosition;

            // 위쪽으로 튕기는 방향으로 설정하고, 최소 높이 보장
            Vector2 bounceDirection = Vector2.up;
            rb.velocity = Vector2.zero; // 현재 속도를 리셋
            rb.AddForce(bounceDirection * bounceForce);

            // 최소 속도를 설정하여 최소 높이까지 튕기도록 보장
            float minVelocity = Mathf.Sqrt(2 * rb.gravityScale * minHeight);
            if (rb.velocity.y < minVelocity)
            {
                rb.velocity = new Vector2(rb.velocity.x, minVelocity);
            }

            // 공의 색상과 타일맵의 색상을 비교하여 점수 증가 또는 게임 오버
            Tilemap tilemap = collision.collider.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                foreach (ContactPoint2D hit in collision.contacts)
                {
                    Vector3 hitPosition = hit.point - (Vector2)(0.01f * hit.normal);
                    Vector3Int tilePosition = tilemap.WorldToCell(hitPosition);
                    TileBase tile = tilemap.GetTile(tilePosition);
                    if (tile != null)
                    {
                        Color tileColor = tilemap.GetColor(tilePosition);
                        if (spriteRenderer.color == tileColor)
                        {
                            gameManager.AddScore(1); // 점수 1 증가
                        }
                        else
                        {
                            gameManager.GameOver(); // 게임 오버
                            return; // 이후 코드를 실행하지 않도록 리턴
                        }
                    }
                }
            }

            // 충돌 상태 설정
            hasCollided = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Tilemap") && hasCollided)
        {
            // 충돌이 끝났을 때 색상 변경
            ChangeColor();
            hasCollided = false;
        }
    }
}
