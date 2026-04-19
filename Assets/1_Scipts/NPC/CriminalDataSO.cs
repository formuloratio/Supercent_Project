using UnityEngine;

[CreateAssetMenu(fileName = "CriminalData", menuName = "ScriptableObjects/CriminalData")]
public class CriminalDataSO : ScriptableObject
{
    [Header("Requirements & Rewards")]
    public int requiredHandcuffs = 2; // 요구 수갑 개수
    public int rewardCash = 2;        // 보상으로 나올 돈의 개수
}