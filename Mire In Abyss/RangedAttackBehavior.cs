using System.Collections;
using AudioEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Attack Behaviors/Ranged")]
public class RangedAttackBehavior : ScriptableObject, IAttackBehavior
{
    [Header("원거리 공격 설정")]
    public float Range = 10f;
    public GameObject ProjectilePrefab;
    public LayerMask HitLayer;
    public int Damage = 5;
    public float Cooldown = 1f;
    [Tooltip("트리거 대신 레이캐스트 즉발 궤적 사용 시 true")]
    public bool UseRaycastTracer = false;

    [Tooltip("투사체 초기 속도")]
    public float ProjectileSpeed = 25f;

    private bool mbReady = true;
    private Transform mSelf;
    private Transform mTarget;
    private EnemyBTController mController;

    public bool IsInRange(Transform self, Transform target)
        => Vector3.Distance(self.position, target.position) <= Range;

    public void Attack(Transform self, Transform target)
    {
        if (!mbReady || ProjectilePrefab == null) return;
        mbReady = false;

        mSelf = self;
        mTarget = target;
        mController = self.GetComponent<EnemyBTController>();

        self.GetComponent<Animator>()?.SetTrigger("Attack");
        self.GetComponent<MonoBehaviour>().StartCoroutine(ResetReady());
    }

    private IEnumerator ResetReady()
    {
        yield return new WaitForSeconds(Cooldown);
        mbReady = true;
    }

    public void FireLastPosition(Transform self, Transform target)
    {
        var fp = self.GetComponent<EnemyBTController>().FirePoint;
        if (fp == null || ProjectilePrefab == null || target == null) return;

        Vector3 targetPos = target.position + Vector3.up * 1.2f; 
        Vector3 dir = (targetPos - fp.position);
        if (dir.sqrMagnitude < 0.01f) dir = fp.forward;
        dir.Normalize();

        AudioManager.Instance.PlayPoolSfx(ExSfxType.ArrowStart);
        Vector3 spawnPos = fp.position + dir * 0.2f; 
        var proj = Instantiate(ProjectilePrefab, spawnPos, Quaternion.LookRotation(dir));
        if (proj.TryGetComponent<Projectile>(out var ps))
            ps.Initialize(dir, ProjectileSpeed, HitLayer, Damage, self);

        Debug.DrawRay(spawnPos, dir * 5f, Color.red, 1f);
        Debug.Log($"[Arrow] SpawnPos: {spawnPos}, Dir: {dir}, TargetPos: {target.position}");
    }

}