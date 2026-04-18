using System.Collections;
using UnityEngine;
using System;
using static GameEnums;

public class ResourceItem : MonoBehaviour
{
    public ResourceType type;
    private Coroutine moveCoroutine; // 현재 실행 중인 코루틴 저장

    public void JumpTo(Transform target, Vector3 targetLocalPos, float duration, Action onComplete = null)
    {
        // [추가] 시작하자마자 부모를 설정하여 좌표계를 고정시킴
        transform.SetParent(target);

        // 이전 움직임이 있다면 정지 (중첩 방지)
        StopAllCoroutines();
        StartCoroutine(JumpRoutine(targetLocalPos, duration, onComplete));
    }

    private IEnumerator JumpRoutine(Vector3 targetLocalPos, float duration, Action onComplete)
    {
        Vector3 startPos = transform.localPosition; // 부모가 설정되었으므로 localPosition 사용
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // 부모(target) 기준의 local 좌표로 부드럽게 이동
            Vector3 lerpPos = Vector3.Lerp(startPos, targetLocalPos, t);

            // 포물선 효과 (Sin 함수)
            lerpPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;

            transform.localPosition = lerpPos;

            // 회전도 Local 회전으로 부드럽게
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, t);

            yield return null;
        }

        transform.localPosition = targetLocalPos;
        transform.localRotation = Quaternion.identity;
        onComplete?.Invoke();
    }
}