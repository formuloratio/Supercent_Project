using UnityEngine;
using System.Collections;

public class MaxPopupFeedback : MonoBehaviour
{
    public float destroyDelay = 0.5f; // 사라질 시간 (0.5초)
    public float moveSpeed = 2f;      // 위로 떠오르는 속도 (선택 사항)

    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        // 프리팹이 활성화되자마자 0.5초 뒤에 사라지는 코루틴 시작
        StopAllCoroutines();
        StartCoroutine(AutoDisableRoutine());
    }

    private void Update()
    {
        // 0.5초 동안 가만히 있는 것보다 위로 살짝 떠오르는 연출을 넣으면 더 좋습니다.
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }

    private void LateUpdate() // 이동 후 마지막에 회전을 맞춰줍니다.
    {
        if (mainCam != null)
        {
            // 카메라를 정면으로 바라보게 함
            transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward,
                             mainCam.transform.rotation * Vector3.up);
        }
    }

    private IEnumerator AutoDisableRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);

        // 0.5초가 지나면 다시 오브젝트 풀로 반납
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.Push(gameObject);
        }
        else
        {
            // 혹시 풀이 없으면 파괴 (예외 처리)
            gameObject.SetActive(false);
        }
    }
}