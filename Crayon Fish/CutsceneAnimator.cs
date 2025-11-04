using System;
using UnityEngine;

public class CutsceneAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;   // CutScene 루트의 Animator
    Action _onFinished;                   // 애니메이션 종료 콜백

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        gameObject.SetActive(false);      // 시작 시 비활성화
    }

    /// <summary>컷씬 재생. 완료 후 onFinished 호출</summary>
    public void Play(Action onFinished)
    {
        _onFinished = onFinished;
        gameObject.SetActive(true);
        animator.ResetTrigger("Play");    // 중복 방지
        animator.SetTrigger("Play");
    }

    /// <summary>Animation Event 에서 호출</summary>
    void OnCutsceneFinished()
    {
        gameObject.SetActive(false);
        _onFinished?.Invoke();
        _onFinished = null;
    }
}