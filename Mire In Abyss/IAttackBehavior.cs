using UnityEngine;

public interface IAttackBehavior
{
    bool IsInRange(Transform self, Transform target);
    void Attack(Transform self, Transform target);
}