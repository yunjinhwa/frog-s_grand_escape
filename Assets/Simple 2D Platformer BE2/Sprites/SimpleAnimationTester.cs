using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SimpleAnimationTester : MonoBehaviour
{
    [Header("State Names")]
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string runStateName = "Run";
    [SerializeField] private string jumpStateName = "Jump";

    [Header("Jump Settings")]
    [SerializeField] private float jumpReturnDelay = 0.35f;

    private Animator animator;
    private Coroutine jumpCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayState(idleStateName);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            PlayState(runStateName);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            PlayJumpThenReturn();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayJumpThenReturn();
        }
    }

    private void PlayState(string stateName)
    {
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
            jumpCoroutine = null;
        }

        animator.Play(stateName, 0, 0f);
    }

    private void PlayJumpThenReturn()
    {
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
        }

        jumpCoroutine = StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        animator.Play(jumpStateName, 0, 0f);
        yield return new WaitForSeconds(jumpReturnDelay);
        animator.Play(idleStateName, 0, 0f);
        jumpCoroutine = null;
    }
}