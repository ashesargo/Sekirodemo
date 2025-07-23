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
        // �����H������ɶ��A�קK�P�ɱҰʳy���d�y
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        // �ҥ� Animator�]�p�G�����ܡ^
        if (TryGetComponent<Animator>(out var anim))
        {
            anim.enabled = true;
        }

        // �ҥ� AI �}���]�A�i�H�����A�ۤv�� AI ���O�W�١^
        //if (TryGetComponent<EnemyAI>(out var ai))
        //{
        //    ai.enabled = true;
        //}
    }
}
