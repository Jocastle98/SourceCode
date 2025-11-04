using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float mSpeed = 15f;

    public Transform ShooterTransform { get; private set; }
    private Rigidbody mRb;
    private Collider mCol;
    private LayerMask mHitLayer;
    private int mDamage;

    
    private bool  mIsBreath = false;
    private float mLastDamageTime;
    private float mDamageInterval = 0.3f;
    private void Awake()
    {
        mRb = GetComponent<Rigidbody>();
        mCol = GetComponent<Collider>();

        mRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        mRb.useGravity = false;
        mCol.isTrigger = true;
    }

    /// <summary>
    /// RangedAttackBehavior에서 호출하세요.
    /// </summary>
    public void Initialize(Vector3 direction, float speed, LayerMask hitLayer, int damage, Transform shooter = null)
    {
        ShooterTransform = shooter;
        mRb.velocity = direction.normalized * speed;
        mHitLayer = hitLayer;
        mDamage = damage;

        Destroy(gameObject, 5f);
    }
    
    public void InitializeBreath(LayerMask hitLayer, int damage)
    {
        mRb.velocity      = Vector3.zero;
        mHitLayer         = hitLayer;
        mDamage           = damage;
        mIsBreath         = true;
        mLastDamageTime   = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mIsBreath) return;

        if (ShooterTransform != null && other.transform == ShooterTransform)
            return;
        int layer = other.gameObject.layer;

        // 반사화살인지 구분하기
        bool isReflected = ShooterTransform != null 
                           && ShooterTransform.GetComponent<PlayerController>() != null;

        // 장애물 외 레이어인지
        if ((mHitLayer.value & (1 << layer)) == 0)
        {
            // 반사화살이 아니라면 장애물에 닿을때 0.5초후 파괴되어서 데미지 안주도록
            if (!isReflected)
            {
                Destroy(gameObject);
            }
            return;
        }

        // 플레이어 닿으면
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.SetHit(mDamage, transform, 1);
        }
        // 몬스터 닿으면 
        else if (other.TryGetComponent<EnemyBTController>(out var enemy))
        {
            enemy.SetHit(mDamage, -1);
        }

        // 패링 포함이라 패링 후에 화살이 몬스터한테 가는 사이 파괴될수있어서 시간조절 잘해야할듯
        Destroy(gameObject,6f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!mIsBreath) return;
        if ((mHitLayer.value & 1 << other.gameObject.layer) == 0) return;

        if (Time.time < mLastDamageTime + mDamageInterval) return;
        mLastDamageTime = Time.time;

        if (other.TryGetComponent<PlayerController>(out var player))
            player.SetHit(mDamage, transform, 2); // 2 = 브레스 연속딜 타입
    }
}