using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackBehavior", menuName = "AI/Attack Behaviors/Melee")]
public class MeleeAttackBehavior : ScriptableObject, IAttackBehavior
{
    [Header("근접 공격 설정")]
    public float Range = 2f;   
    public int Damage = 10;  

    public bool IsInRange(Transform self, Transform target)
        => Vector3.Distance(self.position, target.position) <= Range;

    public void Attack(Transform self, Transform target)
    {
        var anim = self.GetComponent<Animator>();
        anim.SetTrigger("Attack");
    }
}