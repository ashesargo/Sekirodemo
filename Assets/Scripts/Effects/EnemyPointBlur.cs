using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵人徑向模糊後處理
/// </summary>
public class EnemyPointBlur : MonoBehaviour
{
    [Header("Enemy Point Blur Shader")]
    public Shader PointBlurShader; // 徑向模糊 Shader
    public AnimationCurve curve; // 模糊強度動畫曲線
    public GameObject EnemySpark; // 敵人火花特效 Prefab（用於格擋成功）
    public GameObject EnemyGuardSpark; // 敵人防禦火花特效 Prefab（用於防禦）
    public GameObject EnemyHitSpark; // 敵人受傷火花特效 Prefab（用於受傷）
    
    [Header("Enemy Blur Effect")]
    [Range(0, 1)]
    public float BlurStrength = 0.5f; // 敵人徑向模糊強度（比主角少50%）
    public float ParryBlurStrength = 0.25f; // 敵人格擋成功時的模糊強度（比主角少50%）
    public float GuardBlurStrength = 0.15f; // 敵人防禦時的模糊強度（比主角少50%）
    public float HitBlurStrength = 0.2f; // 敵人受傷時的模糊強度（比主角少50%）
    public float BlurSpeed = 1; // 模糊動畫速度
    public float BlurRange = 0.3f; // 模糊範圍
    public float BlurRadius = 1; // 模糊圓圈半徑
    public int downSampleFactor = 2; // 降採樣因子，提高性能

    [Header("Enemy Settings")]
    public EnemyAI enemyAI; // 引用敵人AI
    public Collider enemyWeaponCollider; // 敵人武器 Collider
    public LayerMask detectionLayer = -1; // 碰撞檢測層級

    private Material material; // 後處理材質
    private Vector2 BlurCenter = new Vector2(0.5f, 0.5f); // 徑向模糊螢幕座標中心點
    private Texture2D gradTexture; // 梯度貼圖
    private float t = 1000; // 時間計數器

    void Start()
    {
        InitializeGradientTexture();
        InitializeMaterial();
        
        // 訂閱敵人事件
        if (enemyAI != null)
        {
            enemyAI.OnEnemyParrySuccess += HandleEnemyParrySuccess;
            enemyAI.OnEnemyGuardSuccess += HandleEnemyGuardSuccess;
            enemyAI.OnEnemyHitOccurred += HandleEnemyHitOccurred;
            enemyAI.OnEnemyAttackSuccess += HandleEnemyAttackSuccess;
        }
        
        Debug.Log("[EnemyPointBlur] 初始化完成，enemyAI: " + (enemyAI != null ? "已設置" : "未設置"));
    }
    
    void OnDestroy()
    {
        // 取消訂閱事件，避免內存洩漏
        if (enemyAI != null)
        {
            enemyAI.OnEnemyParrySuccess -= HandleEnemyParrySuccess;
            enemyAI.OnEnemyGuardSuccess -= HandleEnemyGuardSuccess;
            enemyAI.OnEnemyHitOccurred -= HandleEnemyHitOccurred;
            enemyAI.OnEnemyAttackSuccess -= HandleEnemyAttackSuccess;
        }
    }

    void Update()
    {
        UpdateMaterialParameters();
        
        // 如果 enemyAI 為 null，嘗試重新連接
        if (enemyAI == null)
        {
            enemyAI = FindObjectOfType<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.OnEnemyParrySuccess += HandleEnemyParrySuccess;
                enemyAI.OnEnemyGuardSuccess += HandleEnemyGuardSuccess;
                enemyAI.OnEnemyHitOccurred += HandleEnemyHitOccurred;
                enemyAI.OnEnemyAttackSuccess += HandleEnemyAttackSuccess;
            }
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderBlurEffect(source, destination);
    }

    // 初始化梯度貼圖
    private void InitializeGradientTexture()
    {
        // 創建梯度貼圖用於模糊衰減
        gradTexture = new Texture2D(2048, 1, TextureFormat.Alpha8, false);
        gradTexture.wrapMode = TextureWrapMode.Clamp;
        gradTexture.filterMode = FilterMode.Bilinear;
        
        // 將動畫曲線應用於貼圖
        for (var i = 0; i < gradTexture.width; i++)
        {
            var x = 1.0f / gradTexture.width * i; // 計算座標
            var a = curve.Evaluate(x); // 從動畫曲線獲取值
            gradTexture.SetPixel(i, 0, new Color(a, a, a, a)); // 設置像素
        }
        gradTexture.Apply(); // 應用貼圖
    }

