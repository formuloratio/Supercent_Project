using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CriminalNPC : MonoBehaviour
{
    public CriminalDataSO currentData;

    [Header("Visuals")]
    public Transform visualPivot;
    private GameObject currentVisual;

    private NavMeshAgent agent;
    private Animator currentAnimator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(CriminalDataSO data)
    {
        currentData = data;
        ChangeVisual(data.civilianVisualPrefab);
    }

    // 외형 변경 (일반인 -> 죄수)
    public void ChangeToPrisoner()
    {
        ChangeVisual(currentData.prisonerVisualPrefab);
        // 갈아입을 때 약간의 파티클 효과를 넣어주면 훨씬 좋습니다.
    }

    private void ChangeVisual(GameObject visualPrefab)
    {
        if (currentVisual != null) Destroy(currentVisual);

        currentVisual = Instantiate(visualPrefab, visualPivot);
        currentAnimator = currentVisual.GetComponentInChildren<Animator>();
    }

    // 특정 목적지로 이동
    public void MoveToTarget(Vector3 targetPosition)
    {
        agent.isStopped = false;
        agent.SetDestination(targetPosition);
    }

    private void Update()
    {
        // 이동 중일 때 애니메이션 처리
        if (currentAnimator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            currentAnimator.SetBool("isMoving", isMoving);
        }
    }

    // 감옥으로 이동 후 처리
    public void GoToJail(Vector3 jailPosition)
    {
        StartCoroutine(JailRoutine(jailPosition));
    }

    private IEnumerator JailRoutine(Vector3 target)
    {
        MoveToTarget(target);

        // 목적지에 도착할 때까지 대기
        while (Vector3.Distance(transform.position, target) > 0.5f)
        {
            yield return null;
        }

        // 감옥 도착 시 오브젝트 풀로 반환 (혹은 감옥 안에서 대기)
        ObjectPool.Instance.Push(gameObject);
    }
}