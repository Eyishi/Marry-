using System.Collections;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Falling Platform")]
	public class FallingPlatform : MonoBehaviour, IEntityContact
	{
		public bool autoReset = true;
		public float fallDelay = 2f;//掉落延迟
		public float resetDelay = 5f;//重置延迟
		public float fallGravity = 40f;//掉落重力

		[Header("Shake Setting")]//晃动效果
		public bool shake = true;
		public float speed = 45f;
		public float height = 0.1f;

		protected Collider m_collider;
		protected Vector3 m_initialPosition; //初始位置

		protected Collider[] m_overlaps = new Collider[32]; //记录一下周围的碰撞

		/// <summary>
		/// 是否激活
		/// </summary>
		public bool activated { get; protected set; }

		/// <summary>
		/// 平台是否坠落
		/// </summary>
		public bool falling { get; protected set; }

		/// <summary>
		/// 坠落
		/// </summary>
		public virtual void Fall()
		{
			falling = true;
			m_collider.isTrigger = true;
		}

		/// <summary>
		/// 重置
		/// </summary>
		public virtual void Restart()
		{
			activated = falling = false;
			transform.position = m_initialPosition;
			m_collider.isTrigger = false;
			OffsetPlayer();
		}

		public void OnEntityContact(Entity entity)
		{
			if (entity is Player && entity.IsPointUnderStep(m_collider.bounds.max))//玩家在台子之上
			{
				if (!activated)
				{
					activated = true;
					StartCoroutine(Routine());
				}
			}
		}

		//偏移玩家
		protected virtual void OffsetPlayer()
		{
			var center = m_collider.bounds.center;
			var extents = m_collider.bounds.extents;
			var maxY = m_collider.bounds.max.y;
			//检测周围的碰撞物
			var overlaps = Physics.OverlapBoxNonAlloc(center, extents, m_overlaps);

			for (int i = 0; i < overlaps; i++)
			{
				if (!m_overlaps[i].CompareTag(GameTags.Player))
					continue;

				var distance = maxY - m_overlaps[i].transform.position.y;
				var height = m_overlaps[i].GetComponent<Player>().height;
				var offset = Vector3.up * (distance + height * 0.5f);

				m_overlaps[i].transform.position += offset;
			}
		}

		protected IEnumerator Routine()
		{
			var timer = fallDelay;

			while (timer >= 0)
			{
				if (shake && (timer <= fallDelay / 2f))
				{
					//调整振幅
					var shake = Mathf.Sin(Time.time * speed) * height;
					transform.position = m_initialPosition + Vector3.up * shake;
				}

				timer -= Time.deltaTime;
				yield return null;
			}

			Fall();
			//等待一定时间重置
			if (autoReset)
			{
				yield return new WaitForSeconds(resetDelay);
				Restart();
			}
		}

		protected virtual void Start()
		{
			m_collider = GetComponent<Collider>();
			m_initialPosition = transform.position;
			tag = GameTags.Platform;
		}

		protected virtual void Update()
		{
			if (falling)
			{
				transform.position += Vector3.down * (fallGravity * Time.deltaTime);
			}
		}
	}
}
