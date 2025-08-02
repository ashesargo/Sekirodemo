using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShinobiEX : MonoBehaviour
{
    GameObject bossStatus;
    EnemyTest boss;
    public GameObject targetObject;


    // Start is called before the first frame update
    void Start()
    {
        bossStatus = GameObject.Find("Boss");
        if (bossStatus != null)
        {
            boss = bossStatus.GetComponent<EnemyTest>();
        }
        else
        {
            Debug.LogWarning("ShinobiEX: 找不到名為 'Boss' 的遊戲對象");
        }

        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ShinobiEX: targetObject 未設置");
        }

        if (boss == null)
        {
            Debug.LogWarning("ShinobiEX: 無法獲取 EnemyTest 組件");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bossStatus == null)
        {
            bossStatus = GameObject.Find("Boss");
        }
        if (boss == null)
        {
            if (bossStatus != null)
            {
                boss = bossStatus.GetComponent<EnemyTest>();
            }
        }

        if (boss != null)
        {
            if (boss.isDead)
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(true);
                }
            }
        }
    }
}
