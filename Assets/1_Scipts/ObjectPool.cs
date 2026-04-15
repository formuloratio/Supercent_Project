using System;
using UnityEngine;
using System.Collections.Generic;

// 이벤트를 관리하는 정적 클래스
public static class GameEvents
{
    public static Action OnWorkerMined; // 인부가 채굴했을 때 발생
}

// 오브젝트 풀링 (싱글톤)
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    private void Awake() { Instance = this; }

    public GameObject Pop(GameObject prefab)
    {
        string key = prefab.name;
        if (!poolDict.ContainsKey(key)) poolDict[key] = new Queue<GameObject>();

        if (poolDict[key].Count > 0)
        {
            GameObject obj = poolDict[key].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab);
    }

    public void Push(GameObject obj)
    {
        string key = obj.name.Replace("(Clone)", "");
        obj.SetActive(false);
        if (!poolDict.ContainsKey(key)) poolDict[key] = new Queue<GameObject>();
        poolDict[key].Enqueue(obj);
    }
}