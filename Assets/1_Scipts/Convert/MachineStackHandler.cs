using System.Collections.Generic;
using UnityEngine;

public class MachineStackHandler : MonoBehaviour
{
    public Transform stackPivot;
    public int columnCount = 2;
    [SerializeField] private float stepHeight = 0.25f;
    [SerializeField] private float rowSpacing = 0.45f;

    private List<ResourceItem> items = new List<ResourceItem>();
    public int Count => items.Count;

    public void AddToStack(ResourceItem item, int capacity)
    {
        int col = items.Count % columnCount;
        int level = items.Count / columnCount;
        float xOffset = (columnCount - 1) * rowSpacing * 0.5f;
        Vector3 targetPos = new Vector3((col * rowSpacing) - xOffset, level * stepHeight, 0);

        items.Add(item);
        item.JumpTo(stackPivot, targetPos, 0.3f);
    }

    public ResourceItem RemoveFromStack()
    {
        if (items.Count == 0) return null;
        ResourceItem item = items[items.Count - 1];
        items.RemoveAt(items.Count - 1);
        return item;
    }
}