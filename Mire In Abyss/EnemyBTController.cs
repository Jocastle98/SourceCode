using System;
using System.Collections;
using Unity.VisualScripting;
using EnemyEnums;
using Events.HUD;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Events.HUD;
using UIHUDEnums;
using AudioEnums;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBTController : MonoBehaviour, IHpTrackable, IMapTrackable
{
    [Header("체력 설정")]  
    [SerializeField] private int mMaxHealth = 100;
    private int mCurrentHealth;
    private bool mbIsDead;

    [Header("방어력 설정")]  
    [SerializeField] private int mDefense = 5;

    [Header("감지 설정")]  
    [SerializeField] private float mDetectRadius = 10f;
    [SerializeField] private float mDetectAngle = 360f;
    [SerializeField] private LayerMask mPlayerMask;
    private Transform mTarget;
    public Transform Target => mTarget;

    [Header("순찰 설정")]  
    [SerializeField] private float mPatrolRadius = 5f;

    [Header("공격 설정")]  
    [SerializeField] private ScriptableObject mAttackBehaviorAsset;
    private IAttackBehavior mAttackBehavior;

    [Header("원거리 발사 위치 (원거리 스켈레톤, 드래곤)")]
    [SerializeField] private Transform mFirePoint;
    [SerializeField] private Transform mBreathPoint;
    public Transform FirePoint => mFirePoint;
    
    [Header("임팩트 설정 (골렘)")]
    [SerializeField] private GameObject ImpactProjectorPrefab;
    [SerializeField] private LayerMask ImpactHitLayer;
    private bool mImpactHandled = false;

    [Header("렌더러 설정")]  
    [SerializeField] private Renderer[] mRenderers;
    
    [Header("순찰 대기 시간 (초)")]
    [SerializeField] private float mPatrolWaitTime = 2f;
    private float mPatrolWaitTimer = 0f;
    private bool mPatrolPointAssigned = false;
    
    [Header("드래곤 공중")]
    [SerializeField] private float airInterval     = 30f; 
    [SerializeField] private float airDuration     = 10f; 
    private float mLastAirTime = -Mathf.Infinity;
    private bool  mIsFlying    = false;

    [Header("경험치 설정")] 
    [SerializeField] private EnemyType mEnemyType = EnemyType.Common;
    [SerializeField] private EnemySubType mEnemySubType;
    public EnemyType EnemyType => mEnemyType;

    [SerializeField] private EnemyExpRewardController mExpRewardController;
    
    [Header("몬스터 Hit 상태 설정")]
    [SerializeField] private float mStunDuration = 2f;
    [SerializeField] private float mAttackDebuffDuration = 10f;
    [SerializeField] private int mAttackDebuffAmount = 5;
    [SerializeField] private float mFireDotDuration = 3f;
    [SerializeField] private int mFireDotDamagePerSecond = 3;
    [SerializeField] private float mIceDebuffDuration = 8f;
    [SerializeField] private float mIceAnimSpeed = 0.2f;

    [Header("몬스터 UI 관련 설정")]
    [SerializeField] private Transform mHpBarAnchor;
    private float mOriginalAgentSpeed;
    private bool mbAttackDebuffed = false;
    private bool mbIsStunned = false;
    private float mOriginalAnimSpeed;
    
    private NavMeshAgent mAgent;
    private Animator mAnim;
    private BTNode mRoot;
    private bool mbIsAttacking;
    private bool mbIsHit;
    private bool mHasTriggeredDead;
    private Projector currentProjector;
    private bool mExpGiven = false;
    private ItemDropper itemDropper;
    private GameObject mBreathVFXInstance;
    private bool mbIgnoreHits = false;
    private bool mbIsBreathing = false;
    
    public System.Action monsterDead;

    public Transform HpAnchor => mHpBarAnchor;

    public Transform MapAnchor => transform;

    public MiniMapIconType IconType { get; private set; }
    
    private float mNextLoopTime = 0f;

    
    void Awake()
    {
        if (mAttackBehaviorAsset != null)
            mAttackBehaviorAsset = Instantiate(mAttackBehaviorAsset);
        mAgent = GetComponent<NavMeshAgent>();
        mAnim = GetComponent<Animator>();
        mAttackBehavior = mAttackBehaviorAsset as IAttackBehavior;
        mRenderers = GetComponentsInChildren<Renderer>();
        mExpRewardController = GetComponent<EnemyExpRewardController>();
        itemDropper = GetComponent<ItemDropper>();

        IconType = mEnemyType switch
        {
            EnemyType.Boss => MiniMapIconType.Boss,
            _ => MiniMapIconType.Enemy
        };
        TrackableEventHelper.PublishSpawned(this);
    }

    void Start()
    {
        mCurrentHealth = mMaxHealth;
        mNextLoopTime = Time.time + 1f;
        var deadSeq = new BTSequence(
            new BTCondition(() => mbIsDead && !mHasTriggeredDead),
            new BTAction(() =>
            {
                mHasTriggeredDead = true;
                ClearAllBools();
                mAnim.SetTrigger("Dead");
                switch (mEnemyType)
                {
                    case EnemyType.Boss:
                        AudioManager.Instance.StopLoopPoolSfx(ExSfxType.DragonVoice);
                        break;
                    case EnemyType.Elite:
                        AudioManager.Instance.StopLoopPoolSfx(ExSfxType.GolemTrace);
                        break;
                    case EnemyType.Common:
                        AudioManager.Instance.StopLoopPoolSfx(ExSfxType.SkeletonVoice);
                        break;
                    default:
                        break;
                }
                if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    mAgent.isStopped = true;
                mAgent.enabled = false;
            })
        );

        var hitSeq = new BTSequence(
            new BTCondition(() => mbIsHit),
            new BTAction(() =>
            {
                ClearAllBools();
                mAnim.SetTrigger("Hit");
                switch (mEnemyType)
                {
                    case EnemyType.Boss:
                        AudioManager.Instance.StopLoopPoolSfx(ExSfxType.DragonVoice);
                        break;
                    case EnemyType.Elite:
                        AudioManager.Instance.StopLoopPoolSfx(ExSfxType.GolemTrace);
                        break;
                    case EnemyType.Common:
                        AudioManager.Instance.StopLoopPoolSfx(ExSfxType.SkeletonVoice);
                        break;
                    default:
                        break;
                }
                mNextLoopTime = Time.time + 1f;
                StartCoroutine(HitColorChange());
                if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    mAgent.isStopped = true;
                mbIsHit = false;
            })
        );

        var detectCond = new BTCondition(DetectPlayer);
        var flightSeq = new BTSequence(
            new BTCondition(() =>
                !mIsFlying
                &&!mbIsAttacking 
                && mAttackBehaviorAsset is DragonAttackBehavior
                && mCurrentHealth <= mMaxHealth * 0.5f
                && mTarget != null
                && (mAttackBehavior as DragonAttackBehavior)
                .IsInRange(transform, mTarget)
                && Time.time >= mLastAirTime + airInterval 
            ),
            new BTAction(() =>
            {
                mbIsAttacking = false;
                ClearAllBools();

                mIsFlying    = true;
                mLastAirTime = Time.time;
                mAnim.SetBool("IsFlying", true);
                if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    mAgent.isStopped = true;
                mAgent.enabled = false;

                StartCoroutine(FlyAttackRoutine());
            })
        );
        

        BTNode engage;

        if (mAttackBehaviorAsset is GolemAttackBehavior)
        {
            var golem = mAttackBehavior as GolemAttackBehavior;
            var impactSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && golem.CanImpact(transform, mTarget)),
                new BTAction(() =>
                {
                    AudioManager.Instance.StopLoopPoolSfx(ExSfxType.GolemTrace);
                    FaceTarget();
                    mbIsAttacking = true;
                    ClearAllBools();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                        mAgent.isStopped = true;
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var swingSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && golem.CanSwing(transform, mTarget)),
                new BTAction(() =>
                {
                    AudioManager.Instance.StopLoopPoolSfx(ExSfxType.GolemTrace);
                    FaceTarget();
                    mbIsAttacking = true;
                    ClearAllBools();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                        mAgent.isStopped = true;
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var traceSeq = new BTAction(() =>
            {
                if (mbIsAttacking || mTarget == null) return;
                float d = Vector3.Distance(transform.position, mTarget.position);
                if (d > golem.SwingRange && d > golem.ImpactRange)
                {
                    ClearAllBools();
                    mAnim.SetBool("Trace", true);
                    if (Time.time >= mNextLoopTime) 
                        AudioManager.Instance.PlayLoopPoolSfx(ExSfxType.GolemTrace);
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.isStopped = false;
                        mAgent.SetDestination(mTarget.position);
                    }
                }
            });

            engage = new BTSelector(impactSeq, swingSeq, traceSeq);
        }
        else if (mAttackBehaviorAsset is RangedAttackBehavior)
        {
            var ranged = mAttackBehavior as RangedAttackBehavior;
            var rangedAttackSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && ranged.IsInRange(transform, mTarget)),
                new BTAction(() =>
                {
                    FaceTarget();
                    mbIsAttacking = true;
                    ClearAllBools();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                        mAgent.isStopped = true;
                    mAnim.SetTrigger("Attack");
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var rangedAimSeq = new BTSequence(
                new BTCondition(() => mTarget != null && mAttackBehavior.IsInRange(transform, mTarget)),
                new BTAction(FaceTarget)
            );

            var rangedTrace = new BTAction(() =>
            {
                if (mbIsAttacking || mTarget == null) return;
                if (!mAttackBehavior.IsInRange(transform, mTarget))
                {
                    ClearAllBools();
                    mAnim.SetBool("Trace", true);
                    if (Time.time >= mNextLoopTime)
                    {
                        AudioManager.Instance.PlayLoopPoolSfx(ExSfxType.SkeletonVoice);
                        mNextLoopTime = Time.time + 1f;
                    }
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.isStopped = false;
                        mAgent.SetDestination(mTarget.position);
                    }
                }
            });

            engage = new BTSelector(rangedAttackSeq, rangedAimSeq, rangedTrace);
        }
        else if(mAttackBehaviorAsset is MeleeAttackBehavior)
        {
            var attackSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && mTarget != null && mAttackBehavior.IsInRange(transform, mTarget)),
                new BTAction(() =>
                {
                    mbIsAttacking = true;
                    ClearAllBools();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                        mAgent.isStopped = true;
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var traceSeq = new BTAction(() =>
            {
                if (mbIsAttacking || mTarget == null) return;
                if (!mAttackBehavior.IsInRange(transform, mTarget))
                {
                    ClearAllBools();
                    mAnim.SetBool("Trace", true);
                    if (Time.time >= mNextLoopTime)
                    {
                        AudioManager.Instance.PlayLoopPoolSfx(ExSfxType.SkeletonVoice);
                        mNextLoopTime = Time.time + 1f;
                    }
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.isStopped = false;
                        mAgent.SetDestination(mTarget.position);
                    }
                }
            });

            engage = new BTSelector(attackSeq, traceSeq);
        }
        else if (mAttackBehaviorAsset is DragonAttackBehavior)
        {
            var dragon = mAttackBehavior as DragonAttackBehavior;
            var fireballSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && mTarget != null && dragon.CanFireball(transform, mTarget)),
                new BTAction(() =>
                {
                    AudioManager.Instance.StopLoopPoolSfx(ExSfxType.DragonVoice);
                    FaceTarget();
                    mbIsAttacking = true;
                    ClearAllBools();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                        mAgent.isStopped = true;
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var breathSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && mTarget != null && dragon.CanBreath(transform, mTarget)),
                new BTAction(() =>
                {
                    AudioManager.Instance.StopLoopPoolSfx(ExSfxType.DragonVoice);
                    FaceTarget();
                    mbIsAttacking = true;
                    ClearAllBools();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                        mAgent.isStopped = true;
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var tailSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && mTarget != null && dragon.CanTail(transform, mTarget)),
                new BTAction(() =>
                {
                    AudioManager.Instance.StopLoopPoolSfx(ExSfxType.DragonVoice);
                    FaceTarget();
                    mbIsAttacking = true;
                    ClearAllBools();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                        mAgent.isStopped = true;
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var traceSeq = new BTAction(() =>
            {
                if (mbIsAttacking || mTarget == null) return;
                float d = Vector3.Distance(transform.position, mTarget.position);
                if (d > dragon.FireballRange && d > dragon.BreathRange && d > dragon.TailRange)
                {
                    ClearAllBools();
                    mAnim.SetBool("Trace", true);
                    if (Time.time >= mNextLoopTime)
                    {
                        AudioManager.Instance.PlayLoopPoolSfx(ExSfxType.DragonVoice);
                        mNextLoopTime = Time.time + 1f;
                    }
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.isStopped = false;
                        mAgent.SetDestination(mTarget.position);
                    }
                }
            });

            engage = new BTSelector(breathSeq, fireballSeq, tailSeq, traceSeq);
        }
        else
        {
            engage = new BTAction(() => {});
            Debug.LogWarning("다른 AttackBehavior를 추가해서 넣을 예쩡");
        }

        var patrol = new BTAction(() =>
        {
            if (mbIsAttacking) return;

            ClearAllBools();
            if (mAgent == null || !mAgent.enabled || !mAgent.isOnNavMesh)
                return;

            if (mAgent.pathPending) return;
            if (mEnemyType == EnemyType.Elite && Time.time >= mNextLoopTime)
            {
                AudioManager.Instance.PlayLoopPoolSfx(ExSfxType.GolemTrace);
                mNextLoopTime = Time.time + 1f;
            }
            if (!mPatrolPointAssigned)
            {
                var rnd = Random.insideUnitSphere * mPatrolRadius + transform.position;
                if (NavMesh.SamplePosition(rnd, out var hit, mPatrolRadius, NavMesh.AllAreas))
                {
                    
                    mAgent.SetDestination(hit.position);
                    mPatrolPointAssigned = true;
                    mPatrolWaitTimer = 0f;
                }
            }
            else
            {
                if (mAgent.remainingDistance <= mAgent.stoppingDistance)
                {
                    mAgent.isStopped = true;
                    mAnim.SetBool("Idle", true);
                    mPatrolWaitTimer += Time.deltaTime;
                    if (mPatrolWaitTimer >= mPatrolWaitTime)
                    {
                        mPatrolPointAssigned = false;
                        mAgent.isStopped = false;
                    }
                }
                else
                {
                    mAgent.isStopped = false;
                    mAnim.SetBool("Patrol", true);
                    mPatrolWaitTimer = 0f;
                }
            }
        });

        var idle = new BTAction(() =>
        {
            if (mbIsAttacking) return;
            ClearAllBools();
            if (mAgent == null || !mAgent.enabled || !mAgent.isOnNavMesh)
                return;
            mAnim.SetBool("Idle", true);
            mAgent.isStopped = true;
        });

        mRoot = new BTSelector(deadSeq,flightSeq, hitSeq, new BTSequence(detectCond, engage), patrol, idle);
    }

    void Update()
    {
        if (mIsFlying || mHasTriggeredDead || mbIsStunned || mbIsBreathing)
            return;
        mRoot.Tick();
    }

    private void FaceTarget()
    {
        if (mTarget == null) return;
        Vector3 dir = (mTarget.position - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void ClearAllBools()
    {
        
        mAnim.SetBool("Patrol", false);
        mAnim.SetBool("Trace", false);
        mAnim.SetBool("Idle", false);
        if (mAttackBehaviorAsset is DragonAttackBehavior)
        {
            mAnim.SetBool("FlyTrace", false);
        }
    }


    #region 드래곤 공중

    private IEnumerator FlyAttackRoutine()
    {
        mAnim.SetBool("IsFlying", true);
        AudioManager.Instance.PlayPoolSfx(ExSfxType.DragonFly);
        yield return new WaitForSeconds(1.5f);
        

        mAgent.enabled = true;
        mAgent.isStopped = false;
        

        float t0 = Time.time;
        while (Time.time - t0 < airDuration)
        {
            if (DetectPlayer() && mTarget != null)
            {
                var dragon = mAttackBehavior as DragonAttackBehavior;

                if (dragon.CanFireball(transform, mTarget))
                {
                    ClearAllBools();
                    mAnim.SetTrigger("FlyFireBall");
                    FaceTarget();
                    yield return new WaitForSeconds(0.8f);
                }
                else
                {
                    ClearAllBools();
                    mAnim.SetBool("FlyTrace", true);
                    FaceTarget();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.SetDestination(mTarget.position);
                    }
                    yield return null;
                    continue;
                }
            }
            yield return null;
        }

        mAgent.isStopped = true;
        mAgent.enabled = false;
        ClearAllBools();

        mAnim.SetTrigger("Land");
        yield return new WaitForSeconds(1.2f);

        mAnim.SetBool("IsFlying", false);
        mIsFlying = false;
        ClearAllBools();
    }


    #endregion
    
    #region 플레이어 감지
    private bool DetectPlayer()
    {
        var hits = Physics.OverlapSphere(transform.position, mDetectRadius, mPlayerMask);
        foreach (var h in hits)
        {
            var dir = (h.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dir) <= mDetectAngle * 0.5f)
            {
                mTarget = h.transform;
                return true;
            }
        }
        mTarget = null;
        return false;
    }
    #endregion

    #region Hit & Death 처리

    public void SetHit(int damage, int hitType)
    {
        if (mbIsDead || mbIgnoreHits) return;
        switch (mEnemyType)
        {
            case EnemyType.Common:
                AudioManager.Instance.PlayPoolSfx(ExSfxType.SkeletonHit);
                break;
            case EnemyType.Elite:
                AudioManager.Instance.PlayPoolSfx(ExSfxType.GolemHit);
                break;
            case EnemyType.Boss:
                AudioManager.Instance.PlayPoolSfx(ExSfxType.DragonHit);
                break;
            default:
                break;
        }
        
        switch (hitType)
        {
            case 0: //스턴
                ApplyDamage(damage);
                StartCoroutine(StunRoutine());
                break;
            case 1: // 공격력 감소
                ApplyDamage(damage);
                StartCoroutine(AttackDebuffRoutine());
                break;
            case 2: // 화염 도트뎀(해당 코루틴에 데미지 포함)
                StartCoroutine(FireDotRoutine());
                break;
            case 3: // 얼음 속도감소 
                ApplyDamage(damage);
                StartCoroutine(IceDebuffRoutine());
                break;
            default:
                ApplyDamage(damage);
                break;
        }
    }
    private void ApplyDamage(int damage)
    {
        int effective = Mathf.Max(0, damage - mDefense);
        mCurrentHealth -= effective;
        if (effective >= 100)
        {
            PlayerHub.Instance.QuestLog.AddProgress("Q007", 1);
        }
        Debug.Log($"받은 대미지:{damage} 방어력:{mDefense} 최종:{effective} 남은체력:{mCurrentHealth}");
        if (mCurrentHealth <= 0)
        {
            mbIsDead = true;
            switch (mEnemyType)
            {
                case EnemyType.Boss:
                    AudioManager.Instance.PlayPoolSfx(ExSfxType.DragonDie);
                    break;
                case EnemyType.Elite:
                    AudioManager.Instance.PlayPoolSfx(ExSfxType.GolemDie);
                    break;
                case EnemyType.Common:
                    AudioManager.Instance.PlayPoolSfx(ExSfxType.SkeletonDie);
                    break;
                default:
                    break;
            }
        }
        else mbIsHit = true;

        R3EventBus.Instance.Publish(new Events.Combat.DamagePopup(transform.position, effective));
        R3EventBus.Instance.Publish(new Events.Combat.EnemyHpChanged(this.GetInstanceID(), mCurrentHealth, mMaxHealth));
        if(mEnemyType == EnemyType.Boss)
        {
            R3EventBus.Instance.Publish(new Events.Combat.BossHpChanged(this.GetInstanceID(), mCurrentHealth, mMaxHealth));
        }
    }

    // 스턴 
    private IEnumerator StunRoutine()
    {
        mbIsStunned = true;
        mOriginalAnimSpeed = mAnim.speed;
        mAnim.speed = 0f;

        if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
            mAgent.isStopped = true;
        yield return new WaitForSeconds(mStunDuration);

        mAnim.speed = mOriginalAnimSpeed;
        if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
            mAgent.isStopped = false;
        mbIsStunned = false;
    }
    // 공격력 감소 
    private IEnumerator AttackDebuffRoutine()
{
    if (mbAttackDebuffed) yield break;
    mbAttackDebuffed = true;

    int origMeleeDamage = 0, origRangedDamage = 0;
    int origSwingDamage = 0, origImpactDamage = 0;
    int origTailDamage = 0, origFireballDamage = 0, origBreathDamage = 0;

    if (mAttackBehaviorAsset is MeleeAttackBehavior melee)
    {
        origMeleeDamage = melee.Damage;
        melee.Damage = Mathf.Max(0, melee.Damage - mAttackDebuffAmount);
    }
    else if (mAttackBehaviorAsset is RangedAttackBehavior ranged)
    {
        origRangedDamage = ranged.Damage;
        ranged.Damage = Mathf.Max(0, ranged.Damage - mAttackDebuffAmount);
    }
    else if (mAttackBehaviorAsset is GolemAttackBehavior golem)
    {
        origSwingDamage  = golem.SwingDamage;
        origImpactDamage = golem.ImpactDamage;
        golem.SwingDamage  = Mathf.Max(0, golem.SwingDamage  - mAttackDebuffAmount);
        golem.ImpactDamage = Mathf.Max(0, golem.ImpactDamage - mAttackDebuffAmount);
    }
    else if (mAttackBehaviorAsset is DragonAttackBehavior dragon)
    {
        origTailDamage     = dragon.TailDamage;
        origFireballDamage = dragon.FireballDamage;
        origBreathDamage   = dragon.BreathDamage;
        dragon.TailDamage     = Mathf.Max(0, dragon.TailDamage     - mAttackDebuffAmount);
        dragon.FireballDamage = Mathf.Max(0, dragon.FireballDamage - mAttackDebuffAmount);
        dragon.BreathDamage   = Mathf.Max(0, dragon.BreathDamage   - mAttackDebuffAmount);
    }

    yield return new WaitForSeconds(mAttackDebuffDuration);

    // 데미지 복구
    if (mAttackBehaviorAsset is MeleeAttackBehavior melee2)
        melee2.Damage = origMeleeDamage;
    else if (mAttackBehaviorAsset is RangedAttackBehavior ranged2)
        ranged2.Damage = origRangedDamage;
    else if (mAttackBehaviorAsset is GolemAttackBehavior golem2)
    {
        golem2.SwingDamage  = origSwingDamage;
        golem2.ImpactDamage = origImpactDamage;
    }
    else if (mAttackBehaviorAsset is DragonAttackBehavior dragon2)
    {
        dragon2.TailDamage     = origTailDamage;
        dragon2.FireballDamage = origFireballDamage;
        dragon2.BreathDamage   = origBreathDamage;
    }

    mbAttackDebuffed = false;
}

    // 화염 도트 데미지 1초마다 fireDuration까지 데미지
    private IEnumerator FireDotRoutine()
    {
        float elapsed = 0f;
        while (elapsed < mFireDotDuration)
        {
            ApplyDamage(mFireDotDamagePerSecond);
            elapsed += 1f;
            yield return new WaitForSeconds(1f);
        }
    }

    // 얼음디버프 속도 감소 
    private IEnumerator IceDebuffRoutine()
    {
        mOriginalAnimSpeed  = mAnim.speed;
        mOriginalAgentSpeed = mAgent.speed;

        mAnim.speed  = mIceAnimSpeed;
        mAgent.speed = mOriginalAgentSpeed * mIceAnimSpeed;
        yield return new WaitForSeconds(mIceDebuffDuration);

        mAnim.speed  = mOriginalAnimSpeed;
        mAgent.speed = mOriginalAgentSpeed;
    }

    public void OnHitAnimationExit()
    {
        mbIsHit = false;
        if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
        {
            if (DetectPlayer())
            {
                mAnim.SetBool("Trace", true);
                mAgent.isStopped = false;
                mAgent.SetDestination(mTarget.position);
            }
        }
    }

    #region Hit시 머터리얼 변화
    

    private IEnumerator HitColorChange()
    {
        var block = new MaterialPropertyBlock();
        float t = 0f;
        const float duration = 0.5f;
        while (t < duration)
        {
            t += Time.deltaTime;
            ChangeColorRenderer(Color.Lerp(Color.red, Color.white, t / duration), block);
            yield return null;
        }
        ChangeColorRenderer(Color.white, block);
    }

    private IEnumerator Dissolve()
    {
       
        TrackableEventHelper.PublishDestroyed(this);
        var block = new MaterialPropertyBlock();
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime;
            ChangeColorRenderer(new Color(1, 1, 1, alpha), block);
            yield return null;
        }
        Destroy(gameObject);
    }

    private void ChangeColorRenderer(Color color, MaterialPropertyBlock block)
    {
        foreach (var r in mRenderers)
        {
            r.GetPropertyBlock(block);
            block.SetColor("_Color", color);
            r.SetPropertyBlock(block);
        }
    }

    #endregion

    #region Dead시 상태

    public void OnDeadAnimationExit()
    {
        itemDropper.DropItemOnDeadth();
        GiveExpReward();
        AbyssManager.Instance.AddSoulStoneFromEnemy(EnemyType);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            playerController.OnEnemyKilled();
        }

        switch (mEnemySubType)
        {
            case  EnemySubType.MeleeSkeleton:
                PlayerHub.Instance.QuestLog.AddProgress("Q001", 1);
                break;
            case  EnemySubType.RangerSkeleton:
                PlayerHub.Instance.QuestLog.AddProgress("Q004", 1);
                break;
            case  EnemySubType.Golem:
                PlayerHub.Instance.QuestLog.AddProgress("Q012", 1);
                break;
            case  EnemySubType.Dragon:
                PlayerHub.Instance.QuestLog.AddProgress("Q014", 1);
                PlayerHub.Instance.QuestLog.AddProgress("Q011", 1);
                AchievementManager.Instance.AddProgress("A004", 1);
                AchievementManager.Instance.AddProgress("A005", 1);
                if (PlayerHub.Instance.Inventory.Items == null)
                {
                    AchievementManager.Instance.AddProgress("A014", 1);
                }
                break;
            
        }
        PlayerHub.Instance.QuestLog.AddProgress("Q009", 1);
        AchievementManager.Instance.AddProgress("A001", 1);
        AchievementManager.Instance.AddProgress("A002", 1);
        AchievementManager.Instance.AddProgress("A003", 1);
        
        monsterDead.Invoke();
        StartCoroutine(Dissolve());
    }

    

    public void GiveExpReward()
    {
        if (mExpGiven || mExpRewardController == null) return;
        PlayerLevelController playerLevelController = FindPlayerLevelController();

        int expAmount = mExpRewardController.GetExpReward(mEnemyType);
        
        playerLevelController.GainExperience(expAmount);
        mExpGiven = true;
    }

    private PlayerLevelController FindPlayerLevelController()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.GetComponent<PlayerLevelController>();
        }

        return null;
    }


    #endregion
    
    #endregion

    #region Attack 이벤트
    public void OnAttackAnimationExit()
    {
        mbIsAttacking = false;
        ClearAllBools();
        if (mBreathVFXInstance != null)
        {
            Destroy(mBreathVFXInstance);
            mBreathVFXInstance = null;
        }   
        if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
        {
            if (DetectPlayer())
            {
                mAnim.SetBool("Trace", true);
                mAgent.isStopped = false;
                mAgent.SetDestination(mTarget.position);
            }
            else
            {
                mAnim.SetBool("Patrol", true);
                mAgent.isStopped = false;
            }
        }
    }

    public void FireProjectile()
    {
        if (mAttackBehaviorAsset is RangedAttackBehavior ranged)
        {
            ranged.FireLastPosition(transform, mTarget);
        }
        else if (mAttackBehaviorAsset is DragonAttackBehavior dragon) dragon.FireLastPosition(transform);
    }

    public void OnMeleeAttack()
    {
        if (!(mAttackBehaviorAsset is MeleeAttackBehavior melee))
            return;
        AudioManager.Instance.PlayPoolSfx(ExSfxType.MeleeAttack);
        // 플레이어 태그만 필터링
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            melee.Range,
            mPlayerMask        
        );
        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
            {
                col.GetComponent<PlayerController>()
                    .SetHit(melee.Damage, transform, 0);
            }
        }
    }

    #region 드래곤 공격
    public void OnBreathIndicator()
    {
        mbIgnoreHits = true;
        if (!(mAttackBehaviorAsset is DragonAttackBehavior dragon)) return;
        if (dragon.BreathProjectorPrefab == null) return;
        AudioManager.Instance.PlayPoolSfx(ExSfxType.DragonBreathStart);
        var go = Instantiate(dragon.BreathProjectorPrefab, mFirePoint);
        go.transform.localPosition = Vector3.up * 0.1f;
        currentProjector = go.GetComponent<Projector>();
        currentProjector.orthographicSize = 0f;

        StartCoroutine(ChargeBreathIndicator(dragon));
    }

    private IEnumerator ChargeBreathIndicator(DragonAttackBehavior dragon)
    {
        float duration = 1.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (currentProjector == null) yield break;
            currentProjector.orthographicSize = Mathf.Lerp(0f, dragon.BreathRange, elapsed / duration);
            yield return null;
        }
        if (currentProjector != null) currentProjector.orthographicSize = dragon.BreathRange;
    }

    public void OnTailAttack()
    {
        var dragon = mAttackBehavior as DragonAttackBehavior;
        AudioManager.Instance.PlayPoolSfx(ExSfxType.DragonTail);
        var hits = Physics.OverlapSphere(transform.position, dragon.TailRange, dragon.HitLayer);
        foreach (var col in hits)
            if (col.CompareTag("Player"))
                col.GetComponent<PlayerController>().SetHit(dragon.TailDamage, transform, 1);
    }
    public void OnBreathLand()
    {
        mbIgnoreHits = false;
        mbIsBreathing = true; 
        if (currentProjector) Destroy(currentProjector.gameObject);
        currentProjector = null;
        AudioManager.Instance.PlayPoolSfx(ExSfxType.DragonBreath);
        if (mAttackBehaviorAsset is DragonAttackBehavior dragon && dragon.BreathVFXPrefab != null)
        {
            mBreathVFXInstance = Instantiate(
                dragon.BreathVFXPrefab,
                mBreathPoint.position,
                mBreathPoint.rotation
            );

            if (mBreathVFXInstance.TryGetComponent<Projectile>(out var proj))
                proj.InitializeBreath(
                    dragon.BreathHitLayer,
                    dragon.BreathDamage
                );
            StartCoroutine(WaitBreathFinish(mBreathVFXInstance));
        }
        else
        {
            mbIsBreathing = false;
        }
    }
    private IEnumerator WaitBreathFinish(GameObject vfx)
    {
        yield return new WaitForSeconds(3.0f); 
        mbIsBreathing = false;
        if (mBreathVFXInstance)
        {
            Destroy(mBreathVFXInstance);
            mBreathVFXInstance = null;
        }
    }
    #endregion
    
    #region 골렘 공격
    private IEnumerator ScaleUpProjector()
    {
        if (!(mAttackBehaviorAsset is GolemAttackBehavior golem)) yield break;
        float range = golem.ImpactRange;
        float duration = golem.ImpactChargeTime;
        System.Action onComplete = OnImpactLand;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentProjector.orthographicSize = Mathf.Lerp(0f, range, elapsed / duration);
            yield return null;
        }
        currentProjector.orthographicSize = range;
        onComplete?.Invoke();
        Destroy(currentProjector.gameObject);
    }
    public void OnImpactIndicator()
    {
        mImpactHandled = false;
        mbIgnoreHits = true;
        if (ImpactProjectorPrefab == null || !(mAttackBehaviorAsset is GolemAttackBehavior) || mTarget == null) return;
        var go = Instantiate(ImpactProjectorPrefab);
        currentProjector = go.GetComponent<Projector>();
        go.transform.position = mTarget.position + Vector3.up * 5f;
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        StartCoroutine(ScaleUpProjector());
    }

    public void OnImpactLand()
    {
        if (mImpactHandled) return;
        mImpactHandled = true;
        mbIgnoreHits = false;
        if (mAttackBehaviorAsset is GolemAttackBehavior golem
            && golem.mImpactVFXPrefab != null
            && currentProjector != null)
        {
            var spawnPos = transform.position;
            var vfx = Instantiate(golem.mImpactVFXPrefab, spawnPos, golem.mImpactVFXPrefab.transform.rotation);
            AudioManager.Instance.PlayPoolSfx(ExSfxType.GolemImpact);
            Destroy(vfx, golem.mImpactVFXDuration);
        }

        if (mAttackBehaviorAsset is GolemAttackBehavior g)
        {
            Vector3 center = currentProjector != null ? currentProjector.transform.position: transform.position;
            Collider[] hits = Physics.OverlapSphere(center, g.ImpactRange, ImpactHitLayer);
            foreach (var col in hits)
            {
                if (!col.CompareTag("Player")) continue;
                col.GetComponent<PlayerController>()
                    .SetHit(g.ImpactDamage, transform, 2);
            }
        }    
    }

    public void OnSwingAttack()
    {
        if (!(mAttackBehaviorAsset is GolemAttackBehavior golem)) return;
        AudioManager.Instance.PlayPoolSfx(ExSfxType.GolemSwing);
        var hits = Physics.OverlapSphere(transform.position, golem.SwingRange, ImpactHitLayer);
        foreach (var col in hits)
            if (col.CompareTag("Player"))
            {
                col.GetComponent<PlayerController>().SetHit(golem.SwingDamage, transform, 1);
                Debug.Log("스윙 데미지 적용");
            }
    }
    #endregion
    
    #endregion
    
    #region 디버그 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mDetectRadius);
        if (mAttackBehaviorAsset is MeleeAttackBehavior melee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, melee.Range);
        }
        else if (mAttackBehaviorAsset is RangedAttackBehavior ranged)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, ranged.Range);
        }
        else if (mAttackBehaviorAsset is GolemAttackBehavior golem)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, golem.SwingRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, golem.ImpactRange);
        }
    }
    #endregion
}