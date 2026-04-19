using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System; // Action 사용을 위해 추가

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

        if (npcRenderer != null)
        {
            defaultMaterial = npcRenderer.sharedMaterial;
        }
    }

    private void OnEnable()
    {
        if (npcRenderer != null && defaultMaterial != null)
        {
            npcRenderer.material = defaultMaterial;
        }
    }

    public void ChangeToPrisoner()
    {
        if (npcRenderer != null && prisonerMaterial != null)
        {
            npcRenderer.material = prisonerMaterial;
        }
    }

    public void MoveToTarget(Transform targetTransform)
    {
        StopAllCoroutines();
        StartCoroutine(MoveAndSnapRoutine(targetTransform));
    }

    private IEnumerator MoveAndSnapRoutine(Transform target)
    {
        agent.isStopped = false;
        agent.SetDestination(target.position);

        while (agent.pathPending || agent.remainingDistance > 0.1f)
        {
            yield return null;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    public IEnumerator GoToJail(Vector3 jailPosition, Action onArrived)
    {
        agent.isStopped = false;
        agent.SetDestination(jailPosition);

        while (agent.pathPending || agent.remainingDistance > 0.1f)
        {
            yield return null;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // 도착 알림 실행
        onArrived?.Invoke();
    }
}