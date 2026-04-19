using UnityEngine;
using System.Collections;

public class UpgradeZoneManager : MonoBehaviour
{
    [Header("Upgrade Zones")]
    public UpgradeZone drillZone;        // 드릴 업그레이드 발판
    public UpgradeZone bulldozerZone;    // 불도저 업그레이드 발판
    public UpgradeZone minerZone;        // 광부 NPC 고용 발판
    public UpgradeZone transporterZone;  // 운반 NPC 고용 발판

    private void Start()
    {
        // 1. 초기 상태: 모든 존 비활성화
        if (drillZone != null) drillZone.gameObject.SetActive(false);
        if (bulldozerZone != null) bulldozerZone.gameObject.SetActive(false);
        if (minerZone != null) minerZone.gameObject.SetActive(false);
        if (transporterZone != null) transporterZone.gameObject.SetActive(false);

        // 2. 이벤트 구독 (완료 시점 감지)
        if (drillZone != null) drillZone.OnZoneCompleted += HandleDrillCompleted;
        if (minerZone != null) minerZone.OnZoneCompleted += HandleMinerCompleted;

        // 3. 돈 획득 감지 시작
        StartCoroutine(WaitFirstMoneyRoutine());
    }

    // --- 조건 1: 플레이어가 처음으로 돈을 획득했을 때 ---
    private IEnumerator WaitFirstMoneyRoutine()
    {
        // GameManager의 Instance가 있고, 돈이 0보다 커질 때까지 대기
        while (GameManager.Instance == null || GameManager.Instance.CurrentMoney <= 0)
        {
            yield return null;
        }

        if (drillZone != null)
        {
            drillZone.gameObject.SetActive(true);
            Debug.Log("<color=yellow>매니저:</color> 첫 돈 획득! 드릴 업그레이드 존을 활성화합니다.");
        }
    }

    // --- 조건 2: 드릴 구매 완료 시 ---
    private void HandleDrillCompleted(UpgradeZone zone)
    {
        if (bulldozerZone != null) bulldozerZone.gameObject.SetActive(true);
        if (minerZone != null) minerZone.gameObject.SetActive(true);
        Debug.Log("<color=yellow>매니저:</color> 드릴 구매 완료! 불도저 및 광부 존을 활성화합니다.");
    }

    // --- 조건 3: 광부 NPC 구매 완료 시 ---
    private void HandleMinerCompleted(UpgradeZone zone)
    {
        if (transporterZone != null) transporterZone.gameObject.SetActive(true);
        Debug.Log("<color=yellow>매니저:</color> 광부 고용 완료! 운반자 존을 활성화합니다.");
    }

    private void OnDestroy()
    {
        // [수정 완료] minerSourceStack -> minerZone으로 변경
        if (drillZone != null) drillZone.OnZoneCompleted -= HandleDrillCompleted;
        if (minerZone != null) minerZone.OnZoneCompleted -= HandleMinerCompleted;
    }
}