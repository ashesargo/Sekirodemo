using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 徑向模糊後處理
/// </summary>
public class PointBlur : MonoBehaviour
{
    [Header("Point Blur Shader")]
    public Shader PointBlurShader; // 徑向模糊 Shader
    public AnimationCurve curve; // 模糊強度動畫曲線
    public GameObject Spark; // 火花特效 Prefab（用於格擋成功）
    public GameObject GuardSpark; // 防禦火花特效 Prefab（用於防禦）
    public GameObject HitSpark; // 受傷火花特效 Prefab（用於受傷）
    
    [Header("Blur Effect")]
    [Range(0, 1)]
    public float BlurStrength = 1f; // 徑向模糊強度（默認值）
    public float ParryBlurStrength = 0.5f; // 格擋成功時的模糊強度
    public float GuardBlurStrength = 0.3f; // 防禦時的模糊強度
    public float HitBlurStrength = 0.4f; // 受傷時的模糊強度
    public float BlurSpeed = 1; // 模糊動畫速度
    public float BlurRange = 0.3f; // 模糊範圍
    public float BlurRadius = 1; // 模糊圓圈半徑
    public int downSampleFactor = 2; // 降採樣因子，提高性能

    [Header("Player Settings")]
    public TPContraller tpController; // 引用玩家控制器
    public PlayerStatus playerStatus; // 引用玩家狀態
    public Collider weaponCollider; // 武器 Collider
    public LayerMask detectionLayer = -1; // 碰撞檢測層級

    private Material material; // 後處理材質
    private Vector2 BlurCenter = new Vector2(0.5f, 0.5f); // 徑向模糊螢幕座標中心點
    private Texture2D gradTexture; // 梯度貼圖
    private float t = 1000; // 時間計數器

    void Start()
    {
        InitializeGradientTexture();
        InitializeMaterial();
        
        // 初始化 lastParrySuccess 為 false
        
        // 訂閱 Parry 成功事件
        if (tpController != null)
        {
            tpController.OnParrySuccess += HandleParrySuccess;
            tpController.OnGuardSuccess += HandleGuardSuccess;
            tpController.OnHitOccurred += HandleHitOccurred;
        }
        
        Debug.Log("[PointBlur] 初始化完成，tpController: " + (tpController != null ? "已設置" : "未設置"));
    }
    
    void OnDestroy()
    {
        // 取消訂閱事件，避免內存洩漏
        if (tpController != null)
        {
            tpController.OnParrySuccess -= HandleParrySuccess;
            tpController.OnGuardSuccess -= HandleGuardSuccess;
            tpController.OnHitOccurred -= HandleHitOccurred;
        }
    }

