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

    // 取得物件池內的物件並設置位置（適用於有CharacterController的物件）
    public GameObject GetPooledObject(Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetPooledObject();
        
        // 檢查是否有EnemyAI組件，如果有則使用SetSpawnPosition
        EnemyAI enemyAI = obj.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.SetSpawnPosition(position, rotation);
        }
        else
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        
        // 檢查是否為Boss，如果是則確保血條UI正確設置
        HealthPostureController healthController = obj.GetComponent<HealthPostureController>();
        if (healthController != null && healthController.IsBoss())
        {
            Debug.Log($"[ObjectPool] 生成 Boss: {obj.name}");
            
            // 檢查血條UI引用
            if (healthController.healthPostureUI == null)
            {
                Debug.LogWarning($"[ObjectPool] Boss {obj.name} 的 healthPostureUI 為 null，嘗試修復");
                
                // 嘗試在Boss物件下找到血條UI
                HealthPostureUI[] healthUIs = obj.GetComponentsInChildren<HealthPostureUI>(true);
                if (healthUIs.Length > 0)
                {
                    healthController.healthPostureUI = healthUIs[0];
                    Debug.Log($"[ObjectPool] 已修復 Boss 血條UI引用: {healthUIs[0].gameObject.name}");
                }
                else
                {
                    Debug.LogError($"[ObjectPool] 無法在 Boss {obj.name} 下找到血條UI");
                }
            }
            
            // 確保Boss的血條UI是隱藏的（等待玩家進入範圍後才顯示）
            if (healthController.healthPostureUI != null)
            {
                healthController.healthPostureUI.gameObject.SetActive(false);
                Debug.Log($"[ObjectPool] Boss 血條UI已隱藏，等待玩家進入範圍: {healthController.healthPostureUI.gameObject.name}");
            }
        }
        
        return obj;
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