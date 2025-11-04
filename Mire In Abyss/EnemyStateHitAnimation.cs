using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateHitAnimation : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<EnemyBTController>().OnHitAnimationExit();
    }
}
