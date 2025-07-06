using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 基礎物件池腳本
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    public GameObject prefab; // 物件池內的物件
    public Transform poolParent; // 物件池的父物件位置
    public int poolSize = 10; // 物件池的大小
    private List<GameObject> pooledObjects = new List<GameObject>(); // 物件池的物件列表
    public float recycleTime = 10f; // 物件自動回收時間
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 初始化物件池
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, poolParent);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    // 取得物件池內的物件
    public GameObject GetPooledObject()
    {
        foreach (GameObject poolObj in pooledObjects)
        {
            // 如果物件池內的物件是未啟用，則啟用該物件
            if (!poolObj.activeInHierarchy)
            {
                poolObj.SetActive(true);
                StartCoroutine(RecycleObject(poolObj)); // 呼叫 RecycleObject 自動回收物件
                return poolObj;
            }
        }
        // 如果物件池內的物件都已啟用，則創建新物件
        GameObject newObj = Instantiate(prefab, poolParent);
        newObj.SetActive(true);
        StartCoroutine(RecycleObject(newObj)); // 呼叫 RecycleObject 自動回收物件
        pooledObjects.Add(newObj);
        return newObj;
    }

    // 將物件回收至物件池
    public void ReturnPooledObject(GameObject obj)
    {
        // 將物件設為未啟用
        obj.SetActive(false);
        // 將物件放回物件池
        pooledObjects.Add(obj);
    }

    // 自動回收物件
    private IEnumerator RecycleObject(GameObject obj)
    {
        yield return new WaitForSeconds(recycleTime);
        ReturnPooledObject(obj); // 呼叫 ReturnPooledObject 將物件回收至物件池
    }
}