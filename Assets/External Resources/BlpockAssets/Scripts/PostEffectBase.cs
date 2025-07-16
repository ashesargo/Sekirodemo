using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 後處理效果的基礎類別
/// 提供後處理效果所需的基本功能，如資源檢查、材質創建等
/// </summary>
[ExecuteInEditMode] // 允許在編輯模式下執行
public class PostEffectBase : MonoBehaviour {

	/// <summary>
	/// 初始化時檢查資源
	/// </summary>
	protected void Start () {
        CheckResources();
	}
	
	/// <summary>
	/// 每幀更新（目前為空）
	/// </summary>
	void Update () {
		
	}

    /// <summary>
    /// 檢查後處理效果所需的資源是否可用
    /// </summary>
    protected void CheckResources()
    {
        bool isSupported = CheckSupport();
    }

    /// <summary>
    /// 檢查當前平台是否支援圖像效果
    /// </summary>
    /// <returns>如果支援返回true，否則返回false</returns>
    protected bool CheckSupport()
    {
        // 檢查系統是否支援圖像效果
        if (SystemInfo.supportsImageEffects == false )
        {
            Debug.LogWarning("not support image effect");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 當不支援後處理效果時禁用此組件
    /// </summary>
    protected void NotSupported()
    {
        enabled = false;
    }

    /// <summary>
    /// 檢查著色器並創建材質
    /// 如果著色器可用且材質不存在或著色器不匹配，則創建新的材質
    /// </summary>
    /// <param name="shader">要使用的著色器</param>
    /// <param name="material">現有的材質（可為null）</param>
    /// <returns>可用的材質，如果著色器不可用則返回null</returns>
    protected Material CheckShaderAndCreateMaterial(Shader shader,Material material)
    {
        // 如果著色器為空，返回null
        if (shader == null)
        {
            return null;
        }
        
        // 如果著色器支援且材質存在且著色器匹配，直接返回現有材質
        if (shader.isSupported && material && material.shader == shader)
        {
            return material;
        }

        // 如果著色器不支援，返回null
        if (!shader.isSupported)
        {
            return null;
        }
        else
        {
            // 創建新的材質並設置為不保存（避免在場景中保存）
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;

            // 檢查材質是否創建成功
            if (material)
            {
                return material;
            }
            else
            {
                return null;
            }

        }
    }

}
