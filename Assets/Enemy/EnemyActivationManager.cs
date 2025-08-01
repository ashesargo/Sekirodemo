using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActivationManager : MonoBehaviour
{
    [Header("�D��")]
    public Transform player;

    [Header("�ĤH�]�w")]
    public string enemyTag = "Enemy";
    public float activationDistance = 30f;
    public float checkInterval = 1f;

    [Header("������J�]�w")]
    public int maxEnemiesPerBatch = 5;
    public float batchDelay = 0.05f;

    private GameObject[] enemies;

    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        StartCoroutine(CheckEnemiesLoop());
    }

    IEnumerator CheckEnemiesLoop()
    {
        WaitForSeconds waitBetweenChecks = new WaitForSeconds(checkInterval);

        while (true)
        {
            int enemiesProcessed = 0;

            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) continue;

                float distance = Vector3.Distance(player.position, enemy.transform.position);
                bool shouldBeActive = distance <= activationDistance;

                if (enemy.activeSelf != shouldBeActive)
                    enemy.SetActive(shouldBeActive);

                enemiesProcessed++;

                if (enemiesProcessed >= maxEnemiesPerBatch)
                {
                    enemiesProcessed = 0;
                    yield return new WaitForSeconds(batchDelay);
                }
            }

            yield return waitBetweenChecks;
        }
    }
}
