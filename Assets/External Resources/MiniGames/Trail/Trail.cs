using System;
using System.Collections;
using UnityEngine;

namespace Tiny
{
	/// <summary>
	/// 軌跡效果組件 - 用於創建動態的軌跡線條效果
	/// 可以根據物件的移動路徑生成平滑的軌跡，支援圓角處理和循環連接
	/// </summary>
	public class Trail : MonoBehaviour
	{
		[SerializeField, Tooltip("The material to apply to the trail.")]
		private Material material = null; // 軌跡使用的材質

		[SerializeField, Tooltip("Define the lifetime of a point in the trail, in seconds.")]
		private float duration = 0.1f; // 軌跡中每個點的存活時間（秒）

		[SerializeField, Tooltip("Increase this value to make the trail corners appear rounder.")]
		private int corner = 1; // 圓角細分數量，增加此值可使軌跡轉角更圓滑

		[SerializeField, Tooltip("Enable this to connect the first and last positions of the line, and form a closed loop.")]
		private bool loop = false; // 是否啟用循環連接，將首尾點連接形成閉合迴路

		[SerializeField, Tooltip("The array of Vector3 points to connect.")]
		private Vector3[] points = new Vector3[] { new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, 1f) }; // 要連接的點陣列

		// 非序列化欄位 - 不會在 Inspector 中顯示
		[NonSerialized] GameObject trailGo = null; // 軌跡遊戲物件
		[NonSerialized] Mesh mesh = null; // 軌跡的網格

		[NonSerialized] Vector3[] vertices = null; // 頂點陣列
		[NonSerialized] Transform cacheTM = null; // 快取的變換組件

		[NonSerialized] int lastSegmentCount = -1; // 上次的段數
		[NonSerialized] int lastCorner = -1; // 上次的圓角設定
		[NonSerialized] int pointCount = -1; // 點數量
		[NonSerialized] float toCornerT = 0f; // 圓角插值參數

		Coroutine update = null; // 更新協程

		/// <summary>
		/// 要連接的 Vector3 點陣列
		/// </summary>
		public Vector3[] Points {
			get{ return points; }
			set{ points = value; }
		}

		/// <summary>
		/// 是否啟用循環連接 - 連接線條的首尾位置形成閉合迴路
		/// 需要至少3個點才能形成有效的循環
		/// </summary>
		public bool Loop {	get { return loop && points.Length >= 3; }	}

		/// <summary>
		/// 清除軌跡中的所有點
		/// 用於從新位置重新開始軌跡
		/// </summary>
		public void Clear()
		{
			if (!enabled || pointCount <= 1 || !trailGo)
				return;

			if (update != null)
				StopCoroutine(update);

			ClearVertices();

			update = StartCoroutine(PhysicsUpdate());
		}

		/// <summary>
		/// 初始化軌跡系統
		/// 創建軌跡遊戲物件、網格和渲染器
		/// </summary>
		private void Start()
		{
			cacheTM = transform; // 快取變換組件以提高效能

			// 創建軌跡遊戲物件，包含網格過濾器和渲染器
			trailGo = new GameObject(name + "Trail", typeof(MeshFilter), typeof(MeshRenderer));
			DontDestroyOnLoad(trailGo); // 場景切換時不銷毀

			// 創建並設定網格
			mesh = new Mesh { name = "Trail Effect" };
			mesh.MarkDynamic(); // 標記為動態網格以優化效能
			trailGo.GetComponent<MeshFilter>().sharedMesh = mesh;
			trailGo.layer = gameObject.layer; // 設定相同的層級

			// 設定渲染器
			MeshRenderer meshRenderer = trailGo.GetComponent<MeshRenderer>();
			meshRenderer.material = material;
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // 關閉陰影投射

			// 初始化軌跡系統
			Initialize((int)(duration / Time.fixedDeltaTime));
		}

		/// <summary>
		/// 清理資源 - 銷毀網格和遊戲物件
		/// </summary>
		private void OnDestroy()
		{
			if (mesh != null)
				DestroyImmediate(mesh);
			mesh = null;

			if (trailGo != null)
				DestroyImmediate(trailGo);
			trailGo = null;
		}

