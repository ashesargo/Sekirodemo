using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 後處理效果的基礎類別
/// </summary>
public class PostEffectBase : MonoBehaviour
{
    // 初始化時檢查資源
    protected void Start()
    {
        CheckResources();
    }

    // 檢查後處理效果所需的資源是否可用
    protected void CheckResources()
    {
        bool isSupported = CheckSupport();
    }

    // 檢查系統是否支援圖像效果
    protected bool CheckSupport()
    {
        if (SystemInfo.supportsImageEffects == false )
        {
            Debug.LogWarning("not support image effect");
            return false;
        }
        return true;
    }

    // 當不支援後處理效果時禁用此組件
    protected void NotSupported()
    {
        enabled = false;
    }

    // 檢查著色器並創建材質
    protected Material CheckShaderAndCreateMaterial(Shader shader,Material material)
    {
        if (shader == null) return null;
        
        if (shader.isSupported && material && material.shader == shader) return material;

        if (!shader.isSupported) return null;
        else
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;

            if (material) return material;
            else return null;
        }
    }

}
