using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AraSamples;

public class SpawnPoint : MonoBehaviour
{
    // 玩家是否在椅子範圍內
    private bool isInRange = false;
    // 玩家是否已經坐下
    private bool isSitting = false;
    // 玩家的 Transform
    public Transform playerTransform;
    public Animator playerAnimator;
    // 特效 Prefab
    public GameObject effect;
    public GameObject text;
    public GameObject text2;
    // 檢測範圍（可調整）
    public float interactionRange = 2.0f;

    bool firstSit = true;

    void Update()
    {
        // 使用 OverlapSphere 檢測玩家是否在範圍內
        isInRange = CheckPlayerInRange();
        // 檢查玩家是否在範圍內並按下 D 鍵
        if (text != null)
        {
            if (isInRange && !isSitting)
                text.SetActive(true);
            else if (text.activeSelf)
                text.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isSitting && isInRange)
            {
                SitDown();
            }
            else
            {
                StandUp();
            }
        }
    }

    // 使用 OverlapSphere 檢查玩家是否在範圍內
    private bool CheckPlayerInRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    // 坐下邏輯
    private void SitDown()
    {
        isSitting = true;
        // 讓玩家面向營火（僅在 X-Z 平面）
        Vector3 buddhaPos = transform.position;
        Vector3 playerPos = playerTransform.position;
        Vector3 direction = new Vector3(buddhaPos.x, playerPos.y, buddhaPos.z) - playerPos;
        if (direction != Vector3.zero)
        {
            playerTransform.rotation = Quaternion.LookRotation(direction);
        }

        playerAnimator.SetBool("Sit", isSitting);
        effect.SetActive(true);
        if (firstSit == true)
        {
            text2.SetActive(true);
        }
        firstSit = false;
    }

    // 站起邏輯
    private void StandUp()
    {
        isSitting = false;
        playerAnimator.SetBool("Sit", isSitting);
        text2.SetActive(false);
    }

    // 可視化檢測範圍（僅在編輯器中顯示）
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}