    // 初始化後處理材質
    private void InitializeMaterial()
    {
        // 初始化後處理材質
        material = new Material(PointBlurShader);
        material.hideFlags = HideFlags.DontSave; // 不在場景中保存材質
        material.SetTexture("_GradTex", gradTexture); // 設置梯度貼圖
    }

    // 更新後處理材質參數
    private void UpdateMaterialParameters()
    {
        // 更新材質參數
        material.SetFloat("_Timer", t += Time.deltaTime); // 更新時間
        material.SetFloat("_BlurSpeed", BlurSpeed); // 設置模糊速度
        material.SetFloat("_BlurStrength", BlurStrength); // 設置模糊強度
        material.SetFloat("_BlurRange", BlurRange); // 設置模糊範圍
        material.SetVector("_BlurCenter", new Vector4(BlurCenter.x* Camera.main.aspect, BlurCenter.y, 0, 0)); // 設置模糊中心（考慮螢幕比例）
        material.SetFloat("_Aspect", Camera.main.aspect); // 設置螢幕比例
        material.SetFloat("_BlurCircleRadius", BlurRadius); // 設置模糊圓圈半徑
    }

    // 處理敵人 Parry 成功事件
    private void HandleEnemyParrySuccess(Vector3 parryPosition)
    {
        Debug.Log("[EnemyPointBlur] 收到敵人 Parry 成功事件，位置: " + parryPosition);
        TriggerEnemyParryEffectAtPosition(parryPosition);
    }
    
    // 處理敵人防禦成功事件
    private void HandleEnemyGuardSuccess(Vector3 guardPosition)
    {
        Debug.Log("[EnemyPointBlur] 收到敵人防禦成功事件，位置: " + guardPosition);
        TriggerEnemyGuardEffectAtPosition(guardPosition);
    }

    // 處理敵人受傷事件
    private void HandleEnemyHitOccurred(Vector3 hitPosition)
    {
        Debug.Log("[EnemyPointBlur] 收到敵人受傷事件，位置: " + hitPosition);
        TriggerEnemyHitEffectAtPosition(hitPosition);
    }
    
    // 處理敵人攻擊成功事件
    private void HandleEnemyAttackSuccess(Vector3 attackPosition)
    {
        Debug.Log("[EnemyPointBlur] 收到敵人攻擊成功事件，位置: " + attackPosition);
        TriggerEnemyAttackEffectAtPosition(attackPosition);
    }

