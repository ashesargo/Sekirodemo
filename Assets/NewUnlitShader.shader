// 這是一個 Unlit（無光照）Shader 範例，適合用來學習 Unity Shader 的基本結構
Shader "Custom/NewUnlitShader"
{
    // Properties 區塊：這裡定義可以在 Inspector 面板調整的參數
    Properties
    {
        // 主貼圖，預設為白色
        _MainTex ("Texture", 2D) = "white" {}
        // 顏色屬性，可用於調整整體色調
        _Color ("顏色", Color) = (1,1,1,1)
        // 浮點數屬性，可用於自訂參數
        _FloatValue ("浮點數", Float) = 0.5
        // 向量屬性，可用於自訂向量參數
        _VectorValue ("向量", Vector) = (1,0,0,0)
        // 切換開關（布林值），0=關，1=開
        [Toggle] _UseEffect ("啟用特效", Float) = 0
        // 範圍滑桿，常用於強度、透明度等
        _Range ("顏色強度", Range(0, 2)) = 1
        // 額外貼圖範例
        _SecondTex ("第二張貼圖", 2D) = "black" {}
    }
    // SubShader 區塊：定義實際的渲染流程
    SubShader
    {
        // 設定渲染標籤與 LOD（細節等級）
        Tags 
        { 
            "RenderType"="Opaque" // 渲染類型
            "Rendering Order"="100" // 渲染順序
            "Queue"="Transparent" // 渲染隊列
            "IgnoreProjector"="True" // 忽略投影
        }
        LOD 100

        // Pass 區塊：一個渲染通道，這裡只用一個 Pass
        Pass
        {
            
            Stencil // 
            {
                Ref 1
                Comp Always
                Pass Replace
            }   
            Cull Off // 不裁剪面，讓所有面都渲染
            ZWrite Off // 不寫入深度
            ZTest Always // 不測試深度，讓所有像素都渲染
            Blend SrcAlpha OneMinusSrcAlpha // 源顏色乘以源透明度，目標顏色乘以1減去源透明度
            // 這個 Blend 設定表示：
            // 1. 源顏色（Src）乘以源透明度（SrcAlpha）
            // 2. 目標顏色（Dst）乘以1減去源透明度（OneMinusSrcAlpha）
            // 3. 最後將兩者相加
            // 這樣做可以實現透明效果，源顏色會隨著透明度變化，而目標顏色則保持不變
            // 適用於需要混合透明物體的場景，例如玻璃、水、煙霧等

            CGPROGRAM // 開始寫 CG 程式碼
            #pragma vertex vert // 指定頂點著色器函數
            #pragma fragment frag // 指定片元著色器函數

            // 讓 Shader 支援 Unity 的霧效
            #pragma multi_compile_fog

            // 引入 Unity 的常用函式庫
            #include "UnityCG.cginc"

            // appdata 結構：定義頂點著色器的輸入
            struct appdata
            {
                float4 vertex : POSITION; // 頂點座標
                float2 uv : TEXCOORD0;   // UV 座標
            };

            // v2f 結構：頂點著色器的輸出、片元著色器的輸入
            struct v2f
            {
                float2 uv : TEXCOORD0; // UV 座標
                UNITY_FOG_COORDS(1)   // 霧效所需的座標
                float4 vertex : SV_POSITION; // 裝置座標
            };

            // 宣告屬性對應的變數
            sampler2D _MainTex;        // 主貼圖
            float4 _MainTex_ST;        // 主貼圖的 Tiling/Offset
            float4 _Color;             // 顏色
            float _FloatValue;         // 浮點數
            float4 _VectorValue;       // 向量
            float _UseEffect;          // Toggle 開關
            float _Range;              // 範圍滑桿
            sampler2D _SecondTex;      // 第二張貼圖

            // 頂點著色器：負責將模型空間的頂點轉換到螢幕空間
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 轉換到裁剪空間
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);      // 處理 Tiling/Offset
                UNITY_TRANSFER_FOG(o,o.vertex);            // 計算霧效
                return o;
            }

            // 片元著色器：決定每個像素的顏色，並示範如何使用各種 Properties
            fixed4 frag (v2f i) : SV_Target
            {
                // 取樣主貼圖顏色
                fixed4 col = tex2D(_MainTex, i.uv);
                // 1. 乘上顏色屬性
                col *= _Color;
                // 2. 加上浮點數參數，讓顏色整體偏亮
                col.rgb += _FloatValue;
                // 3. 用向量參數的 x 分量調整紅色強度
                col.r *= _VectorValue.x;
                // 4. Toggle 開關，啟用時反相顏色
                if (_UseEffect > 0.5) {
                    col.rgb = 1 - col.rgb;
                }
                // 5. 用 Range 參數調整顏色強度
                col.rgb *= _Range;
                // 6. 取樣第二張貼圖並混合
                fixed4 col2 = tex2D(_SecondTex, i.uv);
                col = lerp(col, col2, 0.5); // 兩張貼圖混合
                // 7. 套用霧效
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG // 結束 CG 程式碼
        }
    }
}
