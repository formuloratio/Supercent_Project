using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class BookingDesk : MonoBehaviour
{
    [Header("Stacks")]
    public MachineStackHandler inputStack;
    public MachineStackHandler outputStack;
    public GameObject cashPrefab;

    [Header("Queue System")]
    public Transform queueStartPoint;
    public Transform[] queuePoints;
    public Transform jailEntrancePoint;
    public Transform spawnPoint;

    [Header("Settings")]
    public GameObject npcPrefab;
    public float spawnInterval = 3f;
    public CriminalDataSO criminalData;

    [Header("Door Object")]
    public GameObject jailDoor;
    private int travelingPrisonersCount = 0;

    private List<CriminalNPC> waitingQueue = new List<CriminalNPC>();
    private CriminalNPC currentProcessingNPC;

    private bool isProcessing = false;
    private float spawnTimer;
    private float lastCollectTime;
    private bool isTransferring = false;

    private AudioSource deskAudioSource;

    private void Awake()
    {
        deskAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        HandleSpawning();
        ProcessQueue();
        CheckAndProcessCriminal();
    }

    private void HandleSpawning()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && waitingQueue.Count < queuePoints.Length)
        {
            spawnTimer = 0f;
            SpawnNPC();
        }
    }

    private void SpawnNPC()
    {
        if (spawnPoint == null || npcPrefab == null) return;
        GameObject npcObj = ObjectPool.Instance.Pop(npcPrefab, spawnPoint.position, spawnPoint.rotation);
        CriminalNPC npc = npcObj.GetComponent<CriminalNPC>();
        waitingQueue.Add(npc);
    }

    private void ProcessQueue()
    {
        if (currentProcessingNPC == null && waitingQueue.Count > 0)
        {
            currentProcessingNPC = waitingQueue[0];
            waitingQueue.RemoveAt(0);
            currentProcessingNPC.MoveToTarget(queueStartPoint);
        }

        for (int i = 0; i < waitingQueue.Count; i++)
        {
            waitingQueue[i].MoveToTarget(queuePoints[i]);
        }
    }

    private void CheckAndProcessCriminal()
    {
        if (isProcessing || currentProcessingNPC == null || criminalData == null) return;
        if (Vector3.Distance(currentProcessingNPC.transform.position, queueStartPoint.position) > 0.5f) return;

        if (outputStack.Count < 100 && inputStack.Count >= criminalData.requiredHandcuffs)
        {
            StartCoroutine(ConvertRoutine());
        }
    }

    private IEnumerator ConvertRoutine()
    {
        isProcessing = true;

        int required = criminalData.requiredHandcuffs;
        for (int i = 0; i < required; i++)
        {
            AudioManager.Instance.Play3DSFX(deskAudioSource, AudioManager.Instance.convertClip);
            ResourceItem handcuff = inputStack.RemoveFromStack();
            if (handcuff != null)
            {
                handcuff.JumpTo(currentProcessingNPC.transform, Vector3.up * 1.5f, 0.3f, () =>
                {
                    ObjectPool.Instance.Push(handcuff.gameObject);
                });
                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return new WaitForSeconds(0.3f);
        currentProcessingNPC.ChangeToPrisoner();

        int reward = criminalData.rewardCash;
        for (int i = 0; i < reward; i++)
        {
            GameObject cashObj = ObjectPool.Instance.Pop(cashPrefab, currentProcessingNPC.transform.position + Vector3.up, Quaternion.identity);
            ResourceItem cashItem = cashObj.GetComponent<ResourceItem>();
            outputStack.AddToStack(cashItem, 100);
            yield return new WaitForSeconds(0.05f);
        }

        // [이동 로직]
        travelingPrisonersCount++;
        if (jailDoor != null) jailDoor.SetActive(false);

        // --- 핵심: 가로막힘과 상관없이 출발하는 순간 즉시 카운트 ---
        GameManager.Instance.AddPrisoner();

        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        CriminalNPC npcToJail = currentProcessingNPC;

        StartCoroutine(npcToJail.GoToJail(jailEntrancePoint.position + randomOffset, () =>
        {
            // 도착 시 문 닫기 체크만 수행 (카운트는 이미 올라감)
            travelingPrisonersCount--;
            if (travelingPrisonersCount <= 0)
            {
                travelingPrisonersCount = 0;
                if (jailDoor != null) jailDoor.SetActive(true);
            }
        }));

        yield return new WaitForSeconds(0.5f);
        currentProcessingNPC = null;
        isProcessing = false;
    }

    public void Deposit(PlayerInteraction player)
    {
        if (isTransferring) return;
        StartCoroutine(DepositRoutine(player));
    }

    private IEnumerator DepositRoutine(PlayerInteraction player)
    {
        isTransferring = true;
        while (player.playerStack.HasResourceType(ResourceType.Handcuff))
        {
            ResourceItem item = player.playerStack.RemoveSpecificType(ResourceType.Handcuff);
            if (item != null)
            {
                inputStack.AddToStack(item, 100);
                yield return new WaitForSeconds(0.05f);
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
                    int cashValue = 5;
                    if (player.playerStack.CanAdd(ResourceType.Cash))
                    {
                        player.playerStack.AddToStack(item, 0);
                        GameManager.Instance.AddMoney(cashValue);
                        player.PlayCashPickupSound();
                    }
                    else
                    {
                        item.JumpTo(player.transform, Vector3.up * 2f, 0.2f, () => {
                            GameManager.Instance.AddMoney(cashValue);
                            player.PlayCashPickupSound();
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