using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyStartup : MonoBehaviour
{
    void OnEnable()
    {
        StartCoroutine(StartAfterDelay());
    }

    IEnumerator StartAfterDelay()
    {
        // 等待隨機延遲時間，避免同時啟動造成卡頓
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        // 啟用 Animator（如果有的話）
        if (TryGetComponent<Animator>(out var anim))
        {
            anim.enabled = true;
        }

        // 啟用 AI 腳本（你可以換成你自己的 AI 類別名稱）
        //if (TryGetComponent<EnemyAI>(out var ai))
        //{
        //    ai.enabled = true;
        //}
    }
}
