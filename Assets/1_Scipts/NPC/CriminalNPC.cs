using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public class CriminalNPC : MonoBehaviour
{
    [Header("Visuals")]
    public Renderer npcRenderer;
    public Material prisonerMaterial;
    private Material defaultMaterial;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (npcRenderer != null) defaultMaterial = npcRenderer.sharedMaterial;
    }

    private void OnEnable()
    {
        if (npcRenderer != null && defaultMaterial != null) npcRenderer.material = defaultMaterial;
    }

    public void ChangeToPrisoner()
    {
        if (npcRenderer != null && prisonerMaterial != null) npcRenderer.material = prisonerMaterial;
    }

    public void MoveToTarget(Transform targetTransform)
    {
        StopAllCoroutines();
        StartCoroutine(MoveAndSnapRoutine(targetTransform));
    }

    private IEnumerator MoveAndSnapRoutine(Transform target)
    {
        if (!agent.isOnNavMesh) yield break;
        agent.isStopped = false;
        agent.SetDestination(target.position);

        while (agent.pathPending || agent.remainingDistance > 0.1f) yield return null;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    public IEnumerator GoToJail(Vector3 jailPosition, Action onArrived)
    {
        if (!agent.isOnNavMesh) yield break;
        agent.isStopped = false;
        agent.SetDestination(jailPosition);

        float stuckTimer = 0f;
        while (agent.pathPending || agent.remainingDistance > 0.8f) // 판정 거리 완화
        {
            stuckTimer += Time.deltaTime;

            // 8초 이상 걸리거나, 다른 죄수에게 막혀 속도가 거의 없을 때 도착으로 간주
            if (stuckTimer > 8f || (stuckTimer > 1f && agent.velocity.sqrMagnitude < 0.02f)) break;

            yield return null;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        onArrived?.Invoke(); // BookingDesk의 문 닫기 콜백 실행
    }
}