using System.Collections;
using UnityEngine;
using static GameEnums;

public class ConverterMachine : MonoBehaviour
{
    [Header("Settings")]
    public ResourceType inputType;
    public ResourceType outputType;
    public GameObject outputPrefab;

    [Header("Stacks")]
    public MachineStackHandler inputStack;  // In_Zone의 바닥 스택 연결
    public MachineStackHandler outputStack; // Out_Zone의 바닥 스택 연결

    [Header("Balance")]
    public float convertInterval = 1.0f;
    public float transferInterval = 0.05f;

    private float lastProcessTime;
    private bool isTransferring = false;

    private float lastCollectTime;
    public float collectInterval = 0.05f; // 수거 간격 (초당 최대 20개)

    // 기계 전체에서 Update는 여기서 '딱 한 번'만 실행됨
    private void Update()
    {
        if (inputStack.Count > 0 && outputStack.Count < 100)
        {
            if (Time.time - lastProcessTime >= convertInterval)
            {
                ProcessItem();
                lastProcessTime = Time.time;
            }
        }
    }

    private void ProcessItem()
    {
        ResourceItem consumedItem = inputStack.RemoveFromStack();
        if (consumedItem == null) return;

        //모든 자원을 오브젝트 풀로 반납합니다.
        ObjectPool.Instance.Push(consumedItem.gameObject);

        // 결과물 생성
        GameObject resultObj = ObjectPool.Instance.Pop(outputPrefab, outputStack.stackPivot.position, Quaternion.identity);
        ResourceItem resultItem = resultObj.GetComponent<ResourceItem>();

        if (resultItem != null)
            outputStack.AddToStack(resultItem, 100);
    }

    // --- 자식 구역들에서 호출할 수 있게 public으로 열어둔 함수들 ---

    public void Deposit(PlayerInteraction player)
    {
        if (isTransferring) return;
        StartCoroutine(DepositRoutine(player));
    }

    private IEnumerator DepositRoutine(PlayerInteraction player)
    {
        isTransferring = true;

        // [문제 1 해결] 가방 안에 기계가 원하는 inputType이 하나라도 있는 동안 계속 실행
        while (player.playerStack.HasResourceType(inputType))
        {
            // 특정 타입만 골라서 가져옴 (다른 자원이 가로막고 있어도 상관없음)
            ResourceItem item = player.playerStack.RemoveSpecificType(inputType);

            if (item != null)
            {
                inputStack.AddToStack(item, 100);
                yield return new WaitForSeconds(transferInterval);
            }
            else break;
        }
        isTransferring = false;
    }

    public void Collect(PlayerInteraction player)
    {
        if (Time.time - lastCollectTime < 0.05f) return;

        if (outputStack.Count > 0)
        {
            lastCollectTime = Time.time;
            ResourceItem item = outputStack.RemoveFromStack();

            if (item != null)
            {
                if (item.type == ResourceType.Cash)
                {
                    if (player.playerStack.CanAdd(ResourceType.Cash))
                    {
                        // 등에 쌓음 (이때 PlayerStackHandler 내부에서 UI를 업데이트함)
                        player.playerStack.AddToStack(item, 0);
                    }
                    else
                    {
                        // 가방이 꽉 찼을 때: 등에 쌓지는 않지만 돈은 벌림
                        item.JumpTo(player.transform, Vector3.up * 2f, 0.2f, () => {
                            GameManager.Instance.AddMoney(10);
                            ObjectPool.Instance.Push(item.gameObject);
                        });
                    }
                }
                else if (player.playerStack.CanAdd(item.type))
                {
                    player.playerStack.AddToStack(item, 0);
                }
                else
                {
                    // 다른 자원은 자리 없으면 다시 기계로
                    outputStack.AddToStack(item, 100);
                }
            }
        }
    }
}