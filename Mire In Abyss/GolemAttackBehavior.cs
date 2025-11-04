using System;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Attack Behaviors/Golem")]
public class GolemAttackBehavior : ScriptableObject, IAttackBehavior
{
    [Header("스윙 공격")]
    public float SwingRange = 3f;
    public int  SwingDamage = 20;

    [Header("임팩트 공격")]
    public float ImpactRange  = 6f;
    public int ImpactDamage = 35;
    [SerializeField] private float mImpactChargeTime = 1.5f;
    [SerializeField] private float mImpactCooldown   = 10f;
    
    [Header("임팩트 VFX")]
    public GameObject mImpactVFXPrefab;
    public float mImpactVFXDuration = 2f;
    
    private float mLastImpactTime = -Mathf.Infinity;

    public float ImpactChargeTime => mImpactChargeTime;
    public float ImpactCooldown   => mImpactCooldown;


    private void OnEnable()
    {
        mLastImpactTime = Time.time;
    }

    public bool IsInRange(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        return dist <= SwingRange || dist <= ImpactRange;
    }

    public bool CanImpact(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        float nextImpactTime = mLastImpactTime + mImpactCooldown;
        float remaining = nextImpactTime - Time.time;
        
        bool can = Time.time >= nextImpactTime && dist <= ImpactRange;
        return can;
    }

    public bool CanSwing(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        return dist <= SwingRange;
    }

    public void Attack(Transform self, Transform target)
    {
        if (target == null) return;
        Animator anim = self.GetComponent<Animator>();

        if (CanImpact(self, target))
        {
            mLastImpactTime = Time.time;
            anim.SetTrigger("AttackImpact");
        }
        else if (CanSwing(self, target))
        {
            anim.SetTrigger("AttackSwing");
        }
    }
}