		/// <summary>
		/// 啟用時重新啟動軌跡
		/// </summary>
		private void OnEnable()
		{
			if (trailGo == null)
				return;

			trailGo.SetActive(true);
			Initialize((int)(duration / Time.fixedDeltaTime));
		}

		/// <summary>
		/// 停用時隱藏軌跡並停止更新
		/// </summary>
		private void OnDisable()
		{
			if (trailGo)
				trailGo.SetActive(false);

			if (update != null)
				StopCoroutine(update);
			update = null;
		}

		/// <summary>
		/// 設定頂點並處理圓角
		/// 使用 Catmull-Rom 樣條曲線創建平滑的圓角效果
		/// </summary>
		private void SetVerticesAndCorner()
		{
			int nextSegmentPoint = pointCount + (pointCount * corner);

			// 將現有頂點向後移動，為新頂點騰出空間
			Array.Copy(vertices, 0, vertices, nextSegmentPoint, vertices.Length - nextSegmentPoint);

			TransformVertices(); // 轉換頂點到世界座標

			int next2 = nextSegmentPoint * 2;
			int next3 = nextSegmentPoint * 3;

			// 為每個點創建圓角插值
			for (int x = -1; ++x < pointCount;)
			{
				Vector3 a = vertices[x];
				Vector3 b = vertices[x + nextSegmentPoint];
				Vector3 c = vertices[x + next2];
				Vector3 d = vertices[x + next3];

				// 在每個段之間插入圓角點
				for (int n = -1, index = pointCount + x; ++n < corner; index += pointCount)
				{
					float t = (n + 1) * toCornerT;
					vertices[index] = CatmullRomSpline(a, a, b, c, t);
					vertices[index + nextSegmentPoint] = CatmullRomSpline(a, b, c, d, t);
				}
			}
		}

		/// <summary>
		/// 設定頂點（無圓角處理）
		/// </summary>
		private void SetVertices()
		{
			// 將現有頂點向後移動
			Array.Copy(vertices, 0, vertices, pointCount, vertices.Length - pointCount);
			TransformVertices(); // 轉換頂點到世界座標
		}

		/// <summary>
		/// 物理更新協程 - 在 FixedUpdate 中更新軌跡
		/// </summary>
		private IEnumerator PhysicsUpdate()
		{
			YieldInstruction wait = new WaitForFixedUpdate();

			// 根據是否有圓角選擇更新方法
			Action action = corner > 0 ? SetVerticesAndCorner : SetVertices;

			while (true)
			{
				yield return wait;
				action(); // 執行頂點更新
				cacheTM.hasChanged = false; // 重置變換變更標記
			}
		}

		/// <summary>
		/// 後期更新 - 處理變換變更並更新網格
		/// </summary>
		private void LateUpdate()
		{
            if (cacheTM.hasChanged) // 檢查變換是否發生變更
				TransformVertices(); // 重新轉換頂點

			mesh.vertices = vertices; // 更新網格頂點
			mesh.RecalculateBounds(); // 重新計算邊界
		}

		/// <summary>
		/// 將本地座標點轉換為世界座標
		/// </summary>
		private void TransformVertices()
		{
			Matrix4x4 localToWorldMatrix = cacheTM.localToWorldMatrix;
			for (int i = -1; ++i < pointCount;)
				vertices[i] = localToWorldMatrix.MultiplyPoint3x4(points[i]);
		}

		/// <summary>
		/// 清除頂點 - 將所有段設定為當前位置
		/// </summary>
		private void ClearVertices()
		{
			TransformVertices(); // 先轉換當前頂點

			// 將所有後續段複製為當前位置
			for (int i = pointCount; i < vertices.Length; i += pointCount)
				Array.Copy(vertices, 0, vertices, i, pointCount);
		}		

