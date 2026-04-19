using TMPro;
using UnityEngine;
using UnityEngine.AI;
using System;
using static GameEnums;
using static Interfaces;

public class UpgradeZone : MonoBehaviour, IInteractable
{
    public enum UpgradeType { Equipment, MinerNPC, TransporterNPC }
    public Action<UpgradeZone> OnZoneCompleted;

    [Header("Upgrade Settings")]
    public UpgradeType upgradeType;

    [Header("Data (SO)")]
    public EquipmentDataSO equipmentData;
    public NPCDataSO npcData;

    [Header("NPC Target Stacks")]
    public MachineStackHandler sourceMachineStack;
    public MachineStackHandler targetDeskStack;
    public MachineStackHandler minerTargetStack;

    [Header("Progress")]
    public int currentCash = 0;
    private int requiredCash = -1;
    private float lastInteractTime;
    private float interactCooldown = 0.05f;

    [Header("UI Reference")]
    public TextMeshProUGUI priceText;

    private AudioSource zoneAudioSource; // [추가] 발판 자체 사운드용

    private void Start()
    {
        zoneAudioSource = GetComponent<AudioSource>(); // [추가]
        InitializeUpgradeData();
        if (priceText == null) priceText = GetComponentInChildren<TextMeshProUGUI>();
        UpdatePriceText();
    }

    private void InitializeUpgradeData()
    {
        if (upgradeType == UpgradeType.Equipment)
        {
            if (equipmentData != null) requiredCash = equipmentData.price;
        }
        else
        {
            if (npcData != null) requiredCash = npcData.price;
        }
    }

    public void Interact(PlayerInteraction player)
    {
        if (requiredCash <= 0 || currentCash >= requiredCash) return;
        if (Time.time - lastInteractTime < interactCooldown) return;

        lastInteractTime = Time.time;

        if (player.playerStack.HasResourceType(ResourceType.Cash))
        {
            ResourceItem cashItem = player.playerStack.RemoveSpecificType(ResourceType.Cash);
            if (cashItem != null)
            {
                // [추가] 자원 이동 사운드 (플레이어 위치)
                player.PlayResourceMoveSound();
                ConsumePhysicalResource(cashItem, player);
                return;
            }
        }

        if (GameManager.Instance.SpendMoney(5))
        {
            currentCash += 5;
            UpdatePriceText();
            CheckUpgradeComplete(player);
        }
    }

    private void ConsumePhysicalResource(ResourceItem item, PlayerInteraction player)
    {
        item.JumpTo(transform, Vector3.zero, 0.2f, () =>
        {
            GameManager.Instance.SpendMoney(5);
            currentCash += 5;
            UpdatePriceText();
            ObjectPool.Instance.Push(item.gameObject);
            CheckUpgradeComplete(player);
        });
    }

    private void UpdatePriceText()
    {
        if (priceText != null)
        {
            int remaining = requiredCash - currentCash;
            priceText.text = remaining > 0 ? remaining.ToString() : "0";
        }
    }

    private void CheckUpgradeComplete(PlayerInteraction player)
    {
        if (currentCash >= requiredCash)
        {
            // [수정] 구매 성공 사운드 (발판 위치)
            AudioManager.Instance.Play3DSFX(zoneAudioSource, AudioManager.Instance.purchaseClip);

            OnZoneCompleted?.Invoke(this);

            switch (upgradeType)
            {
                case UpgradeType.Equipment:
                    if (equipmentData != null) player.GetComponent<PlayerController>().ChangeEquipment(equipmentData);
                    break;
                case UpgradeType.MinerNPC:
                    SpawnMiners();
                    break;
                case UpgradeType.TransporterNPC:
                    SpawnTransporters();
                    break;
            }
            gameObject.SetActive(false);
        }
    }

    private void SpawnMiners()
    {
        if (npcData == null || npcData.npcPrefab == null) return;

        for (int i = 0; i < npcData.spawnCount; i++)
        {
            Vector3 spawnPos = transform.position + transform.forward * 2f + new Vector3(i * 0.7f, 0, 0);
            GameObject npcObj = Instantiate(npcData.npcPrefab, spawnPos, Quaternion.identity);

            MinerNPC miner = npcObj.GetComponent<MinerNPC>();
            if (miner != null) miner.targetMachineInput = minerTargetStack;

            NavMeshAgent agent = npcObj.GetComponent<NavMeshAgent>();
            if (agent != null) agent.Warp(spawnPos);
        }
    }

    private void SpawnTransporters()
    {
        if (npcData == null || npcData.npcPrefab == null) return;

        for (int i = 0; i < npcData.spawnCount; i++)
        {
            Vector3 spawnPos = transform.position + transform.forward * 2f + new Vector3(i * 0.7f, 0, 0);
            GameObject npcObj = Instantiate(npcData.npcPrefab, spawnPos, Quaternion.identity);

            TransporterNPC transporter = npcObj.GetComponent<TransporterNPC>();
            if (transporter != null)
            {
                transporter.sourceStack = sourceMachineStack;
                transporter.targetStack = targetDeskStack;
            }

            NavMeshAgent agent = npcObj.GetComponent<NavMeshAgent>();
            if (agent != null) agent.Warp(spawnPos);
        }
    }
}