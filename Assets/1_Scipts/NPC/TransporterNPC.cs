using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static GameEnums;

public class TransporterNPC : MonoBehaviour
{
    [Header("Target Stacks")]
    public MachineStackHandler sourceStack; // 기계의 Output (UpgradeZone에서 할당)
    public MachineStackHandler targetStack; // 데스크의 Input (UpgradeZone에서 할당)

    private NavMeshAgent agent;
    private Animator anim;
    private PlayerStackHandler myStack;

    private bool isWorking = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        myStack = GetComponent<PlayerStackHandler>();

        // 컴포넌트 누락 방지 체크
        if (myStack == null)
        {
            Debug.LogError($"{gameObject.name}: PlayerStackHandler 컴포넌트가 없습니다! 프리팹을 확인하세요.");
        }
    }

    private void Update()
    {
        // 핵심 컴포넌트나 목표 스택이 없으면 실행하지 않음
        if (isWorking || anim == null || myStack == null || sourceStack == null || targetStack == null)
            return;

        // 1. 가방이 비어있으면 기계(Source)로 이동
        if (myStack.GetCurrentCountByType(ResourceType.Handcuff) == 0)
        {
            if (sourceStack.Count > 0)
            {
                MoveToTarget(sourceStack.transform.position, () => StartCoroutine(CollectRoutine()));
            }
            else
            {
                anim.SetBool("isMoving", false);
            }
        }
        // 2. 가방에 물건이 있으면 데스크(Target)로 이동
        else
        {
            MoveToTarget(targetStack.transform.position, () => StartCoroutine(DeliverRoutine()));
        }
    }

    private void MoveToTarget(Vector3 pos, System.Action onArrival)
    {
        if (!agent.isOnNavMesh) return;

        agent.SetDestination(pos);
        anim.SetBool("isMoving", true);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            anim.SetBool("isMoving", false);
            onArrival?.Invoke();
        }
    }

    private IEnumerator CollectRoutine()
    {
        isWorking = true;
        agent.isStopped = true;

        while (sourceStack != null && sourceStack.Count > 0 && myStack.CanAdd(ResourceType.Handcuff))
        {
            var item = sourceStack.RemoveFromStack();
            if (item != null)
            {
                myStack.AddToStack(item, 0);
                yield return new WaitForSeconds(0.1f);
            }
            else break;
        }

        agent.isStopped = false;
        isWorking = false;
    }

    private IEnumerator DeliverRoutine()
    {
        isWorking = true;
        agent.isStopped = true;

        while (myStack.HasResourceType(ResourceType.Handcuff))
        {
            if (targetStack != null && targetStack.Count < 100)
            {
                var item = myStack.RemoveSpecificType(ResourceType.Handcuff);
                if (item != null)
                {
                    targetStack.AddToStack(item, 100);
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
            }
        }

        agent.isStopped = false;
        isWorking = false;
    }
}