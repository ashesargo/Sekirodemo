using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 點狀模糊後處理效果
/// 實現以特定點為中心的動態模糊效果，類似於相機對焦效果
/// </summary>
public class PointBlur : MonoBehaviour {
    /// <summary>
    /// 點狀模糊的著色器
    /// </summary>
    public Shader PointBlurShader;
    
    /// <summary>
    /// 模糊強度的動畫曲線，用於控制模糊的衰減
    /// </summary>
    public AnimationCurve curve;
    
    /// <summary>
    /// 點擊時產生的火花特效預製體
    /// </summary>
    public GameObject Spark;
    
    /// <summary>
    /// 後處理材質
    /// </summary>
    Material material;
    
    /// <summary>
    /// 模糊強度，範圍0-1
    /// </summary>
    [Range(0, 1)]
    public float BlurStrength = 0.5f;

    /// <summary>
    /// 模糊中心點（螢幕座標，0-1範圍）
    /// </summary>
    private Vector2 BlurCenter = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// 梯度貼圖，用於存儲模糊衰減曲線
    /// </summary>
    private Texture2D gradTexture;
    
    /// <summary>
    /// 初始化函數
    /// </summary>
    private void Start()
    {
        // 初始化梯度貼圖（將waveform曲線轉換為貼圖）
        gradTexture = new Texture2D(2048, 1, TextureFormat.Alpha8, false);
        gradTexture.wrapMode = TextureWrapMode.Clamp;
        gradTexture.filterMode = FilterMode.Bilinear;
        
        // 將動畫曲線的值寫入貼圖
        for (var i = 0; i < gradTexture.width; i++)
        {
            var x = 1.0f / gradTexture.width * i;
            var a = curve.Evaluate(x);
            gradTexture.SetPixel(i, 0, new Color(a, a, a, a));
        }
        gradTexture.Apply();

        // 初始化後處理材質
        material = new Material(PointBlurShader);
        material.hideFlags = HideFlags.DontSave;
        material.SetTexture("_GradTex", gradTexture);
    }
    
    /// <summary>
    /// 降採樣因子，用於提高性能
    /// </summary>
    public int downSampleFactor = 2;
    
    /// <summary>
    /// 後處理渲染函數
    /// </summary>
    /// <param name="source">原始渲染紋理</param>
    /// <param name="destination">目標渲染紋理</param>
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // 創建降採樣的臨時渲染紋理以提高性能
        RenderTexture rt1 = RenderTexture.GetTemporary(source.width >> downSampleFactor, source.height >> downSampleFactor, 0, source.format);
        RenderTexture rt2 = RenderTexture.GetTemporary(source.width >> downSampleFactor, source.height >> downSampleFactor, 0, source.format);
        
        // 將原始圖像複製到降採樣紋理
        Graphics.Blit(source, rt1);

        // 在降採樣紋理上進行模糊處理（Pass 0）
        Graphics.Blit(rt1, rt2, material, 0);

        // 將模糊結果與原始圖像進行混合（Pass 1）
        material.SetTexture("_BlurTex", rt2);
        Graphics.Blit(source, destination, material, 1);

        // 釋放臨時渲染紋理
        RenderTexture.ReleaseTemporary(rt1);
        RenderTexture.ReleaseTemporary(rt2);
    }
    
    /// <summary>
    /// 時間計數器
    /// </summary>
    float t = 1000;
    
    /// <summary>
    /// 模糊動畫速度
    /// </summary>
    public float BlurSpeed = 1;
    
    /// <summary>
    /// 模糊範圍
    /// </summary>
    public float BlurRange = 0.3f;
    
    /// <summary>
    /// 模糊圓圈半徑
    /// </summary>
    public float BlurRadius = 1;
    
    /// <summary>
    /// 每幀更新模糊參數
    /// </summary>
    private void Update()
    {
        // 更新材質中的各種參數
        material.SetFloat("_Timer", t += Time.deltaTime);
        material.SetFloat("_BlurSpeed", BlurSpeed);
        material.SetFloat("_BlurStrength", BlurStrength);
        material.SetFloat("_BlurRange", BlurRange);
        material.SetVector("_BlurCenter", new Vector4(BlurCenter.x* Camera.main.aspect, BlurCenter.y, 0, 0));
        material.SetFloat("_Aspect", Camera.main.aspect);
        material.SetFloat("_BlurCircleRadius", BlurRadius);

        // 檢測滑鼠左鍵點擊
        if (Input.GetMouseButtonDown(0))
        {
            // 重置時間計數器
            t = 0;
            
            // 使用射線檢測來獲取點擊的世界座標
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray,out hit))
            {
                // 在點擊位置生成火花特效
                Instantiate(Spark, new Vector3(hit.point.x,hit.point.y,0), Quaternion.identity);
                
                // 將世界座標轉換為螢幕座標，再轉換為0-1範圍的UV座標
                BlurCenter = Camera.main.WorldToScreenPoint(new Vector3(hit.point.x, hit.point.y, 0));
                BlurCenter.Set(BlurCenter.x / Screen.width, BlurCenter.y / Screen.height);
            }
        }
    }
}
