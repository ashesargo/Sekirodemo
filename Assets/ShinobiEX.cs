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
        bossStatus = GameObject.Find("NewBoss1");
        boss = bossStatus.GetComponent<EnemyTest>();
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
        if (bossStatus == null) Debug.Log("bossStatus");
        if (boss == null) Debug.Log("boss");

    }

    // Update is called once per frame
    void Update()
    {
        if ( boss.isDead )
        {
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
            
        }
    }
}
