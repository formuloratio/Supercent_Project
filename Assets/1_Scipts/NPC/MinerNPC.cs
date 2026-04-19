using UnityEngine;
using UnityEngine.AI;
using static GameEnums;
using static Interfaces;

public class MinerNPC : MonoBehaviour
{
    [Header("Mining Settings")]
    public GameObject orePrefab;             // 생성할 광석 프리팹
    public MachineStackHandler targetMachineInput; // 광석이 날아갈 기계 (UpgradeZone에서 할당)
    public int mineDamage = 34;

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
        if (anim == null || isMining) return;

        if (targetRock == null || !targetRock.IsActive)
        {
            FindNearestRock();
        }
        else
        {
            float dist = Vector3.Distance(transform.position, ((Component)targetRock).transform.position);
            if (dist <= agent.stoppingDistance + 0.5f)
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
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(closestRock.transform.position);
                anim.SetBool("isMoving", true);
            }
        }
    }

    private void StartMining()
    {
        isMining = true;
        agent.isStopped = true;
        anim.SetBool("isMoving", false);
        anim.SetTrigger("doMining");

        if (targetRock != null)
            transform.LookAt(((Component)targetRock).transform.position);
    }

    // --- 애니메이션 이벤트: 채굴 타격 시 호출 ---
    public void OnMiningImpact()
    {
        if (targetRock != null && targetRock.IsActive)
        {
            // [사운드 추가] 타격 대상인 바위의 AudioSource를 가져와서 재생
            AudioSource rockSource = ((Component)targetRock).GetComponent<AudioSource>();
            if (rockSource != null)
            {
                // 광부 NPC는 곡괭이 소리(miningClip)를 냅니다.
                AudioManager.Instance.Play3DSFX(rockSource, AudioManager.Instance.miningClip, AudioManager.Instance.miningVolume);
            }

            // 타격 후 파괴 여부 확인
            bool isBroken = targetRock.TakeDamage(mineDamage, ((Component)targetRock).transform.position);

            if (isBroken)
            {
                if (orePrefab != null && targetMachineInput != null)
                {
                    GameObject oreObj = ObjectPool.Instance.Pop(orePrefab, ((Component)targetRock).transform.position, Quaternion.identity);
                    ResourceItem item = oreObj.GetComponent<ResourceItem>();

                    if (item != null)
                    {
                        item.JumpTo(targetMachineInput.transform, Vector3.zero, 0.5f, () =>
                        {
                            targetMachineInput.AddToStack(item, 100);
                        });
                    }
                }
            }
        }
    }

    public void FinishMining()
    {
        isMining = false;
        if (agent.isOnNavMesh) agent.isStopped = false;
    }
}