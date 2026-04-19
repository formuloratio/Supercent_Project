using UnityEngine;
using UnityEngine.AI;
using static Interfaces;

public class MinerNPC : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private IMineable targetRock;
    private bool isMining = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isMining) return;

        if (targetRock == null || !targetRock.IsActive)
        {
            FindNearestRock();
        }
        else
        {
            float dist = Vector3.Distance(transform.position, ((Component)targetRock).transform.position);
            if (dist <= agent.stoppingDistance)
            {
                StartMining();
            }
        }
    }

    private void FindNearestRock()
    {
        GameObject[] rocks = GameObject.FindGameObjectsWithTag("IronStone");
        float closestDist = Mathf.Infinity;
        GameObject closestRock = null;

        foreach (var rockObj in rocks)
        {
            var mineable = rockObj.GetComponent<IMineable>();
            if (mineable != null && mineable.IsActive)
            {
                float dist = Vector3.Distance(transform.position, rockObj.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestRock = rockObj;
                }
            }
        }

        if (closestRock != null)
        {
            targetRock = closestRock.GetComponent<IMineable>();
            agent.SetDestination(closestRock.transform.position);
            anim.SetBool("isMoving", true);
        }
    }

    private void StartMining()
    {
        isMining = true;
        agent.isStopped = true;
        anim.SetBool("isMoving", false);
        anim.SetTrigger("doMining");
    }

    // 플레이어와 같은 애니메이션 이벤트 호출
    public void OnMiningImpact()
    {
        if (targetRock != null && targetRock.IsActive)
        {
            // 작업자는 데미지만 입히고 아이템은 생성하지 않거나, 별도 처리
            targetRock.TakeDamage(10, ((Component)targetRock).transform.position);
        }
    }

    public void FinishMining()
    {
        isMining = false;
        agent.isStopped = false;
    }
}