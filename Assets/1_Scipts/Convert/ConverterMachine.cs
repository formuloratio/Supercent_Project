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
    public float collectInterval = 0.05f; // 수거 간격

    // [추가] 기계 자체 사운드 재생을 위한 컴포넌트
    private AudioSource machineAudioSource;

    private void Awake()
    {
        // 기계 위치에서 소리를 내기 위해 AudioSource 참조
        machineAudioSource = GetComponent<AudioSource>();
    }

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

        // [사운드] 자원이 기계 안에서 변환될 때 재생 (기계 위치)
        if (machineAudioSource != null)
        {
            AudioManager.Instance.Play3DSFX(machineAudioSource, AudioManager.Instance.convertClip);
        }

        // 모든 자원을 오브젝트 풀로 반납합니다.
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

        while (player.playerStack.HasResourceType(inputType))
        {
            ResourceItem item = player.playerStack.RemoveSpecificType(inputType);

            if (item != null)
            {
                // [사운드] 자원이 플레이어 등에서 빠져나갈 때 재생 (플레이어 위치)
                player.PlayResourceMoveSound();

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
                // [사운드] 자원이 플레이어에게 들어올 때 재생 (플레이어 위치)
                player.PlayResourceMoveSound();

                if (item.type == ResourceType.Cash)
                {
                    if (player.playerStack.CanAdd(ResourceType.Cash))
                    {
                        player.playerStack.AddToStack(item, 0);
                    }
                    else
                    {
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
                    outputStack.AddToStack(item, 100);
                }
            }
        }
    }
}