using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackStateAnimation : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<EnemyBTController>().OnAttackAnimationExit();
    }
}
