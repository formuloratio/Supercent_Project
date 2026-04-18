using UnityEngine;
using static GameEnums;

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Game/Equipment Data")]
public class EquipmentDataSO : ScriptableObject
{
    public EquipmentType type;
    public GameObject visualPrefab;
    public int maxCapacity;
    public float mineCooldown;
    public int mineDamage;
    public float moveSpeed;
    public RuntimeAnimatorController animatorOverride; // 장비에 따른 애니메이션 변경용
}