    void Update()
    {
        UpdateMaterialParameters();
        
        // 如果 tpController 為 null，嘗試重新連接
        if (tpController == null)
        {
            tpController = FindObjectOfType<TPContraller>();
            if (tpController != null)
            {
                tpController.OnParrySuccess += HandleParrySuccess;
                tpController.OnGuardSuccess += HandleGuardSuccess;
                tpController.OnHitOccurred += HandleHitOccurred;
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

    // 處理 Parry 成功事件
    private void HandleParrySuccess(Vector3 parryPosition)
    {
        // Debug.Log("[PointBlur] 收到 Parry 成功事件，位置: " + parryPosition);
        TriggerParryEffectAtPosition(parryPosition);
    }
    
    // 處理防禦成功事件
    private void HandleGuardSuccess(Vector3 guardPosition)
    {
        // Debug.Log("[PointBlur] 收到防禦成功事件，位置: " + guardPosition);
        TriggerGuardEffectAtPosition(guardPosition);
    }

    // 處理受傷事件
    private void HandleHitOccurred(Vector3 hitPosition)
    {
        // Debug.Log("[PointBlur] 收到受傷事件，位置: " + hitPosition);
        TriggerHitEffectAtPosition(hitPosition);
    }

    // 在指定位置觸發 Parry 特效
    private void TriggerParryEffectAtPosition(Vector3 position)
    {
        // Debug.Log("[PointBlur] 在指定位置觸發 Parry 特效: " + position);
        t = 0; // 重置時間計數器
        
        // 設置格擋成功的模糊強度
        BlurStrength = ParryBlurStrength;
        
        // 使用格擋成功的火花特效
        if (Spark != null)
        {
            GameObject effect = Instantiate(Spark, position, Quaternion.identity);
            // Debug.Log("[PointBlur] 生成 Parry 特效: " + effect.name);
        }
        else
        {
            //Debug.LogWarning("[PointBlur] Spark 特效為 null");
        }
        
        // 將世界座標轉換為螢幕UV座標
        BlurCenter = Camera.main.WorldToScreenPoint(position); // 世界座標轉螢幕座標
        BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height); // 螢幕座標轉UV座標
    }

    // 在指定位置觸發防禦特效
    private void TriggerGuardEffectAtPosition(Vector3 position)
    {
        // Debug.Log("[PointBlur] 在指定位置觸發防禦特效: " + position);
        t = 0; // 重置時間計數器
        
        // 設置防禦的模糊強度
        BlurStrength = GuardBlurStrength;
        
        // 計算武器碰撞最近點
        Vector3? collisionPoint = CalculateClosestCollisionPoint();
        
        if (collisionPoint.HasValue) // 如果找到碰撞點
        {
            Debug.Log("[PointBlur] 找到防禦碰撞點: " + collisionPoint.Value);
            // 使用防禦的火花特效
            if (GuardSpark != null)
            {
                Instantiate(GuardSpark, collisionPoint.Value, Quaternion.identity);
                Debug.Log("[PointBlur] 在碰撞點生成防禦特效");
            }
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(collisionPoint.Value); // 世界座標轉螢幕座標
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height); // 螢幕座標轉UV座標
        }
        else // 如果沒有找到碰撞點，在主角前方生成特效
        {
            Debug.Log("[PointBlur] 未找到防禦碰撞點，在主角前方生成特效");
            
            // 獲取主角位置和方向
            Vector3 playerPosition = tpController != null ? tpController.transform.position : transform.position;
            Vector3 playerForward = tpController != null ? tpController.transform.forward : transform.forward;
            
            // 在主角前方生成特效
            Vector3 spawnPosition = playerPosition + playerForward * 2f; // 距離主角2單位
            
            if (GuardSpark != null)
            {
                GameObject effect = Instantiate(GuardSpark, spawnPosition, Quaternion.identity);
                Debug.Log("[PointBlur] 在主角前方生成防禦特效: " + effect.name);
            }
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(spawnPosition);
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height);
        }
    }

