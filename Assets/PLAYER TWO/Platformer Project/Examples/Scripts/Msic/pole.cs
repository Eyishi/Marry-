using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(CapsuleCollider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Pole")]
	public class Pole : MonoBehaviour
	{
		/// <summary>
		/// 碰撞体
		/// </summary>
		public  new CapsuleCollider collider { get; protected set; }

		/// <summary>
		/// 半径
		/// </summary>
		public float radius => collider.radius;

		/// <summary>
		/// 中心
		/// </summary>
		public Vector3 center => transform.position;

		/// <summary>
		/// 玩家到 旗杆的方向
		/// </summary>
		/// <param name="other">玩家</param>
		/// <returns>方向</returns>
		public Vector3 GetDirectionToPole(Transform other) => GetDirectionToPole(other, out _);

		/// <summary>
		/// 面向这个杆子
		/// </summary>
		/// <param name="other">The transform you want to use.</param>
		/// <param name="distance">The distance from the pole center.</param>
		/// <returns>The direction from the Transform to the Pole.</returns>
		public Vector3 GetDirectionToPole(Transform other, out float distance)
		{
			//目标
			var target = new Vector3(center.x, other.position.y, center.z) - other.position;
			distance = target.magnitude;
			return target / distance;
		}

		/// <summary>
		/// 返回在旗杆上的点（限制范围）
		/// </summary>
		/// <param name="point"></param>
		/// <param name="offset">偏移</param>
		/// <returns>旗杆高度内的点.</returns>
		public Vector3 ClampPointToPoleHeight(Vector3 point, float offset)
		{
			var minHeight = collider.bounds.min.y + offset;
			var maxHeight = collider.bounds.max.y - offset;
			var clampedHeight = Mathf.Clamp(point.y, minHeight, maxHeight);
			return new Vector3(point.x, clampedHeight, point.z);
		}

		protected virtual void Awake()
		{
			tag = GameTags.Pole;
			collider = GetComponent<CapsuleCollider>();
		}
	}
}