		/// <summary>
		/// 初始化軌跡系統
		/// 創建網格、UV 座標和索引
		/// </summary>
		/// <param name="segment">段數 - 基於持續時間和固定時間步長計算</param>
		private void Initialize(int segment)
		{
			int corner = segment >= 3 ? this.corner : 0; // 至少需要3段才能使用圓角

			// 檢查是否需要重新初始化
			if (lastSegmentCount == segment && pointCount == points.Length && lastCorner == corner)
			{
				ClearVertices();
				update = StartCoroutine(PhysicsUpdate());
				return;
			}

			// 更新狀態
			pointCount = points.Length;
			lastCorner = corner;
			lastSegmentCount = segment;

			// 如果點數不足，清除網格
			if (pointCount <= 1)
			{
				mesh.Clear();
				return;
			}

			int segmentAndCorner = segment + (segment * corner); // 總段數（包含圓角）

			// 創建 UV 座標陣列
			Vector2[] uvs = new Vector2[pointCount * (segmentAndCorner + 1)];

			bool isLoop = Loop; // 是否為循環

			// 創建索引陣列 - 每個四邊形由兩個三角形組成（6個索引）
			int[] indexs = new int[(isLoop ? pointCount : pointCount - 1) * 6 * segmentAndCorner];

			Vector2 uv = new Vector2();

			int endPoint = pointCount - 1;

			float invSegment = 1f / segment; // 段數倒數
			float invEnd = 1f / endPoint; // 端點倒數
			toCornerT = 1f / (corner + 1); // 圓角插值參數

			// 生成 UV 座標
			for (int y = -1, i = -1; ++y <= segment;)
			{
				uv.y = y * invSegment;
				for (int x = -1; ++x < pointCount;)
				{
					uv.x = x * invEnd;
					uvs[++i] = uv;
				}

				if (y == segment)
					continue;

				// 為圓角段生成額外的 UV 座標
				for (int n = -1; ++n < corner;)
				{
					uv.y = Mathf.Lerp(y * invSegment, (y + 1) * invSegment, (n + 1) * toCornerT);

					for (int x = -1; ++x < pointCount;)
					{
						uv.x = x * invEnd;
						uvs[++i] = uv;
					}
				}
			}

			// 生成三角形索引
			int index = 0;
			int lineCount = isLoop ? endPoint+1 : endPoint; // 循環時需要額外一條線

			for (int y = -1; ++y < segmentAndCorner;)
			{
				int beginIndex = y * pointCount;
				int nextIndex = y * pointCount;
				if (isLoop)
					beginIndex += endPoint; // 循環時調整起始索引
				else
					nextIndex += 1;

				// 為每個四邊形創建兩個三角形
				for (int x = -1; ++x < lineCount; index += 6, beginIndex = nextIndex++)
				{
					// 第一個三角形
					indexs[index + 0] = beginIndex;
					indexs[index + 1] = beginIndex + pointCount;
					indexs[index + 2] = nextIndex;					
					// 第二個三角形
					indexs[index + 3] = nextIndex;
					indexs[index + 4] = beginIndex + pointCount;
					indexs[index + 5] = nextIndex + pointCount;					
				}
			}

			// 創建頂點陣列並初始化
			vertices = new Vector3[uvs.Length];
			ClearVertices();

			// 設定網格資料
			mesh.vertices = vertices;
			mesh.uv = uvs;
			mesh.SetIndices(indexs, MeshTopology.Triangles, 0);

			// 啟動更新協程
			update = StartCoroutine(PhysicsUpdate());
		}

		/// <summary>
		/// Catmull-Rom 樣條曲線插值
		/// 在 p1 和 p2 之間創建平滑的曲線
		/// 
		/// t == 0 時返回 p1，t == 1 時返回 p2
		/// 使用四個控制點創建平滑的樣條曲線
		/// </summary>
		/// <param name="p0">第一個控制點</param>
		/// <param name="p1">第二個控制點（起始點）</param>
		/// <param name="p2">第三個控制點（結束點）</param>
		/// <param name="p3">第四個控制點</param>
		/// <param name="t">插值參數 (0-1)</param>
		/// <returns>插值後的點位置</returns>
		static Vector3 CatmullRomSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			float t2 = t * t; // t 的平方
			float t3 = t2 * t; // t 的立方
			// Catmull-Rom 樣條曲線公式
			return 0.5f * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
		}
	}
}