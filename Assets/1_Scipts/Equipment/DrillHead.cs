using System.Collections.Generic;
using UnityEngine;
using static Interfaces;

public class DrillHead : MonoBehaviour
{
    // 현재 드릴 머리에 닿아 있는 광석들
    public List<IMineable> touchingRocks = new List<IMineable>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("IronStone"))
        {
            var rock = other.GetComponent<IMineable>();
            if (rock != null && !touchingRocks.Contains(rock))
            {
                touchingRocks.Add(rock);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("IronStone"))
        {
            var rock = other.GetComponent<IMineable>();
            if (rock != null)
            {
                touchingRocks.Remove(rock);
            }
        }
    }

    // 파괴된 광석은 리스트에서 제거하기 위한 정리 함수
    public void CleanUpList()
    {
        touchingRocks.RemoveAll(rock => rock == null || !rock.IsActive);
    }
}