    // 在指定位置觸發敵人 Parry 特效
    private void TriggerEnemyParryEffectAtPosition(Vector3 position)
    {
        Debug.Log("[EnemyPointBlur] 在指定位置觸發敵人 Parry 特效: " + position);
        t = 0; // 重置時間計數器
        
        // 設置敵人格擋成功的模糊強度
        BlurStrength = ParryBlurStrength;
        
        // 使用敵人格擋成功的火花特效
        if (EnemySpark != null)
        {
            GameObject effect = Instantiate(EnemySpark, position, Quaternion.identity);
            Debug.Log("[EnemyPointBlur] 生成敵人 Parry 特效: " + effect.name);
        }
        else
        {
            Debug.LogWarning("[EnemyPointBlur] EnemySpark 特效為 null");
        }
        
        // 將世界座標轉換為螢幕UV座標
        BlurCenter = Camera.main.WorldToScreenPoint(position); // 世界座標轉螢幕座標
        BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height); // 螢幕座標轉UV座標
    }

    // 在指定位置觸發敵人防禦特效
    private void TriggerEnemyGuardEffectAtPosition(Vector3 position)
    {
        Debug.Log("[EnemyPointBlur] 在指定位置觸發敵人防禦特效: " + position);
        t = 0; // 重置時間計數器
        
        // 設置敵人防禦的模糊強度
        BlurStrength = GuardBlurStrength;
        
        // 計算敵人武器碰撞最近點
        Vector3? collisionPoint = CalculateEnemyClosestCollisionPoint();
        
        if (collisionPoint.HasValue) // 如果找到碰撞點
        {
            Debug.Log("[EnemyPointBlur] 找到敵人防禦碰撞點: " + collisionPoint.Value);
            // 使用敵人防禦的火花特效
            if (EnemyGuardSpark != null)
            {
                Instantiate(EnemyGuardSpark, collisionPoint.Value, Quaternion.identity);
                Debug.Log("[EnemyPointBlur] 在碰撞點生成敵人防禦特效");
            }
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(collisionPoint.Value); // 世界座標轉螢幕座標
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height); // 螢幕座標轉UV座標
        }
        else // 如果沒有找到碰撞點，在敵人身前生成特效
        {
            Debug.Log("[EnemyPointBlur] 未找到敵人防禦碰撞點，在敵人身前生成特效");
            
            // 獲取敵人位置和方向
            Vector3 enemyPosition = enemyAI != null ? enemyAI.transform.position : transform.position;
            Vector3 enemyForward = enemyAI != null ? enemyAI.transform.forward : transform.forward;
            
            // 在敵人身前生成特效
            Vector3 spawnPosition = enemyPosition + enemyForward * 2f; // 距離敵人2單位
            
            if (EnemyGuardSpark != null)
            {
                GameObject effect = Instantiate(EnemyGuardSpark, spawnPosition, Quaternion.identity);
                Debug.Log("[EnemyPointBlur] 在敵人身前生成防禦特效: " + effect.name);
            }
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(spawnPosition);
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height);
        }
    }

    // 在指定位置觸發敵人攻擊特效
    private void TriggerEnemyAttackEffectAtPosition(Vector3 position)
    {
        Debug.Log("[EnemyPointBlur] 在指定位置觸發敵人攻擊特效: " + position);
        t = 0; // 重置時間計數器
        
        // 設置敵人攻擊的模糊強度（使用受傷強度）
        BlurStrength = HitBlurStrength;
        
        // 在敵人位置產生EnemyHitSpark特效
        if (EnemyHitSpark != null)
        {
            GameObject effect = Instantiate(EnemyHitSpark, position, Quaternion.identity);
            Debug.Log("[EnemyPointBlur] 在敵人位置生成攻擊特效: " + effect.name);
        }
        else
        {
            Debug.LogWarning("[EnemyPointBlur] EnemyHitSpark 特效為 null");
        }
        
        // 將世界座標轉換為螢幕UV座標
        BlurCenter = Camera.main.WorldToScreenPoint(position); // 世界座標轉螢幕座標
        BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height); // 螢幕座標轉UV座標
    }
    
    // 在指定位置觸發敵人受傷特效
    private void TriggerEnemyHitEffectAtPosition(Vector3 position)
    {
        Debug.Log("[EnemyPointBlur] 在指定位置觸發敵人受傷特效: " + position);
        t = 0; // 重置時間計數器
        
        // 設置敵人受傷的模糊強度
        BlurStrength = HitBlurStrength;
        
        // 計算敵人武器碰撞最近點
        Vector3? collisionPoint = CalculateEnemyClosestCollisionPoint();
        
        if (collisionPoint.HasValue) // 如果找到碰撞點
        {
            Debug.Log("[EnemyPointBlur] 找到敵人受傷碰撞點: " + collisionPoint.Value);
            // 使用敵人受傷的火花特效
            if (EnemyHitSpark != null)
            {
                GameObject effect = Instantiate(EnemyHitSpark, collisionPoint.Value, Quaternion.identity);
                Debug.Log("[EnemyPointBlur] 在碰撞點生成敵人受傷特效: " + effect.name);
            }
            else
            {
                Debug.LogWarning("[EnemyPointBlur] EnemyHitSpark 特效為 null");
            }
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(collisionPoint.Value); // 世界座標轉螢幕座標
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height); // 螢幕座標轉UV座標
        }
        else // 如果沒有找到碰撞點，在敵人身前生成特效
        {
            Debug.Log("[EnemyPointBlur] 未找到敵人受傷碰撞點，在敵人身前生成特效");
            
            // 獲取敵人位置和方向
            Vector3 enemyPosition = enemyAI != null ? enemyAI.transform.position : transform.position;
            Vector3 enemyForward = enemyAI != null ? enemyAI.transform.forward : transform.forward;
            
            // 在敵人身前生成特效
            Vector3 spawnPosition = enemyPosition + enemyForward * 2f; // 距離敵人2單位
            
            if (EnemyHitSpark != null)
            {
                GameObject effect = Instantiate(EnemyHitSpark, spawnPosition, Quaternion.identity);
                Debug.Log("[EnemyPointBlur] 在敵人身前生成受傷特效: " + effect.name);
            }
            else
            {
                Debug.LogWarning("[EnemyPointBlur] EnemyHitSpark 特效為 null");
            }
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(spawnPosition);
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height);
        }
    }
    
    // 渲染敵人徑向模糊效果
    private void RenderBlurEffect(RenderTexture source, RenderTexture destination)
    {
        // 創建降採樣的臨時渲染紋理
        RenderTexture rt1 = RenderTexture.GetTemporary(source.width >> downSampleFactor, source.height >> downSampleFactor, 0, source.format);
        RenderTexture rt2 = RenderTexture.GetTemporary(source.width >> downSampleFactor, source.height >> downSampleFactor, 0, source.format);
        
        Graphics.Blit(source, rt1); // 複製原始圖像到降採樣紋理
        Graphics.Blit(rt1, rt2, material, 0); // 執行模糊處理（Pass 0）
        material.SetTexture("_BlurTex", rt2); // 設置模糊紋理
        Graphics.Blit(source, destination, material, 1); // 混合原始圖像和模糊結果（Pass 1）

        // 釋放臨時紋理
        RenderTexture.ReleaseTemporary(rt1);
        RenderTexture.ReleaseTemporary(rt2);
    }

    // 計算敵人武器碰撞最近點
    private Vector3? CalculateEnemyClosestCollisionPoint()
    {
        if (enemyWeaponCollider == null) 
        {
            Debug.LogWarning("[EnemyPointBlur] enemyWeaponCollider 為 null");
            return null; // 如果沒有敵人武器碰撞器則返回null
        }
        
        // 使用敵人武器碰撞器範圍檢測所有碰撞體
        Collider[] hitColliders = Physics.OverlapBox(
            enemyWeaponCollider.bounds.center, // 碰撞器中心
            enemyWeaponCollider.bounds.extents, // 碰撞器半徑
            enemyWeaponCollider.transform.rotation, // 碰撞器旋轉
            detectionLayer // 檢測層級
        );
        
        Debug.Log("[EnemyPointBlur] 檢測到 " + hitColliders.Length + " 個碰撞體");
        
        Vector3? closestPoint = null; // 最近的碰撞點
        float closestDistance = float.MaxValue; // 最近距離
        
        foreach (Collider col in hitColliders)
        {
            // 計算 Collider 到敵人武器 Collider 中心的最遠點
            Vector3 closestPointOnCollider = col.ClosestPoint(enemyWeaponCollider.bounds.center);
            
            // 從敵人武器 Collider 中心向最遠點發射射線，獲取精確的碰撞點
            Vector3 rayDirection = (closestPointOnCollider - enemyWeaponCollider.bounds.center).normalized;
            float rayDistance = Vector3.Distance(enemyWeaponCollider.bounds.center, closestPointOnCollider) + 0.1f; // 稍微增加射線距離
            
            if (Physics.Raycast(enemyWeaponCollider.bounds.center, rayDirection, out RaycastHit hitInfo, rayDistance, detectionLayer))
            {
                // 射線成功擊中，使用射線的碰撞點
                float distance = Vector3.Distance(enemyWeaponCollider.bounds.center, hitInfo.point);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = hitInfo.point;
                    Debug.Log("[EnemyPointBlur] 射線擊中碰撞體 " + col.name + "，碰撞點: " + hitInfo.point + "，距離: " + distance);
                }
            }
            else
            {
                // 射線檢測失敗，使用 ClosestPoint 作為備用
                float distance = Vector3.Distance(enemyWeaponCollider.bounds.center, closestPointOnCollider);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = closestPointOnCollider;
                    Debug.Log("[EnemyPointBlur] 射線未擊中，使用 ClosestPoint 作為備用，碰撞體: " + col.name + "，碰撞點: " + closestPointOnCollider + "，距離: " + distance);
                }
            }
        }
        
        if (closestPoint.HasValue)
        {
            Debug.Log("[EnemyPointBlur] 找到最近碰撞點: " + closestPoint.Value + ", 距離: " + closestDistance);
        }
        else
        {
            Debug.Log("[EnemyPointBlur] 未找到碰撞點");
        }
        
        return closestPoint; // 返回最近的碰撞點
    }
} 