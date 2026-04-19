using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;
using static Interfaces;

public class PlayerInteraction : MonoBehaviour
{
    public PlayerStackHandler playerStack;
    public GameObject orePrefab;

    [Header("UI Feedback")]
    public GameObject maxTextPrefab;

    public EquipmentDataSO currentEqData;
    private IMineable currentTargetRock;
    private bool isMining = false;

    private AudioSource playerAudioSource;
    private readonly int doMiningHash = Animator.StringToHash("doMining");
    private readonly int doDrillingHash = Animator.StringToHash("doDrilling");

    private void Awake()
    {
        playerAudioSource = GetComponent<AudioSource>();
    }

    public void UpdateEquipmentData(EquipmentDataSO data)
    {
        currentEqData = data;
        if (playerStack != null)
        {
            playerStack.maxIronOre = data.maxCapacity;
            playerStack.maxCash = data.maxCapacity;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("IronStone"))
        {
            currentTargetRock = other.GetComponent<IMineable>();
            if (currentTargetRock != null && currentTargetRock.IsActive && !isMining)
            {
                PerformMining();
            }
        }

        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            interactable.Interact(this);
        }
    }

    private void PerformMining()
    {
        isMining = true;
        Animator anim = GetComponent<Animator>();

        if (currentEqData != null && currentEqData.isDrillType)
            anim.SetTrigger(doDrillingHash);
        else
            anim.SetTrigger(doMiningHash);
    }

    public void OnMiningImpact()
    {
        if (currentEqData != null && currentEqData.isDrillType)
        {
            DrillHead[] allDrillHeads = GetComponentsInChildren<DrillHead>();
            if (allDrillHeads.Length > 0)
            {
                HashSet<IMineable> uniqueRocks = new HashSet<IMineable>();
                foreach (var head in allDrillHeads)
                {
                    head.CleanUpList();
                    foreach (var rock in head.touchingRocks) uniqueRocks.Add(rock);
                }
                foreach (var rock in uniqueRocks) ProcessMining(rock);
                return;
            }
        }

        if (currentTargetRock != null && currentTargetRock.IsActive)
        {
            ProcessMining(currentTargetRock);
        }
    }

    private void ProcessMining(IMineable target)
    {
        if (target == null || !target.IsActive) return;

        bool canAdd = playerStack.CanAdd(ResourceType.IronOre);

        // [핵심 수정] 가방이 꽉 찼을 때 처리
        if (!canAdd)
        {
            // 1. 가득 찼다면 어떤 장비든 상관없이 MAX 피드백을 계속 띄움
            ShowMaxFeedback(((Component)target).transform.position + Vector3.up * 2f);

            // 2. 일반 곡괭이(isDrillType이 아님)인 경우에만 여기서 로직 중단(채굴 안 됨)
            // 드릴이나 불도저(isDrillType)라면 return하지 않고 아래의 사운드/데미지 로직으로 넘어감
            if (currentEqData != null && !currentEqData.isDrillType)
            {
                return;
            }
        }

        // --- 이중 사운드 재생 로직 (가득 차도 드릴/불도저는 실행됨) ---
        AudioSource rockSource = ((Component)target).GetComponent<AudioSource>();

        if (currentEqData.isDrillType)
        {
            AudioManager.Instance.Play3DSFX(rockSource, AudioManager.Instance.drillClip, AudioManager.Instance.drillVolume);
            AudioManager.Instance.Play3DSFX(rockSource, AudioManager.Instance.miningClip, AudioManager.Instance.miningVolume * 0.8f);
        }
        else
        {
            AudioManager.Instance.Play3DSFX(rockSource, AudioManager.Instance.miningClip, AudioManager.Instance.miningVolume);
        }

        // 데미지 처리
        bool isDestroyed = target.TakeDamage(currentEqData.mineDamage, ((Component)target).transform.position);

        if (isDestroyed)
        {
            // 가방에 공간이 있을 때만 아이템을 생성하여 습득
            if (canAdd)
            {
                GameObject oreObj = ObjectPool.Instance.Pop(orePrefab, ((Component)target).transform.position, Quaternion.identity);
                ResourceItem item = oreObj.GetComponent<ResourceItem>();
                if (item != null) playerStack.AddToStack(item, currentEqData.maxCapacity);
            }
            // 가득 찬 상태에서 부서지면 아이템 없이 광석만 파괴됨 (불도저/드릴 공통)
        }
    }

    public void PlayResourceMoveSound()
    {
        if (playerAudioSource != null)
            AudioManager.Instance.Play3DSFX(playerAudioSource, AudioManager.Instance.resourceMoveClip, AudioManager.Instance.defaultVolume);
    }

    public void PlayCashPickupSound()
    {
        if (AudioManager.Instance.CanPlayCashSound())
            AudioManager.Instance.Play3DSFX(playerAudioSource, AudioManager.Instance.cashPickupClip, AudioManager.Instance.cashPickupVolume);
    }

    private void ShowMaxFeedback(Vector3 position)
    {
        if (maxTextPrefab != null) ObjectPool.Instance.Pop(maxTextPrefab, position, Quaternion.identity);
    }

    public void FinishMining()
    {
        isMining = false;
    }
}