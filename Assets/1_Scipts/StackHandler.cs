using System.Collections.Generic;
using UnityEngine;

public class StackHandler : MonoBehaviour
{
    [SerializeField] private Transform stackPivot;
    [SerializeField] private float stepHeight = 0.2f;
    [SerializeField] private GameObject maxUI; // MAX 글자 오브젝트

    private List<GameObject> stackList = new List<GameObject>();
    public int Count => stackList.Count;

    public void AddStack(GameObject prefab, int capacity)
    {
        if (stackList.Count >= capacity)
        {
            if (maxUI != null) maxUI.SetActive(true);
            return;
        }

        GameObject obj = ObjectPool.Instance.Pop(prefab);
        obj.transform.SetParent(stackPivot);
        obj.transform.localPosition = new Vector3(0, stackList.Count * stepHeight, 0);
        obj.transform.localRotation = Quaternion.identity;
        stackList.Add(obj);
    }

    public GameObject RemoveStack()
    {
        if (stackList.Count == 0) return null;
        if (maxUI != null) maxUI.SetActive(false);

        GameObject obj = stackList[stackList.Count - 1];
        stackList.RemoveAt(stackList.Count - 1);
        return obj;
    }
}