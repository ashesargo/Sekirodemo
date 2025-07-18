using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointBlur : MonoBehaviour {
    public Shader PointBlurShader; // 點狀模糊著色器
    public AnimationCurve curve; // 模糊強度動畫曲線
    public GameObject Spark; // 火花特效預製體
    public TPContraller tpController; // 玩家控制器引用
    public Collider weaponCollider; // 武器碰撞器
    public LayerMask detectionLayer = -1; // 碰撞檢測層級
    Material material; // 後處理材質
    
    [Range(0, 1)]
    public float BlurStrength = 0.5f; // 模糊強度

    private Vector2 BlurCenter = new Vector2(0.5f, 0.5f); // 模糊中心點（螢幕UV座標）
    private Texture2D gradTexture; // 梯度貼圖
    

    private void Start()
    {
        // 創建梯度貼圖用於模糊衰減
        gradTexture = new Texture2D(2048, 1, TextureFormat.Alpha8, false);
        gradTexture.wrapMode = TextureWrapMode.Clamp;
        gradTexture.filterMode = FilterMode.Bilinear;
        
        // 將動畫曲線轉換為貼圖數據
        for (var i = 0; i < gradTexture.width; i++)
        {
            var x = 1.0f / gradTexture.width * i; // 計算UV座標
            var a = curve.Evaluate(x); // 從動畫曲線獲取值
            gradTexture.SetPixel(i, 0, new Color(a, a, a, a)); // 設置像素值
        }
        gradTexture.Apply(); // 應用貼圖更改

        // 初始化後處理材質
        material = new Material(PointBlurShader);
        material.hideFlags = HideFlags.DontSave; // 不在場景中保存材質
        material.SetTexture("_GradTex", gradTexture); // 設置梯度貼圖
        
        // 自動尋找TPController
        if (tpController == null)
        {
            tpController = FindObjectOfType<TPContraller>();
        }
        
        // 自動尋找武器碰撞器
        if (weaponCollider == null)
        {
            if (tpController != null)
            {
                weaponCollider = tpController.GetComponentInChildren<Collider>(); // 從玩家控制器子物件尋找
            }
            
            if (weaponCollider == null)
            {
                weaponCollider = FindObjectOfType<WeaponEffect>()?.weaponCollider; // 從WeaponEffect組件尋找
            }
        }
    }
    
    public int downSampleFactor = 2; // 降採樣因子，提高性能
    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // 創建降採樣的臨時渲染紋理
        RenderTexture rt1 = RenderTexture.GetTemporary(source.width >> downSampleFactor, source.height >> downSampleFactor, 0, source.format);
        RenderTexture rt2 = RenderTexture.GetTemporary(source.width >> downSampleFactor, source.height >> downSampleFactor, 0, source.format);
        
        Graphics.Blit(source, rt1); // 複製原始圖像到降採樣紋理
        Graphics.Blit(rt1, rt2, material, 0); // 執行模糊處理（Pass 0）
        material.SetTexture("_BlurTex", rt2); // 設置模糊紋理
        Graphics.Blit(source, destination, material, 1); // 混合原始圖像和模糊結果（Pass 1）

        RenderTexture.ReleaseTemporary(rt1); // 釋放臨時紋理
        RenderTexture.ReleaseTemporary(rt2);
    }
    
    float t = 1000; // 時間計數器
    private bool lastParrySuccess = false; // 記錄上一次的格擋成功狀態
    public float BlurSpeed = 1; // 模糊動畫速度
    public float BlurRange = 0.3f; // 模糊範圍
    public float BlurRadius = 1; // 模糊圓圈半徑
    
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
    
    private void Update()
    {
        // 更新材質參數
        material.SetFloat("_Timer", t += Time.deltaTime); // 更新時間
        material.SetFloat("_BlurSpeed", BlurSpeed); // 設置模糊速度
        material.SetFloat("_BlurStrength", BlurStrength); // 設置模糊強度
        material.SetFloat("_BlurRange", BlurRange); // 設置模糊範圍
        material.SetVector("_BlurCenter", new Vector4(BlurCenter.x* Camera.main.aspect, BlurCenter.y, 0, 0)); // 設置模糊中心（考慮螢幕比例）
        material.SetFloat("_Aspect", Camera.main.aspect); // 設置螢幕比例
        material.SetFloat("_BlurCircleRadius", BlurRadius); // 設置模糊圓圈半徑

        if (tpController != null)
        {
            // 檢測格擋成功狀態變化（從false變為true時觸發）
            if (tpController.parrySuccess && !lastParrySuccess)
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
            
            lastParrySuccess = tpController.parrySuccess; // 更新上一次的格擋狀態
        }
    }
}
