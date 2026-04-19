using System.Net;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "ScriptableObjects/NPCData")]
public class NPCDataSO : ScriptableObject
{
    public string npcName;       // NPC 이름
    public int price;            // 구매 가격
    public GameObject npcPrefab; // 생성할 NPC 프리팹
    public int spawnCount = 3;   // 한 번에 생성될 마릿수
}