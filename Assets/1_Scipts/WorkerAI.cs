using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WorkerAI : MonoBehaviour
{
    public float mineSpeed = 1f;
    public NavMeshAgent agent;
    private IMineable targetOre;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(WorkerRoutine());
    }

    IEnumerator WorkerRoutine()
    {
        while (true)
        {
            targetOre = FindNearestOre();
            if (targetOre != null)
            {
                agent.SetDestination(((Component)targetOre).transform.position);

                // 도착할 때까지 대기
                while (agent.pathPending || agent.remainingDistance > 0.5f) yield return null;

                targetOre.TakeDamage(1, () => {
                    GameEvents.OnWorkerMined?.Invoke();
                });
            }
            yield return new WaitForSeconds(mineSpeed);
        }
    }

    private IMineable FindNearestOre()
    {
        // "IronOre" 태그를 가진 오브젝트 중 활성화된 것을 찾음
        GameObject[] ores = GameObject.FindGameObjectsWithTag("IronOre");
        float closestDist = Mathf.Infinity;
        IMineable closestOre = null;

        foreach (var obj in ores)
        {
            if (obj.TryGetComponent<IMineable>(out var mineable) && mineable.IsActive)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestOre = mineable;
                }
            }
        }
        return closestOre;
    }
}