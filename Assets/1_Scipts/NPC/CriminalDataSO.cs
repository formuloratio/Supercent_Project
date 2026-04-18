using UnityEngine;

[CreateAssetMenu(fileName = "CriminalData", menuName = "ScriptableObjects/CriminalData")]
public class CriminalDataSO : ScriptableObject
{
    [Header("Criminal Info")]
    public string criminalName = "일반 도둑";

    [Header("Requirements & Rewards")]
    public int requiredHandcuffs = 2; // 이 사람을 잡기 위해 필요한 수갑 개수
    public int rewardCash = 2;        // 죄수가 되었을 때 뱉어내는 돈(Cash)의 개수

    [Header("Visuals")]
    public GameObject civilianVisualPrefab; // 일반인 모습 프리팹
    public GameObject prisonerVisualPrefab; // 죄수복 모습 프리팹
}