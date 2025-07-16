// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/// <summary>
/// 徑向模糊著色器
/// 實現以特定點為中心的徑向模糊效果，包含爆炸特效
/// </summary>
Shader "Hidden/RacialBlur"
{
	/// <summary>
	/// 著色器屬性定義
	/// </summary>
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}           // 主紋理（原始圖像）
		_GradTex ("_GradTexture", 2D) = "white" {}      // 梯度紋理（模糊衰減曲線）
		_BlurTex("BlurTexture",2d)="white"{}            // 模糊紋理（Pass 0的結果）
		_BlurCenter("BlurCenter",vector)=(0.5,0.5,0,0) // 模糊中心點
		_BlurStrength("BlurStrength",float)=0.5         // 模糊強度
		_BlurRange("BlurRange",float)=0.3               // 模糊範圍
		_Timer("Timer",float)=1000                      // 時間計數器
		_BlurSpeed("BlurSpeed",float)=1                 // 模糊動畫速度
		_Aspect("Aspect",float)=1                       // 螢幕寬高比
		_BlurCircleRadius("_BlurCircleRadius",float)=1  // 模糊圓圈半徑
	}

	/// <summary>
	/// 共用的著色器代碼
	/// </summary>
	CGINCLUDE
			#include "UnityCG.cginc"
			
			/// <summary>
			/// 頂點著色器輸入結構
			/// </summary>
			struct appdata
			{
				float4 vertex : POSITION;  // 頂點位置
				float2 uv : TEXCOORD0;     // UV座標
			};

			/// <summary>
			/// 頂點著色器輸出結構
			/// </summary>
			struct v2f
			{
				float2 uv : TEXCOORD0;     // UV座標
				float4 vertex : SV_POSITION; // 裁剪空間位置
			};

			/// <summary>
			/// 頂點著色器（Pass 0）
			/// </summary>
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex); // 轉換到裁剪空間
				o.uv = v.uv;
				return o;
			}
			
			// 紋理和參數聲明
			sampler2D _MainTex;           // 主紋理
			sampler2D _GradTex;           // 梯度紋理
			sampler2D _BlurTex;           // 模糊紋理
			half4 _MainTex_TexelSize;     // 主紋理的像素大小
			float _BlurStrength;          // 模糊強度
			float2 _BlurCenter;           // 模糊中心點
			float _Timer;                 // 時間計數器
			float _BlurSpeed;             // 模糊速度
			float _BlurCircleRadius;      // 模糊圓圈半徑
			float _BlurRange;             // 模糊範圍
			float _Aspect;                // 螢幕寬高比

			/// <summary>
			/// 計算模糊百分比
			/// 根據距離和時間計算當前像素的模糊程度
			/// </summary>
			/// <param name="pos">當前像素位置</param>
			/// <returns>模糊百分比（0-1）</returns>
			float GetBlurPercent(float2 pos){
				// 計算基於距離和時間的模糊因子
				float t=_Timer-length(pos-_BlurCenter)/_BlurSpeed;
				t*=1/_BlurCircleRadius;
				// 從梯度紋理中取樣得到模糊百分比
				return tex2D(_GradTex,float2(t,0)).a;
			}

			/// <summary>
			/// 計算爆炸特效顏色
			/// 在模糊中心點產生黃色光暈效果
			/// </summary>
			/// <param name="pos">當前像素位置</param>
			/// <returns>爆炸特效顏色</returns>
			fixed4 GetExplosionColor(float2 pos){
				fixed4 yellow = fixed4(1,0.5,0,1); // 黃色
				float t=_Timer*5;

				// 計算爆炸強度：時間衰減 * 距離衰減
				return yellow*max(-1*t*t+t,0)*min(0.05/length(pos-_BlurCenter),3 );
			}

			/// <summary>
			/// 片段著色器（Pass 0）- 徑向模糊
			/// 實現以中心點為基準的徑向模糊效果
			/// </summary>
			fixed4 frag (v2f i) : SV_Target
			{
				// 調整UV座標以適應螢幕寬高比
				float2 p=i.uv*float2(_Aspect,1);
				// 計算從中心點到當前像素的方向向量
				float2 dir =normalize(p-_BlurCenter);
				// 將方向向量縮放到紋理像素大小
				dir*=_MainTex_TexelSize.xy;

				// 在徑向方向上進行多次採樣，實現模糊效果
				fixed4 col =tex2D(_MainTex,i.uv-dir*1) ;  // 距離1的採樣
				col +=tex2D(_MainTex,i.uv-dir*2) ;        // 距離2的採樣
				col +=tex2D(_MainTex,i.uv-dir*3) ;        // 距離3的採樣
				col +=tex2D(_MainTex,i.uv-dir*5) ;        // 距離5的採樣
				col +=tex2D(_MainTex,i.uv-dir*8) ;        // 距離8的採樣
				col +=tex2D(_MainTex,i.uv+dir*1) ;        // 反方向距離1的採樣
				col +=tex2D(_MainTex,i.uv+dir*2) ;        // 反方向距離2的採樣
				col +=tex2D(_MainTex,i.uv+dir*3) ;        // 反方向距離3的採樣
				col +=tex2D(_MainTex,i.uv+dir*5) ;        // 反方向距離5的採樣
				col +=tex2D(_MainTex,i.uv+dir*8) ;        // 反方向距離8的採樣
				col *=0.1; // 平均化（10個採樣點）

				return col;
			}

			/// <summary>
			/// 混合用的頂點著色器輸出結構
			/// </summary>
			struct v2f_lerp
			{
				float4 pos : SV_POSITION;    // 裁剪空間位置
				float2 uv1 : TEXCOORD0;      // 原始圖像UV
				float2 uv2 : TEXCOORD1;      // 模糊圖像UV
			};

			/// <summary>
			/// 混合用的頂點著色器（Pass 1）
			/// </summary>
			v2f_lerp vert_mix(appdata_img  v){
				v2f_lerp o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv1 = v.texcoord.xy;
				o.uv2 = v.texcoord.xy;
				
				// 處理DirectX平台的UV座標差異
				// DirectX中紋理從左上角開始，需要反向
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv2.y = 1 - o.uv2.y;
				#endif
				return o;
			}

			/// <summary>
			/// 混合用的片段著色器（Pass 1）
			/// 將原始圖像和模糊圖像進行混合，並添加爆炸特效
			/// </summary>
			fixed4 frag_mix (v2f_lerp i) : SV_Target{
				// 取樣原始圖像和模糊圖像
				fixed4 rawCol = tex2D(_MainTex, i.uv1);
				fixed4 blurCol = tex2D(_BlurTex, i.uv2);
				
				// 計算混合因子
				float blendFactor = _BlurStrength
					* GetBlurPercent(i.uv1*float2(_Aspect,1))  // 獲取當前像素在曲線上的值
					* min(_BlurRange/length(i.uv1*float2(_Aspect,1)-_BlurCenter),1); // 範圍限制
				
				// 線性插值混合原始圖像和模糊圖像
				fixed4 col = lerp(rawCol,blurCol,blendFactor);
				
				// 添加黃色爆炸光暈效果
				col+=GetExplosionColor(i.uv1*float2(_Aspect,1));
				return col;
			}

	ENDCG

	/// <summary>
	/// 子著色器定義
	/// </summary>
	SubShader
	{
		// 關閉背面剔除、深度寫入和深度測試
		Cull Off ZWrite Off ZTest Always

		/// <summary>
		/// Pass 0: 徑向模糊處理
		/// 在降採樣的紋理上進行徑向模糊計算
		/// </summary>
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			ENDCG
		}

		/// <summary>
		/// Pass 1: 圖像混合
		/// 將原始圖像與模糊結果混合，並添加爆炸特效
		/// </summary>
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_mix
			#pragma fragment frag_mix
			
			#include "UnityCG.cginc"

			ENDCG
		}
	}
}
