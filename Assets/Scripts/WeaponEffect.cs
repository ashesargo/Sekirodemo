using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 武器特效
public class WeaponEffect : MonoBehaviour
{
    public Transform weaponTip; // 武器尖端 Transform
    public LayerMask environmentLayer; // 判定 Layer
    public GameObject sparkPrefab; // 火花 Prefab
    private float nextSparkTime; // 火花間隔時間

    void Update()
    {
        if (Time.time > nextSparkTime)
        {
            CheckWeaponHit();
        }
    }

    // 檢查武器碰撞
    void CheckWeaponHit()
    {
        Vector3 origin = weaponTip.position;
        Vector3 direction = weaponTip.forward;

        if (Physics.SphereCast(origin, 0.1f, direction, out RaycastHit hit, 0.5f, environmentLayer))
        {
            SpawnSpark(hit.point, hit.normal);
            nextSparkTime = Time.time + 0.1f; // 每0.1秒觸發一次
        }
    }

    // 生成火花
    void SpawnSpark(Vector3 position, Vector3 normal)
    {
        GameObject spark = Instantiate(sparkPrefab, position, Quaternion.LookRotation(normal));
        Destroy(spark, 1f);
    }
}
