using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipmentHandler : MonoBehaviour
{
    public List<Transform> itemPivots;
    public GameObject defaultPickaxe;
    private GameObject currentActiveItem;
    private bool isInsideMiningZone = false;

    private void Awake()
    {
        if (defaultPickaxe != null) currentActiveItem = defaultPickaxe;
    }

    // 오직 "비주얼 교체"만 책임집니다.
    public void ChangeVisual(GameObject itemPrefab, int pivotIndex)
    {
        // 1. 기존 아이템 처리
        if (currentActiveItem != null)
        {
            if (currentActiveItem == defaultPickaxe) currentActiveItem.SetActive(false);
            else Destroy(currentActiveItem);
        }

        // 2. 인덱스 검사 (에러 방지용 로그)
        if (pivotIndex < 0 || pivotIndex >= itemPivots.Count)
        {
            Debug.LogError($"[EquipmentHandler] {pivotIndex}번 피벗이 리스트에 없습니다! 리스트 크기를 확인하세요.");
            return;
        }

        // 3. 생성 및 부착
        Transform targetPivot = itemPivots[pivotIndex];
        currentActiveItem = Instantiate(itemPrefab, targetPivot);

        // 부모(피벗)의 위치와 회전에 딱 맞춤
        currentActiveItem.transform.localPosition = Vector3.zero;
        currentActiveItem.transform.localRotation = Quaternion.identity;

        // 현재 구역 상태에 따라 보이기/숨기기
        currentActiveItem.SetActive(isInsideMiningZone);

        Debug.Log($"[EquipmentHandler] {targetPivot.name} 위치에 장비 생성 완료!");
    }

    public void SetEquipmentVisibility(bool isVisible)
    {
        isInsideMiningZone = isVisible;
        if (currentActiveItem != null) currentActiveItem.SetActive(isVisible);
    }
}