using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 基礎物件池控制器腳本
public class PoolController : MonoBehaviour
{
    public ObjectPool objectPool;
    public float spawnTime = 1f; // 生成物件時間間隔

    private void Start()
    {
        StartCoroutine(spawnObjects());
    }

    private IEnumerator spawnObjects()
    {
        // 檢查物件池是否存在
        if (objectPool == null)
        {
            Debug.LogWarning("PoolController: objectPool 為 null，停止生成");
            yield break;
        }
        
        // 無限循環生成物件
        while (true)
        {
            GameObject obj = objectPool.GetPooledObject();
            // 設置物件位置!!!!!
            obj.transform.position = Vector3.zero;

            yield return new WaitForSeconds(spawnTime);
        }
    }
} 