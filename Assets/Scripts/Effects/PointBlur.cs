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
    public GameObject Spark; // 火花特效 Prefab
    
    [Header("Blur Effect")]
    [Range(0, 1)]
    public float BlurStrength = 0.5f; // 徑向模糊強度
    public float BlurSpeed = 1; // 模糊動畫速度
    public float BlurRange = 0.3f; // 模糊範圍
    public float BlurRadius = 1; // 模糊圓圈半徑
    public int downSampleFactor = 2; // 降採樣因子，提高性能

    [Header("Player Settings")]
    public TPContraller tpController; // 引用玩家控制器
    public Collider weaponCollider; // 武器 Collider
    public LayerMask detectionLayer = -1; // 碰撞檢測層級

    private Material material; // 後處理材質
    private Vector2 BlurCenter = new Vector2(0.5f, 0.5f); // 徑向模糊螢幕座標中心點
    private Texture2D gradTexture; // 梯度貼圖
    private float t = 1000; // 時間計數器
    private bool lastParrySuccess = false; // 記錄上一次的格擋成功狀態

    void Start()
    {
        InitializeGradientTexture();
        InitializeMaterial();
    }

    void Update()
    {
        UpdateMaterialParameters();
        CheckParrySuccess();
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

    // 檢查格擋成功狀態
    private void CheckParrySuccess()
    {
        if (tpController != null)
        {
            // 檢測格擋成功狀態變化（從false變為true時觸發）
            if (tpController.parrySuccess && !lastParrySuccess)
            {
                TriggerBlurEffect();
            }
            
            lastParrySuccess = tpController.parrySuccess; // 更新上一次的格擋狀態
        }
    }

    // 觸發徑向模糊效果
    private void TriggerBlurEffect()
    {
        t = 0; // 重置時間計數器
        
        // 計算武器碰撞最近點
        Vector3? collisionPoint = CalculateClosestCollisionPoint();
        
        if (collisionPoint.HasValue) // 如果找到碰撞點
        {
            Instantiate(Spark, collisionPoint.Value, Quaternion.identity); // 在碰撞點生成火花特效
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(collisionPoint.Value); // 世界座標轉螢幕座標
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height); // 螢幕座標轉UV座標
        }
        else // 如果沒有找到碰撞點，使用射線檢測作為備用
        {
            UseRaycastFallback();
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
        if (weaponCollider == null) return null; // 如果沒有武器碰撞器則返回null
        
        // 使用武器碰撞器範圍檢測所有碰撞體
        Collider[] hitColliders = Physics.OverlapBox(
            weaponCollider.bounds.center, // 碰撞器中心
            weaponCollider.bounds.extents, // 碰撞器半徑
            weaponCollider.transform.rotation, // 碰撞器旋轉
            detectionLayer // 檢測層級
        );
        
        Vector3? closestPoint = null; // 最近的碰撞點
        float closestDistance = float.MaxValue; // 最近距離
        
        foreach (Collider col in hitColliders)
        {
            Vector3 collisionPoint = col.ClosestPoint(weaponCollider.bounds.center); // 計算碰撞體到武器中心的最近點
            float distance = Vector3.Distance(weaponCollider.bounds.center, collisionPoint); // 計算距離
            
            if (distance < closestDistance) // 如果這個點更近
            {
                closestDistance = distance; // 更新最近距離
                closestPoint = collisionPoint; // 更新最近點
            }
        }
        
        return closestPoint; // 返回最近的碰撞點
    }

    // 使用射線檢測作為備用
    private void UseRaycastFallback()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)); // 從螢幕中心發射射線
        if(Physics.Raycast(ray, out hit)) // 如果射線擊中物體
        {
            Instantiate(Spark, new Vector3(hit.point.x, hit.point.y, 0), Quaternion.identity); // 在擊中點生成火花特效
            
            // 將世界座標轉換為螢幕UV座標
            BlurCenter = Camera.main.WorldToScreenPoint(new Vector3(hit.point.x, hit.point.y, 0));
            BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height);
        }
        else // 如果射線沒有擊中任何物體
        {
            BlurCenter = new Vector2(0.5f, 0.5f); // 使用螢幕中心作為模糊中心
        }
    }
}
