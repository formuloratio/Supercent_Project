using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject Pop(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;
        if (!poolDict.ContainsKey(key)) poolDict[key] = new Queue<GameObject>();

        GameObject obj;
        if (poolDict[key].Count > 0)
        {
            obj = poolDict[key].Dequeue();
        }
        else
        {
            obj = Instantiate(prefab);
            obj.name = prefab.name; // "(Clone)" 제거
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void Push(GameObject obj)
    {
        string key = obj.name;
        obj.SetActive(false);
        obj.transform.SetParent(transform); // 씬 정리용
        if (!poolDict.ContainsKey(key)) poolDict[key] = new Queue<GameObject>();
        poolDict[key].Enqueue(obj);
    }
}