    // 在指定位置觸發受傷特效
    private void TriggerHitEffectAtPosition(Vector3 position)
    {
        Debug.Log("[PointBlur] 在指定位置觸發受傷特效: " + position);
        t = 0; // 重置時間計數器
        
        // 設置受傷的模糊強度
        BlurStrength = HitBlurStrength;
        
        // 計算武器碰撞最近點
        Vector3? collisionPoint = CalculateClosestCollisionPoint();
        
        if (collisionPoint.HasValue) // 如果找到碰撞點
        {
            Debug.Log("[PointBlur] 找到受傷碰撞點: " + collisionPoint.Value);
            // 使用受傷的火花特效
            if (HitSpark != null)
            {
                GameObject effect = Instantiate(HitSpark, collisionPoint.Value, Quaternion.identity);
                Debug.Log("[PointBlur] 在碰撞點生成受傷特效: " + effect.name);
            }
            else
            {
                Debug.LogWarning("[PointBlur] HitSpark 特效為 null");
            }
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(collisionPoint.Value); // 世界座標轉螢幕座標
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height); // 螢幕座標轉UV座標
        }
        else // 如果沒有找到碰撞點，在主角前方生成特效
        {
            Debug.Log("[PointBlur] 未找到受傷碰撞點，在主角前方生成特效");
            
            // 獲取主角位置和方向
            Vector3 playerPosition = tpController != null ? tpController.transform.position : transform.position;
            Vector3 playerForward = tpController != null ? tpController.transform.forward : transform.forward;
            
            // 在主角前方生成特效
            Vector3 spawnPosition = playerPosition + playerForward * 2f; // 距離主角2單位
            
            if (HitSpark != null)
            {
                GameObject effect = Instantiate(HitSpark, spawnPosition, Quaternion.identity);
                Debug.Log("[PointBlur] 在主角前方生成受傷特效: " + effect.name);
            }
            else
            {
                Debug.LogWarning("[PointBlur] HitSpark 特效為 null");
            }
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(spawnPosition);
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height);
        }
    }
    
    // 渲染徑向模糊效果
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

    // 計算武器碰撞最近點
    private Vector3? CalculateClosestCollisionPoint()
    {
        if (weaponCollider == null) 
        {
            Debug.LogWarning("[PointBlur] weaponCollider 為 null");
            return null; // 如果沒有武器碰撞器則返回null
        }
        
        // 使用武器碰撞器範圍檢測所有碰撞體
        Collider[] hitColliders = Physics.OverlapBox(
            weaponCollider.bounds.center, // 碰撞器中心
            weaponCollider.bounds.extents, // 碰撞器半徑
            weaponCollider.transform.rotation, // 碰撞器旋轉
            detectionLayer // 檢測層級
        );
        
        Debug.Log("[PointBlur] 檢測到 " + hitColliders.Length + " 個碰撞體");
        
        Vector3? closestPoint = null; // 最近的碰撞點
        float closestDistance = float.MaxValue; // 最近距離
        
        foreach (Collider col in hitColliders)
        {
            // 計算 Collider 到武器 Collider 中心的最遠點
            Vector3 closestPointOnCollider = col.ClosestPoint(weaponCollider.bounds.center);
            
            // 從武器 Collider 中心向最遠點發射射線，獲取精確的碰撞點
            Vector3 rayDirection = (closestPointOnCollider - weaponCollider.bounds.center).normalized;
            float rayDistance = Vector3.Distance(weaponCollider.bounds.center, closestPointOnCollider) + 0.1f; // 稍微增加射線距離
            
            if (Physics.Raycast(weaponCollider.bounds.center, rayDirection, out RaycastHit hitInfo, rayDistance, detectionLayer))
            {
                // 射線成功擊中，使用射線的碰撞點
                float distance = Vector3.Distance(weaponCollider.bounds.center, hitInfo.point);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = hitInfo.point;
                    Debug.Log("[PointBlur] 射線擊中碰撞體 " + col.name + "，碰撞點: " + hitInfo.point + "，距離: " + distance);
                }
            }
            else
            {
                // 射線檢測失敗，使用 ClosestPoint 作為備用
                float distance = Vector3.Distance(weaponCollider.bounds.center, closestPointOnCollider);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = closestPointOnCollider;
                    Debug.Log("[PointBlur] 射線未擊中，使用 ClosestPoint 作為備用，碰撞體: " + col.name + "，碰撞點: " + closestPointOnCollider + "，距離: " + distance);
                }
            }
        }
        
        if (closestPoint.HasValue)
        {
            Debug.Log("[PointBlur] 找到最近碰撞點: " + closestPoint.Value + ", 距離: " + closestDistance);
        }
        else
        {
            Debug.Log("[PointBlur] 未找到碰撞點");
        }
        
        return closestPoint; // 返回最近的碰撞點
    }

    // 使用射線檢測作為備用
    private void UseRaycastFallback()
    {
        // Debug.Log("[PointBlur] 使用射線檢測備用方案");
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)); // 從螢幕中心發射射線
        if(Physics.Raycast(ray, out hit)) // 如果射線擊中物體
        {
            // Debug.Log("[PointBlur] 射線擊中物體: " + hit.point);
            if (Spark != null)
            {
                GameObject effect = Instantiate(Spark, new Vector3(hit.point.x, hit.point.y, 0), Quaternion.identity); // 在擊中點生成火花特效
                // Debug.Log("[PointBlur] 生成備用 Parry 特效: " + effect.name);
            }
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(new Vector3(hit.point.x, hit.point.y, 0));
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height);
        }
        else // 如果射線沒有擊中任何物體，在主角前方生成特效
        {
            // Debug.Log("[PointBlur] 射線未擊中物體，在主角前方生成特效");
            
            // 獲取主角位置和方向
            Vector3 playerPosition = tpController != null ? tpController.transform.position : transform.position;
            Vector3 playerForward = tpController != null ? tpController.transform.forward : transform.forward;
            
            // 在主角前方生成特效
            Vector3 spawnPosition = playerPosition + playerForward * 2f; // 距離主角2單位
            
            if (Spark != null)
            {
                GameObject effect = Instantiate(Spark, spawnPosition, Quaternion.identity);
                // Debug.Log("[PointBlur] 在主角前方生成備用 Parry 特效: " + effect.name);
            }
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(spawnPosition);
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height);
        }
    }
}
