using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "ScriptableObjects/EquipmentData")]
public class EquipmentDataSO : ScriptableObject
{
    public string itemName;
    public GameObject visualPrefab; // 장비 모델 프리팹
    public int targetPivotIndex;    // 장착될 피벗 번호
    public bool isDrillType;        // 드릴 애니메이션 여부

    [Header("Economy")]
    public int price = 20; // 장비의 구매 가격

    [Header("Stats")]
    public int mineDamage = 100;
    public int maxCapacity = 10;
}