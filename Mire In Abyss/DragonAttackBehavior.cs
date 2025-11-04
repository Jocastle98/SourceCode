using System.Collections;
using AudioEnums;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "AI/Attack Behaviors/Dragon")]
public class DragonAttackBehavior : ScriptableObject, IAttackBehavior
{
    [Header("꼬리 공격 (기본)")]
    public float TailRange = 3f;
    public int TailDamage = 10;

    [Header("파이어볼 공격")]
    public float FireballRange = 8f;
    public GameObject FireBallPrefab;
    public LayerMask HitLayer;
    public int FireballDamage = 20;
    public float FireballSpeed = 20f;
    public float FireballCooldown = 5f;
    private float mLastFireTime = -Mathf.Infinity;
    private Vector3 mLastValidFireballTargetPos;

    private Transform mSelf;
    private Transform mTarget;
    private EnemyBTController mController;

    [Header("브레스 공격")]
    public float BreathRange = 10f;
    public GameObject BreathProjectorPrefab;
    public GameObject BreathVFXPrefab;
    public LayerMask BreathHitLayer;
    public float BreathAngle = 45f;
    public int BreathDamage = 30;
    public float BreathCooldown = 15f;
    private float mLastBreathTime = -Mathf.Infinity;
    
    private void OnEnable()
    {
        mLastBreathTime = Time.time;
        mLastFireTime = Time.time;
        
    }

    public bool CanFireball(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        bool cool = Time.time >= mLastFireTime + FireballCooldown;
        if (cool && dist <= FireballRange)
        {
            mLastValidFireballTargetPos = target.position;
            return true;
        }
        return false;
    }

    public bool CanBreath(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        return Time.time >= mLastBreathTime + BreathCooldown
               && dist <= BreathRange;
    }

    public bool CanTail(Transform self, Transform target)
    {
        return target != null
               && Vector3.Distance(self.position, target.position) <= TailRange;
    }

    public bool IsInRange(Transform self, Transform target)
    {
        if (target == null) return false;
        float d = Vector3.Distance(self.position, target.position);
        return d <= TailRange || d <= FireballRange || d <= BreathRange;
    }

    public void Attack(Transform self, Transform target)
    {
        var anim = self.GetComponent<Animator>();

        if (CanBreath(self, target))
        {
            mLastBreathTime = Time.time;
            anim.SetTrigger("Breath");
        }
        else if (CanFireball(self, target))
        {
            mLastFireTime = Time.time;
            anim.SetTrigger("FireBall");
        }
        else if (CanTail(self, target))
        {
            anim.SetTrigger("TailAttack");
        }
    }


    // Ranger 참고해서 설정
    public void FireLastPosition(Transform self)
    {
        var fp = self.GetComponent<EnemyBTController>().FirePoint;
        if (fp == null || FireBallPrefab == null) return;

        Vector3 dir = mLastValidFireballTargetPos - fp.position;
        if (dir.sqrMagnitude < 0.01f)
            dir = self.forward;
        dir.Normalize();
        AudioManager.Instance.PlayPoolSfx(ExSfxType.DragonFireBall);
        var proj = Instantiate(FireBallPrefab, fp.position, Quaternion.LookRotation(dir));
        if (proj.TryGetComponent<Projectile>(out var ps))
            ps.Initialize(dir, FireballSpeed, HitLayer, FireballDamage, self);
    }
}
