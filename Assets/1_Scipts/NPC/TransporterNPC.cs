using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static GameEnums;

public class TransporterNPC : MonoBehaviour
{
    public MachineStackHandler sourceStack; // 기계의 Out
    public MachineStackHandler targetStack; // 데스크의 In
    private NavMeshAgent agent;
    private PlayerStackHandler myStack; // NPC도 가방이 있어야 함
    private bool isWorking = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        myStack = GetComponent<PlayerStackHandler>();
    }

    private void Update()
    {
        if (isWorking) return;

        // 가방이 비어있으면 기계로, 가방이 차있으면 데스크로
        if (myStack.GetCurrentCountByType(ResourceType.Handcuff) == 0)
        {
            if (sourceStack.Count > 0)
            {
                MoveTo(sourceStack.transform.position, () => StartCoroutine(CollectRoutine()));
            }
        }
        else
        {
            MoveTo(targetStack.transform.position, () => StartCoroutine(DeliverRoutine()));
        }
    }

    private void MoveTo(Vector3 pos, System.Action onArrival)
    {
        agent.SetDestination(pos);
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            onArrival?.Invoke();
        }
    }

    private IEnumerator CollectRoutine()
    {
        isWorking = true;
        while (sourceStack.Count > 0 && myStack.CanAdd(ResourceType.Handcuff))
        {
            var item = sourceStack.RemoveFromStack();
            myStack.AddToStack(item, 0);
            yield return new WaitForSeconds(0.1f);
        }
        isWorking = false;
    }

    private IEnumerator DeliverRoutine()
    {
        isWorking = true;
        while (myStack.HasResourceType(ResourceType.Handcuff))
        {
            var item = myStack.RemoveSpecificType(ResourceType.Handcuff);
            targetStack.AddToStack(item, 100);
            yield return new WaitForSeconds(0.1f);
        }
        isWorking = false;
    }
}